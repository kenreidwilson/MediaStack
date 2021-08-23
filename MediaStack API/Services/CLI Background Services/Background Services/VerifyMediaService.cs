using System;
using System.Linq;
using MediaStackCore.Controllers;
using MediaStackCore.Data_Access_Layer;
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

        public override void Execute()
        {
            using var unitOfWork = this.UnitOfWorkService.Create();
            ExecuteWithData(unitOfWork.Media.Get(m => m.Path != null).ToList());
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

        protected override void ProcessData(object data)
        {
            if (data is Media media)
            {
                this.addMedia(this.VerifyMedia(media));
            }
        }

        protected Media VerifyMedia(Media media)
        {
            using var unitOfWork = this.UnitOfWorkService.Create();
            if (this.fileSystemFSHelper.DoesMediaFileExist(media))
            {
                unitOfWork.DisableMedia(media);
                return media;
            }

            var currentHash = this.fileSystemFSHelper.Hasher.CalculateHash(this.fileSystemFSHelper.GetMediaData(media).GetDataStream());
            if (currentHash != media.Hash)
            {
                return this.HandleMediaHashChange(media, currentHash, unitOfWork);
            }

            return null;
        }

        protected Media HandleMediaHashChange(Media media, string newHash, IUnitOfWork unitOfWork)
        {
            unitOfWork.DisableMedia(media);
            this.addMedia(media);
            var otherMedia = unitOfWork.Media.Get().FirstOrDefault(m => m.Hash == newHash);
            if (otherMedia == null)
            {
                return this.createMedia(this.fileSystemFSHelper.GetMediaData(media), unitOfWork);
            }

            return this.HandleMovedMedia(otherMedia);
        }

        protected Media HandleMovedMedia(Media media)
        {
            media.Path = this.fileSystemFSHelper.GetMediaDataRelativePath(
                this.fileSystemFSHelper.GetMediaData(media));
            return media;
        }

        private Media createMedia(MediaData mediaData, IUnitOfWork unitOfWork)
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
