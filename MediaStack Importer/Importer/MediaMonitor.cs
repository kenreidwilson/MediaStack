using MediaStack_Library.Controllers;
using MediaStack_Library.Data_Access_Layer;
using MediaStack_Library.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediaStack_Importer.Importer
{
    /// <summary>
    ///     Monitors for changes on disk and updates
    ///     the DAL accordingly.
    /// </summary>
    public class MediaMonitor
    {
        protected MediaFSController controller;

        protected FileSystemWatcher watcher;

        private List<string> ignoreExtensionList = new List<string>
        {
            ".part", ".crdownload"
        };

        public MediaMonitor(MediaFSController controller)
        {
            this.controller = controller;
        }

        public async Task Start()
        {
            this.watcher = new FileSystemWatcher(MediaFSController.MEDIA_DIRECTORY);

            this.watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            this.watcher.Changed += OnChanged;
            this.watcher.Created += OnCreated;
            this.watcher.Deleted += OnDeleted;
            this.watcher.Renamed += OnRenamed;

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
                if (e.ChangeType != WatcherChangeTypes.Changed || Directory.Exists(e.FullPath) || ignoreExtensionList.Contains(Path.GetExtension(e.FullPath)))
                {
                    return;
                }

                Console.WriteLine($"Changed: {e.FullPath}");
                try
                {
                    using (var unitOfWork = new UnitOfWork<MediaStackContext>())
                    {
                        this.controller.CreateOrUpdateMediaFromFile(e.FullPath, unitOfWork);
                        unitOfWork.Save();
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("Couldn't Read...");
                    return;
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
                    using (var unitOfWork = new UnitOfWork<MediaStackContext>())
                    {
                        this.controller.CreateOrUpdateMediaFromFile(e.FullPath, unitOfWork);
                        unitOfWork.Save();
                    }
                }
                catch (IOException)
                {
                    return;
                } 
            });
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Task.Run(() =>
            {
                Console.WriteLine($"Deleted: {e.FullPath}");
                using (var unitOfWork = new UnitOfWork<MediaStackContext>())
                {
                    Media media = unitOfWork.Media.Get(media => media.Path == e.FullPath).FirstOrDefault();
                    if (media != null)
                    {
                        this.controller.DisableMedia(media, unitOfWork);
                        unitOfWork.Save();
                    }
                    else
                    {
                        foreach (Media m in unitOfWork.Media.Get(media => media.Path.ToLower().StartsWith(e.FullPath.ToLower())))
                        {
                            this.controller.DisableMedia(m, unitOfWork);
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
                if (Directory.Exists(e.FullPath) || ignoreExtensionList.Contains(Path.GetExtension(e.FullPath)))
                {
                    return;
                }

                Console.WriteLine($"Renamed:");
                Console.WriteLine($"    Old: {e.OldFullPath}");
                Console.WriteLine($"    New: {e.FullPath}");
                using (var unitOfWork = new UnitOfWork<MediaStackContext>())
                {
                    if (File.Exists(e.FullPath))
                    {
                        Media media = unitOfWork.Media.Get().Where(media => media.Path == e.OldFullPath).FirstOrDefault();
                        if (media != null)
                        {
                            media.Path = e.FullPath;
                            unitOfWork.Media.Update(media);
                            unitOfWork.Save();
                        }
                    }
                    else if (Directory.Exists(e.FullPath))
                    {
                        string newPath = e.OldFullPath.Last() == Path.DirectorySeparatorChar ? e.OldFullPath + "media" : e.OldFullPath + Path.DirectorySeparatorChar + "media";
                        string oldPath = e.FullPath.Last() == Path.DirectorySeparatorChar ? e.FullPath + "media" : e.FullPath + Path.DirectorySeparatorChar + "media";

                        List<Task> tasks = new List<Task>();
                        foreach (string filePath in Directory.GetFiles(newPath, "*", SearchOption.AllDirectories))
                        {
                            tasks.Add(Task.Factory.StartNew(() => this.controller.CreateOrUpdateMediaFromFile(filePath, unitOfWork)));
                        }
                        Task.WaitAll(tasks.ToArray());
                        unitOfWork.Save();
                    }
                }
            });
        }
    }
}
