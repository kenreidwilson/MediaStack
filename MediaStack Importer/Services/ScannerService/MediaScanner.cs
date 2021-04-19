using System;
using System.IO;
using System.Linq;
using System.Threading;
using MediaStack_Library.Controllers;
using MediaStack_Library.Data_Access_Layer;
using MediaStack_Library.Services.UnitOfWorkService;

namespace MediaStack_Importer.Services.ScannerService
{
    /// <summary>
    ///     Ensures that the data persisted is
    ///     up-to-date with what is on disk.
    /// </summary>
    public class MediaScanner : BaseImporterService
    {
        #region Data members

        private const int BATCH_SIZE = 100;
        protected IMediaFileSystemController FSController;

        protected IUnitOfWorkService UnitOfWorkService;

        #endregion

        #region Constructors

        public MediaScanner(IMediaFileSystemController fsController, IUnitOfWorkService unitOfWorkService) : base(
            fsController)
        {
            this.FSController = fsController;
            this.UnitOfWorkService = unitOfWorkService;
        }

        #endregion

        #region Methods

        public void Start()
        {
            Console.WriteLine("Searching for new Media...");
            this.searchForNewMedia();
            Console.WriteLine("Verifing Media...");
            this.verifyAllMedia();
            Console.WriteLine("Done");
        }

        private void searchForNewMedia()
        {
            using (var resetEvent = new ManualResetEvent(false))
            {
                using (var unitOfWork = this.UnitOfWorkService.Create())
                {
                    var filePaths = Directory.GetFiles(this.FSController.MediaDirectory, "*",
                        SearchOption.AllDirectories);
                    var counter = 0;
                    var counterLock = new object();
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
                                    if (counter % BATCH_SIZE == 0)
                                    {
                                        unitOfWork.Save();
                                    }

                                    if (counter == filePaths.Length)
                                    {
                                        resetEvent.Set();
                                    }
                                }
                            }
                        });
                    }

                    resetEvent.WaitOne();
                    unitOfWork.Save();
                }
            }
        }

        private void verifyAllMedia()
        {
            using (var resetEvent = new ManualResetEvent(false))
            {
                using (var unitOfWork = this.UnitOfWorkService.Create())
                {
                    var counter = 0;
                    var counterLock = new object();
                    var medias = unitOfWork.Media.Get().Where(media => media.Path != null).ToList();
                    foreach (var media in medias)
                    {
                        ThreadPool.QueueUserWorkItem(callBack =>
                        {
                            try
                            {
                                if (!File.Exists(this.FSController.GetMediaFullPath(media)))
                                {
                                    unitOfWork.DisableMedia(media);
                                }
                                else
                                {
                                    CreateOrUpdateMediaFromFile(this.FSController.GetMediaFullPath(media), unitOfWork,
                                        counterLock);
                                }
                            }
                            catch (Exception)
                            {
                            }
                            finally
                            {
                                lock (counterLock)
                                {
                                    counter++;
                                    if (counter % BATCH_SIZE == 0)
                                    {
                                        unitOfWork.Save();
                                    }

                                    if (counter == medias.Count)
                                    {
                                        resetEvent.Set();
                                    }
                                }
                            }
                        });
                    }

                    resetEvent.WaitOne();
                    unitOfWork.Save();
                }
            }
        }

        #endregion
    }
}