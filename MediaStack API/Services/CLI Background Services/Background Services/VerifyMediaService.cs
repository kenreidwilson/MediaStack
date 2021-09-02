using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaStackCore.Extensions;
using MediaStackCore.Models;
using MediaStackCore.Services.MediaScannerService;
using MediaStackCore.Services.MediaService;
using MediaStackCore.Services.UnitOfWorkFactoryService;
using Microsoft.Extensions.Logging;

namespace MediaStack_API.Services.CLI_Background_Services.Background_Services
{
    public class VerifyMediaService : BatchedParallelService<Media>
    {
        #region Data members

        protected IUnitOfWorkFactory unitOfWorkFactory;

        protected IMediaService mediaService;

        protected IMediaScanner mediaScanner;

        #endregion

        #region Constructors

        public VerifyMediaService(ILogger logger, IUnitOfWorkFactory unitOfWorkFactory, IMediaService mediaService,
            IMediaScanner mediaScanner)
            : base(logger)
        {
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.mediaService = mediaService;
            this.mediaScanner = mediaScanner;
        }

        #endregion

        #region Methods

        public override async Task Execute(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Verifying Media");
            using var unitOfWork = this.unitOfWorkFactory.Create();

            IList<Media> mediaWithChangedFile = new List<Media>();
            IList<MediaData> mediaDataWhoReplaceOldMediaFile = new List<MediaData>();

            this.mediaScanner.OnMissingMediaFound += mediaWithChangedFile.Add;
            this.mediaScanner.OnNewMediaFileFound += mediaDataWhoReplaceOldMediaFile.Add;

            await this.mediaScanner.FindChangedMediaFiles();
            await ExecuteWithData(mediaWithChangedFile, cancellationToken);
            await ExecuteWithData(mediaDataWhoReplaceOldMediaFile, cancellationToken);
        }

        protected override async Task ProcessData(object data)
        {
            if (data is Media media)
            {
                BatchedEntities[media.Hash] = this.unitOfWorkFactory.Create().DisableMedia(media);
            }

            if (data is MediaData mediaData)
            {
                var newMedia = await this.mediaService.CreateNewMediaOrFixMediaPathAsync(mediaData);

                if (newMedia != null)
                {
                    BatchedEntities[newMedia.Hash] = newMedia;
                }
            }
        }

        protected override void Save()
        {
            Logger.LogDebug("Saving Verified Media");
            using (var unitOfWork = this.unitOfWorkFactory.Create())
            {
                unitOfWork.Media.BulkInsert(
                    BatchedEntities.Values
                                   .Where(media => media.ID == 0 && !unitOfWork.Media
                                                                               .Get()
                                                                               .Any(m => m.Hash == media.Hash))
                                   .ToList());
                unitOfWork.Media.BulkUpdate(BatchedEntities.Values.Where(m => m.ID != 0).ToList());
                unitOfWork.Save();
            }

            BatchedEntities.Clear();
        }

        #endregion
    }
}