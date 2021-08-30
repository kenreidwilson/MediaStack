using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaStackCore.Controllers;
using MediaStackCore.Extensions;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.Extensions.Logging;

namespace MediaStack_API.Services.CLI_Background_Services.Background_Services
{
    public class VerifyMediaService : BatchedParallelService<Media>
    {
        #region Data members

        protected IUnitOfWorkService UnitOfWorkService;

        protected IFileSystemController fileSystemFSHelper;

        #endregion

        #region Constructors

        public VerifyMediaService(ILogger logger, IUnitOfWorkService unitOfWorkService, IFileSystemController helper)
            : base(logger)
        {
            this.UnitOfWorkService = unitOfWorkService;
            this.fileSystemFSHelper = helper;
        }

        #endregion

        #region Methods

        public override async Task Execute(CancellationToken cancellationToken)
        {
            using var unitOfWork = this.UnitOfWorkService.Create();

            IList<Media> mediaWithChangedFile = new List<Media>();
            IList<MediaData> mediaDataWhoReplaceOldMediaFile = new List<MediaData>();

            var mfc = new MediaFilesController(this.fileSystemFSHelper, this.UnitOfWorkService);

            mfc.OnMediaFileChanged += (media, mediaData) =>
            {
                mediaWithChangedFile.Add(media);
                mediaDataWhoReplaceOldMediaFile.Add(mediaData);
            };

            await mfc.FindChangedMediaFiles();
            await ExecuteWithData(mediaWithChangedFile, cancellationToken);
            await ExecuteWithData(mediaDataWhoReplaceOldMediaFile, cancellationToken);
        }

        protected override async Task ProcessData(object data)
        {
            if (data is Media media)
            {
                BatchedEntities[media.Hash] = this.UnitOfWorkService.Create().DisableMedia(media);
            }

            if (data is MediaData mediaData)
            {
                var newMedia = await this.UnitOfWorkService.Create()
                                         .CreateNewMediaOrFixMediaPathAsync(this.fileSystemFSHelper, mediaData);

                if (newMedia != null)
                {
                    BatchedEntities[newMedia.Hash] = newMedia;
                }
            }
        }

        protected override void Save()
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
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