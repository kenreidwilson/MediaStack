using MediaStack_Importer.Controllers;
using MediaStack_Importer.Services.ScannerService.ScannerJobs;
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

        protected IMediaFileSystemHelper MediaFSHelper;


        #endregion

        #region Constructors

        public MediaScanner(ILogger logger, IUnitOfWorkService unitOfWorkService, IMediaFileSystemHelper helper)
        {
            this.Logger = logger;
            this.UnitOfWorkService = unitOfWorkService;
            this.MediaFSHelper = helper;
        }

        #endregion

        #region Methods

        public void Start()
        {
            this.Logger.LogInformation("Creating Media References");
            new CreateCategoriesJob(this.Logger, this.UnitOfWorkService, this.MediaFSHelper).Run();
            new CreateArtistsJob(this.Logger, this.UnitOfWorkService, this.MediaFSHelper).Run();
            new CreateAlbumsJob(this.Logger, this.UnitOfWorkService, this.MediaFSHelper).Run();
            this.Logger.LogInformation("Searching for new Media...");
            new CreateNewMediaJob(this.Logger, this.UnitOfWorkService, this.MediaFSHelper).Run();
            this.Logger.LogInformation("Verifying Media...");
            new VerifyMediaJob(this.Logger,  this.UnitOfWorkService, this.MediaFSHelper).Run();
            this.Logger.LogInformation("Done");
        }

        #endregion
    }
}
