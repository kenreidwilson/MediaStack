using MediaStack_Library.Controllers;
using MediaStack_Library.Data_Access_Layer;
using MediaStack_Library.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace MediaStack_Importer.Importer
{
    /// <summary>
    ///     Ensures that the data persisted is
    ///     up-to-date with what is on disk.
    /// </summary>
    public class MediaScanner
    {
        protected MediaFSController controller;

        private const int BATCH_SIZE = 100;

        public MediaScanner(MediaFSController controller)
        {
            this.controller = controller;
        }

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
            using (ManualResetEvent resetEvent = new ManualResetEvent(false))
            {
                using (var unitOfWork = new UnitOfWork<MediaStackContext>())
                {
                    var filePaths = Directory.GetFiles(MediaFSController.MEDIA_DIRECTORY, "*", SearchOption.AllDirectories);
                    int counter = 0;
                    object counterLock = new object();
                    foreach (string filePath in filePaths)
                    {
                        ThreadPool.QueueUserWorkItem(callBack =>
                        {
                            try
                            {
                                this.controller.CreateOrUpdateMediaFromFile(filePath, unitOfWork);
                            }
                            catch (Exception) { } 
                            finally
                            {
                                lock (counterLock)
                                {
                                    counter++;
                                    if (counter % BATCH_SIZE == 0)
                                    {
                                        this.controller.Save(unitOfWork);
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
                    this.controller.Save(unitOfWork);
                }
            }
        }

        private void verifyAllMedia()
        {
            using (ManualResetEvent resetEvent = new ManualResetEvent(false))
            {
                using (var unitOfWork = new UnitOfWork<MediaStackContext>())
                {
                    int counter = 0;
                    object counterLock = new object();
                    List<Media> medias = unitOfWork.Media.Get().Where(media => media.Path != null).ToList();
                    foreach (Media media in medias)
                    {
                        ThreadPool.QueueUserWorkItem(callBack =>
                        {
                            try
                            {
                                if (!File.Exists(media.Path))
                                {
                                    this.controller.DisableMedia(media, unitOfWork);
                                }
                                else
                                {
                                    this.controller.CreateOrUpdateMediaFromFile(media.Path, unitOfWork);
                                }
                            }
                            catch (Exception) { }
                            finally
                            {
                                lock (counterLock)
                                {
                                    counter++;
                                    if (counter % BATCH_SIZE == 0)
                                    {
                                        this.controller.Save(unitOfWork);
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
                    this.controller.Save(unitOfWork);
                }
            }
        }
    }
}
