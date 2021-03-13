using MediaStack_Library.Controllers;
using MediaStack_Library.Data_Access_Layer;
using MediaStack_Library.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaStack_Importer.Importer
{
    public class MediaImporter
    {
        protected IUnitOfWork unitOfWork;
        protected MediaFSController controller;

        public MediaImporter()
        {
            this.unitOfWork = new UnitOfWork<MediaStackContext>();
            this.controller = new MediaFSController(unitOfWork);
        }

        public async Task start()
        {
            await this.startScanner();
            await this.startMonitor();
        }

        public async Task startScanner()
        {
            this.searchForNewMedia();
            this.verifyMedia();
        }

        private void searchForNewMedia()
        {
            foreach (string filePath in Directory.GetFiles(MediaFSController.MEDIA_DIRECTORY, "*", SearchOption.AllDirectories))
            {
                Media media = this.unitOfWork.Media.Get().Where(media => media.Path == filePath).FirstOrDefault();
                if (media == null)
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        string mediaHash = MediaFSController.CalculateHash(stream);
                        media = this.unitOfWork.Media.Get().Where(media => media.Hash == mediaHash).FirstOrDefault();
                    }
                    if (media == null)
                    {
                        this.controller.InitializeMedia(filePath);
                    }
                    else
                    {
                        media.Path = filePath;
                    }
                    this.unitOfWork.Save();
                }
            }
        }

        private void verifyMedia()
        {
            foreach (Media media in this.unitOfWork.Media.Get().Where(media => media.Path != null))
            {
                if (!File.Exists(media.Path))
                {
                    media.Path = null;
                    this.unitOfWork.Media.Update(media);
                }
                else
                {
                    string hash = null;
                    using (var stream = File.OpenRead(media.Path))
                    {
                        hash = MediaFSController.CalculateHash(stream);
                    }
                    if (media.Hash != hash)
                    {
                        string path = media.Path;
                        media.Path = null;
                        this.controller.InitializeMedia(path, hash);
                    }

                }
            }
            this.unitOfWork.Save();
        }

        public async Task startMonitor()
        {
            return;
        }
    }
}
