using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaStackCore.Controllers;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Services.UnitOfWorkService;

namespace MediaStack_Importer.Services.MonitorService
{
    /// <summary>
    ///     Monitors for changes on disk and updates
    ///     the DAL accordingly.
    /// </summary>
    public class MediaMonitor : BaseImporterService
    {
        #region Data members

        protected IMediaFileSystemController FSController;

        protected IUnitOfWorkService UnitOfWorkService;

        protected FileSystemWatcher watcher;

        private readonly object controllerLock = new();

        private readonly List<string> ignoreExtensionList = new() {
            ".part", ".crdownload"
        };

        #endregion

        #region Constructors

        public MediaMonitor(IMediaFileSystemController fsController, IUnitOfWorkService unitOfWorkService) : base(
            fsController)
        {
            this.FSController = fsController;
            this.UnitOfWorkService = unitOfWorkService;
        }

        #endregion

        #region Methods

        public async Task Start()
        {
            this.watcher = new FileSystemWatcher(this.FSController.MediaDirectory);

            this.watcher.NotifyFilter = NotifyFilters.Attributes
                                        | NotifyFilters.CreationTime
                                        | NotifyFilters.DirectoryName
                                        | NotifyFilters.FileName
                                        | NotifyFilters.LastWrite
                                        | NotifyFilters.Security
                                        | NotifyFilters.Size;

            this.watcher.Changed += this.OnChanged;
            this.watcher.Created += this.OnCreated;
            this.watcher.Deleted += this.OnDeleted;
            this.watcher.Renamed += this.OnRenamed;

            this.watcher.IncludeSubdirectories = true;
            this.watcher.EnableRaisingEvents = true;

            Console.WriteLine("Ready");

            try
            {
                Thread.Sleep(Timeout.Infinite);
            }
            catch (ThreadInterruptedException)
            {
                this.watcher.Dispose();
            }
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Task.Run(() =>
            {
                if (e.ChangeType != WatcherChangeTypes.Changed || Directory.Exists(e.FullPath) ||
                    this.ignoreExtensionList.Contains(Path.GetExtension(e.FullPath)))
                {
                    return;
                }

                Console.WriteLine($"Changed: {e.FullPath}");
                try
                {
                    using (var unitOfWork = this.UnitOfWorkService.Create())
                    {
                        CreateOrUpdateMediaFromFile(e.FullPath, unitOfWork);
                        unitOfWork.Save();
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("Couldn't Read...");
                }
            });
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            Task.Run(() =>
            {
                if (Directory.Exists(e.FullPath))
                {
                    return;
                }

                Console.WriteLine($"Created: {e.FullPath}");
                try
                {
                    using (var unitOfWork = this.UnitOfWorkService.Create())
                    {
                        CreateOrUpdateMediaFromFile(e.FullPath, unitOfWork);
                        unitOfWork.Save();
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("Couldn't Read...");
                }
            });
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Task.Run(() =>
            {
                Console.WriteLine($"Deleted: {e.FullPath}");
                using (var unitOfWork = this.UnitOfWorkService.Create())
                {
                    var mediaPath = e.FullPath.Replace(this.FSController.MediaDirectory, "");
                    var media = unitOfWork.Media.Get(media => media.Path == mediaPath).FirstOrDefault();
                    if (media != null)
                    {
                        unitOfWork.DisableMedia(media);
                        unitOfWork.Media.Update(media);
                        unitOfWork.Save();
                    }
                    else
                    {
                        foreach (var m in unitOfWork.Media.Get(media =>
                            media.Path.ToLower().StartsWith(mediaPath.ToLower())))
                        {
                            unitOfWork.DisableMedia(m);
                        }

                        unitOfWork.Save();
                    }
                }
            });
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            Task.Run(() =>
            {
                if (this.ignoreExtensionList.Contains(Path.GetExtension(e.FullPath)))
                {
                    return;
                }

                Console.WriteLine("Renamed:");
                Console.WriteLine($"    Old: {e.OldFullPath}");
                Console.WriteLine($"    New: {e.FullPath}");
                using (var unitOfWork = this.UnitOfWorkService.Create())
                {
                    if (File.Exists(e.FullPath))
                    {
                        CreateOrUpdateMediaFromFile(e.FullPath, unitOfWork);
                        unitOfWork.Save();
                    }
                    else if (Directory.Exists(e.FullPath))
                    {
                        var newPath = e.OldFullPath.Last() == Path.DirectorySeparatorChar
                            ? e.OldFullPath + "media"
                            : e.OldFullPath + Path.DirectorySeparatorChar + "media";
                        var oldPath = e.FullPath.Last() == Path.DirectorySeparatorChar
                            ? e.FullPath + "media"
                            : e.FullPath + Path.DirectorySeparatorChar + "media";

                        var filePaths = Directory.GetFiles(this.FSController.MediaDirectory, "*",
                            SearchOption.AllDirectories);

                        var counter = 0;
                        var counterLock = new object();

                        using (var resetEvent = new ManualResetEvent(false))
                        {
                            foreach (var filePath in filePaths)
                            {
                                ThreadPool.QueueUserWorkItem(callBack =>
                                {
                                    try
                                    {
                                        CreateOrUpdateMediaFromFile(filePath, unitOfWork, counterLock);
                                    }
                                    catch (Exception)
                                    {
                                    }
                                    finally
                                    {
                                        lock (counterLock)
                                        {
                                            counter++;
                                            if (counter == filePaths.Length)
                                            {
                                                resetEvent.Set();
                                            }
                                        }
                                    }
                                });
                            }

                            resetEvent.WaitOne();
                        }

                        unitOfWork.Save();
                    }
                }
            });
        }

        #endregion
    }
}