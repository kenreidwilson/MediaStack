using MediaStack_Importer.Services.ScannerService.ScannerJobs;
using MediaStackCore.Controllers;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.Extensions.Logging;

namespace MediaStack_Importer.Services.ScannerService
{
    /// <summary>
    ///     Ensures that the data persisted is
    ///     up-to-date with what is on disk.
    /// </summary>
    public class MediaScanner
    {
        #region Data members

        protected ILogger Logger;

        protected IUnitOfWorkService UnitOfWorkService;

        protected IMediaFileSystemController FSController;

        #endregion

        #region Constructors

        public MediaScanner(ILogger logger, IMediaFileSystemController fsController, IUnitOfWorkService unitOfWorkService)
        {
            this.Logger = logger;
            this.FSController = fsController;
            this.UnitOfWorkService = unitOfWorkService;
        }

        #endregion

        #region Methods

        public void Start()
        {
            this.Logger.LogInformation("Creating Media References");
            new CreateCategoriesJob(this.Logger, this.FSController, this.UnitOfWorkService).CreateCategories();
            new CreateArtistsJob(this.Logger, this.FSController, this.UnitOfWorkService).CreateArtists();
            new CreateAlbumsJob(this.Logger, this.FSController, this.UnitOfWorkService).CreateAlbums();
            this.Logger.LogInformation("Searching for new Media...");
            var nmJob = new CreateNewMediaJob(this.Logger, this.FSController, this.UnitOfWorkService);
            nmJob.CreateNewMedia();
            this.Logger.LogInformation("Verifying Media...");
            var vmJob = new VerifyMediaJob(this.Logger, this.FSController, this.UnitOfWorkService) {
                HashCache = nmJob.HashCache
            };
            vmJob.VerifyAllMedia();
            this.Logger.LogInformation("Done");
        }

        #endregion
    }
}