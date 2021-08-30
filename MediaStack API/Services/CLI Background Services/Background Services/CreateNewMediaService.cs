using System;
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
    public class CreateNewMediaService : BatchedParallelService<Media>
    {
        #region Data members

        protected IUnitOfWorkService UnitOfWorkService;

        protected IFileSystemController fileSystemFSHelper;

        #endregion

        #region Constructors

        public CreateNewMediaService(ILogger logger, IUnitOfWorkService unitOfWorkService,
            IFileSystemController fsHelper) : base(logger)
        {
            this.UnitOfWorkService = unitOfWorkService;
            this.fileSystemFSHelper = fsHelper;
        }

        #endregion

        #region Methods

        public override async Task Execute(CancellationToken cancellationToken)
        {
            Logger.LogDebug("Creating New Media");
            await ExecuteWithData(await this.getNewMediaData(), cancellationToken);
        }

        protected override async Task ProcessData(object data)
        {
            if (data is MediaData mediaData)
            {
                Logger.LogDebug($"Processing Media: {mediaData.RelativePath}");

                using var unitOfWork = this.UnitOfWorkService.Create();

                try
                {
                    var media = await unitOfWork.CreateNewMediaOrFixMediaPathAsync(this.fileSystemFSHelper, mediaData);
                    if (media != null)
                    {
                        BatchedEntities[media.Hash] = media;
                    }
                }
                catch (ArgumentException e)
                {
                    Logger.LogDebug($"Error processing {mediaData.RelativePath}: {e.Message}");
                }
            }
        }

        protected override void Save()
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                Logger.LogDebug("Saving Media");
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

        private async Task<IEnumerable<MediaData>> getNewMediaData()
        {
            var mfc = new MediaFilesController(this.fileSystemFSHelper, this.UnitOfWorkService);
            var mediaDataList = new List<MediaData>();
            mfc.OnNewMediaFileFound += mediaDataList.Add;
            await mfc.FindNewMedia();
            return mediaDataList;
        }

        #endregion
    }
}