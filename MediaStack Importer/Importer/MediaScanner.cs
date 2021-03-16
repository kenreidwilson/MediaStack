using MediaStack_Library.Controllers;
using MediaStack_Library.Data_Access_Layer;
using MediaStack_Library.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MediaStack_Importer.Importer
{
    /// <summary>
    ///     Ensures that the data persisted is
    ///     up-to-date with what is on disk.
    /// </summary>
    public class MediaScanner
    {
        protected MediaFSController controller;

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
            List<Task> tasks = new List<Task>();
            using (var unitOfWork = new UnitOfWork<MediaStackContext>())
            {
                foreach (string filePath in Directory.GetFiles(MediaFSController.MEDIA_DIRECTORY, "*", SearchOption.AllDirectories))
                {
                    tasks.Add(Task.Factory.StartNew(() => this.controller.CreateOrUpdateMediaFromFile(filePath, unitOfWork)));
                }
                Task.WaitAll(tasks.ToArray());
                unitOfWork.Save();
            }
        }

        private void verifyAllMedia()
        {
            List<Task> tasks = new List<Task>();
            using (var unitOfWork = new UnitOfWork<MediaStackContext>())
            {
                foreach (Media media in unitOfWork.Media.Get().Where(media => media.Path != null).ToList())
                {
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        if (!File.Exists(media.Path))
                        {
                            this.controller.DisableMedia(media, unitOfWork);
                        }
                        else
                        {
                            this.controller.CreateOrUpdateMediaFromFile(media.Path, unitOfWork);
                        }
                    }));
                }
                Task.WaitAll(tasks.ToArray());
                unitOfWork.Save();
            }
        }
    }
}
