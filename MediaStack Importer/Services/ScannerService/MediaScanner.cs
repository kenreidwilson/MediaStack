using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using MediaStack_Importer.Services.ScannerService.ScannerJobs;
using MediaStackCore.Controllers;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;

namespace MediaStack_Importer.Services.ScannerService
{
    /// <summary>
    ///     Ensures that the data persisted is
    ///     up-to-date with what is on disk.
    /// </summary>
    public class MediaScanner
    {
        #region Data members

        protected IUnitOfWorkService UnitOfWorkService;

        protected IMediaFileSystemController FSController;

        #endregion

        #region Constructors

        public MediaScanner(IMediaFileSystemController fsController, IUnitOfWorkService unitOfWorkService)
        {
            this.FSController = fsController;
            this.UnitOfWorkService = unitOfWorkService;
        }

        #endregion

        #region Methods

        public void Start()
        {
            Console.WriteLine("Creating Media References");
            new CreateCategoriesJob(this.FSController, this.UnitOfWorkService).CreateCategories();
            Console.WriteLine("Searching for new Media...");
            new CreateNewMediaJob(this.FSController, this.UnitOfWorkService).CreateNewMedia();
            //Console.WriteLine("Verifing Media...");
            //new VerifyMediaJob(this.FSController, this.UnitOfWorkService)
            Console.WriteLine("Done");
        }

        private void CreateMediaReferences()
        {
            using IUnitOfWork unitOfWork = this.UnitOfWorkService.Create();
            var categoryNames =
                Directory.GetDirectories(this.FSController.MediaDirectory, "*", SearchOption.TopDirectoryOnly);
            foreach (string categoryDirectory in categoryNames)
            {
                string categoryName = categoryDirectory.Split(Path.DirectorySeparatorChar).Last();
                Category category = unitOfWork.FindOrCreateCategory(categoryName);
                var artistNames = Directory.GetDirectories($"{categoryDirectory}", "*", SearchOption.TopDirectoryOnly);
                foreach (string artistDirectory in artistNames)
                {
                    string artistName = artistDirectory.Split(Path.DirectorySeparatorChar).Last();
                    Artist artist = unitOfWork.FindOrCreateArtist(artistName);
                    var albumNames =
                        Directory.GetDirectories($"{artistDirectory}");
                    foreach (string albumDirectory in albumNames)
                    {
                        string albumName = albumDirectory.Split(Path.DirectorySeparatorChar).Last();
                        unitOfWork.FindOrCreateAlbum(artist, albumName);
                    }
                }
            }
            unitOfWork.Save();
        }

        #endregion
    }
}
