using System;
using System.Linq;
using MediaStackCore.Controllers;
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

        public override void Execute()
        {
            Logger.LogDebug("Creating New Media");
            ExecuteWithData(this.fileSystemFSHelper.GetAllMediaData());
        }

        protected override void ProcessData(object data)
        {
            if (data is MediaData mediaData)
            {
                Logger.LogDebug($"Processing Media: {mediaData.Path}");
                this.addMedia(this.createMediaIfNotExists(mediaData));
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

        private Media createMediaIfNotExists(MediaData mediaData)
        {
            using var unitOfWork = this.UnitOfWorkService.Create();
            
            if (!unitOfWork.Media.Get().Any(m => m.Path == this.fileSystemFSHelper.GetMediaDataRelativePath(mediaData)))
            {
                return this.createMedia(mediaData);
            }

            return null;
        }

        private Media createMedia(MediaData mediaData)
        {
            throw new NotImplementedException();
        }

        private void addMedia(Media media)
        {
            if (media?.Hash == null)
            {
                return;
            }

            if (!BatchedEntities.ContainsKey(media.Hash))
            {
                BatchedEntities[media.Hash] = media;
            }
        }

        #endregion
    }
}
