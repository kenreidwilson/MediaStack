using System;
using MediaStack_Importer.Services.ScannerService.ScannerJobs;
using MediaStackCore.Controllers;
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
            new CreateArtistsJob(this.FSController, this.UnitOfWorkService).CreateArtists();
            new CreateAlbumsJob(this.FSController, this.UnitOfWorkService).CreateAlbums();
            Console.WriteLine("Searching for new Media...");
            var nmJob = new CreateNewMediaJob(this.FSController, this.UnitOfWorkService);
            nmJob.CreateNewMedia();
            Console.WriteLine("Verifying Media...");
            var vmJob = new VerifyMediaJob(this.FSController, this.UnitOfWorkService) {
                HashCache = nmJob.HashCache
            };
            vmJob.VerifyAllMedia();
            Console.WriteLine("Done");
        }

        #endregion
    }
}