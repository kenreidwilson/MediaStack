using System.Threading;
using System.Threading.Tasks;
using MediaStack_API.Services.CLI_Background_Services.Background_Services;
using MediaStackCore.Controllers;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MediaStack_API.Services.CLI_Background_Services
{
    public class CLIBackgroundServiceHandler : BackgroundService
    {
        #region Properties

        protected IConfiguration Configuration { get; }

        protected ILogger Logger { get; }

        protected IUnitOfWorkService UnitOfWorkService { get; }

        protected IFileSystemController FileSystemController { get; }

        #endregion

        #region Constructors

        public CLIBackgroundServiceHandler(IConfiguration configuration, ILogger logger,
            IUnitOfWorkService unitOfWorkService, IFileSystemController fsController)
        {
            this.Configuration = configuration;
            this.Logger = logger;
            this.UnitOfWorkService = unitOfWorkService;
            this.FileSystemController = fsController;
        }

        #endregion

        #region Methods

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (this.Configuration.GetValue<bool>("background"))
            {
                this.RunServices(stoppingToken);
                return Task.CompletedTask;
            }

            return this.RunServices(stoppingToken);
        }

        protected virtual async Task RunServices(CancellationToken stoppingToken)
        {
            if (this.Configuration.GetValue<bool>("createSections"))
            {
                await this.RunCreateCategoriesService(stoppingToken);
                await this.RunCreateArtistsService(stoppingToken);
                await this.RunCreateAlbumService(stoppingToken);
            }

            if (this.Configuration.GetValue<bool>("createNew"))
            {
                await this.RunCreateNewMediaService(stoppingToken);
            }

            if (this.Configuration.GetValue<bool>("disableMissing"))
            {
                await this.RunDisableMissingMediaService(stoppingToken);
            }

            if (this.Configuration.GetValue<bool>("verify"))
            {
                await this.RunVerifyMediaService(stoppingToken);
            }

            if (this.Configuration.GetValue<bool>("organizeAlbums"))
            {
                await this.RunOrganizeAlbumService(stoppingToken);
            }
        }

        protected virtual Task RunCreateAlbumService(CancellationToken stoppingToken)
        {
            return new CreateAlbumsService(this.Logger, this.UnitOfWorkService, this.FileSystemController)
                .Execute(stoppingToken);
        }

        protected virtual Task RunCreateArtistsService(CancellationToken stoppingToken)
        {
            return new CreateArtistsService(this.Logger, this.UnitOfWorkService, this.FileSystemController)
                .Execute(stoppingToken);
        }

        protected virtual Task RunCreateCategoriesService(CancellationToken stoppingToken)
        {
            return new CreateCategoriesService(this.Logger, this.UnitOfWorkService, this.FileSystemController)
                .Execute(stoppingToken);
        }

        protected virtual Task RunCreateNewMediaService(CancellationToken stoppingToken)
        {
            return new CreateNewMediaService(this.Logger, this.UnitOfWorkService, this.FileSystemController)
                .Execute(stoppingToken);
        }

        protected virtual Task RunDisableMissingMediaService(CancellationToken stoppingToken)
        {
            return new DisableMissingMediaService(this.Logger, this.UnitOfWorkService, this.FileSystemController)
                .Execute(stoppingToken);
        }

        protected virtual Task RunOrganizeAlbumService(CancellationToken stoppingToken)
        {
            return new OrganizeAlbumService(this.Logger, this.UnitOfWorkService)
                .Execute(stoppingToken);
        }

        protected virtual Task RunVerifyMediaService(CancellationToken stoppingToken)
        {
            return new VerifyMediaService(this.Logger, this.UnitOfWorkService, this.FileSystemController)
                .Execute(stoppingToken);
        }

        #endregion
    }
}