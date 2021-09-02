using System.Threading;
using System.Threading.Tasks;
using MediaStack_API.Services.CLI_Background_Services.Background_Services;
using MediaStackCore.Services.MediaFilesService;
using MediaStackCore.Services.MediaScannerService;
using MediaStackCore.Services.MediaService;
using MediaStackCore.Services.UnitOfWorkFactoryService;
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

        protected IUnitOfWorkFactory UnitOfWorkFactory { get; }

        protected IMediaFilesService MediaFilesService { get; }

        protected IMediaService MediaService { get; }

        protected IMediaScanner MediaScanner { get; }

        #endregion

        #region Constructors

        public CLIBackgroundServiceHandler(IConfiguration configuration, ILogger<CLIBackgroundServiceHandler> logger,
            IUnitOfWorkFactory unitOfWorkFactory, IMediaFilesService fsController, IMediaService mediaService, IMediaScanner mediaScanner)
        {
            this.Configuration = configuration;
            this.Logger = logger;
            this.UnitOfWorkFactory = unitOfWorkFactory;
            this.MediaFilesService = fsController;
            this.MediaService = mediaService;
            this.MediaScanner = mediaScanner;
        }

        #endregion

        #region Methods

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (this.Configuration.GetValue<bool>("background"))
            {
                //this.RunServices(stoppingToken);
            }
            else
            {
                await this.RunServices(stoppingToken);
            }
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

        protected virtual async Task RunCreateAlbumService(CancellationToken stoppingToken)
        {
            await new CreateAlbumsService(this.Logger, this.UnitOfWorkFactory, this.MediaFilesService)
                .Execute(stoppingToken);
        }

        protected virtual async Task RunCreateArtistsService(CancellationToken stoppingToken)
        {
            await new CreateArtistsService(this.Logger, this.UnitOfWorkFactory, this.MediaFilesService)
                .Execute(stoppingToken);
        }

        protected virtual async Task RunCreateCategoriesService(CancellationToken stoppingToken)
        {
            await new CreateCategoriesService(this.Logger, this.UnitOfWorkFactory, this.MediaFilesService)
                .Execute(stoppingToken);
        }

        protected virtual async Task RunCreateNewMediaService(CancellationToken stoppingToken)
        {
            await new CreateNewMediaService(this.Logger, this.UnitOfWorkFactory, this.MediaService, this.MediaScanner)
                .Execute(stoppingToken);
        }

        protected virtual async Task RunDisableMissingMediaService(CancellationToken stoppingToken)
        {
            await new DisableMissingMediaService(this.Logger, this.UnitOfWorkFactory, this.MediaFilesService)
                .Execute(stoppingToken);
        }

        protected virtual async Task RunOrganizeAlbumService(CancellationToken stoppingToken)
        {
            await new OrganizeAlbumService(this.Logger, this.UnitOfWorkFactory)
                .Execute(stoppingToken);
        }

        protected virtual async Task RunVerifyMediaService(CancellationToken stoppingToken)
        {
            await new VerifyMediaService(this.Logger, this.UnitOfWorkFactory, this.MediaService, this.MediaScanner)
                .Execute(stoppingToken);
        }

        #endregion
    }
}