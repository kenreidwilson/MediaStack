using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaStackCore.Models;
using MediaStackCore.Services.MediaFilesService;
using MediaStackCore.Services.MediaScannerService;
using MediaStackCore.Services.UnitOfWorkFactoryService;
using Microsoft.Extensions.Logging;

namespace MediaStack_API.Services.CLI_Background_Services.Background_Services
{
    public class DisableMissingMediaService : BatchedParallelService<Media>
    {
        #region Data members

        protected IUnitOfWorkFactory unitOfWorkFactory;

        protected IMediaFilesService mediaFilesService;

        #endregion

        #region Constructors

        public DisableMissingMediaService(ILogger logger, IUnitOfWorkFactory unitOfWorkFactory,
            IMediaFilesService fsHelper) : base(logger)
        {
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.mediaFilesService = fsHelper;
        }

        #endregion

        #region Methods

        public override async Task Execute(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Disabling Missing Media");
            await ExecuteWithData(await this.getMissingMedia(), cancellationToken);
        }

        protected override Task ProcessData(object data)
        {
            if (data is Media media)
            {
                media.Path = null;
                BatchedEntities[media.Hash] = media;
            }

            return Task.CompletedTask;
        }

        protected override void Save()
        {
            Logger.LogDebug("Saving Disabled Media");
            using (var unitOfWork = this.unitOfWorkFactory.Create())
            {
                unitOfWork.Media.BulkUpdate(BatchedEntities.Values.ToList());
                unitOfWork.Save();
            }

            BatchedEntities.Clear();
        }

        protected override void OnFinish()
        {
            this.Save();
            this.Logger.LogInformation("Done Disabling Media");
        }

        private async Task<IEnumerable<Media>> getMissingMedia()
        {
            var missingMedia = new List<Media>();
            var mfc = new MediaScanner(this.mediaFilesService, this.unitOfWorkFactory);
            mfc.OnMissingMediaFound += missingMedia.Add;
            await mfc.FindMissingMedia();
            Logger.LogInformation($"Found {missingMedia.Count} missing Media");
            return missingMedia;
        }

        #endregion
    }
}
