using System.IO;
using System.Linq;
using MediaStack_Importer.Controllers;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.Extensions.Logging;

namespace MediaStack_Importer.Services.ScannerService.ScannerJobs
{
    public class VerifyMediaJob : BatchScannerJob<Media>
    {
        #region Data members

        protected IUnitOfWorkService UnitOfWorkService;

        protected IMediaFileSystemHelper MediaFSHelper;

        #endregion

        #region Constructors

        public VerifyMediaJob(ILogger logger, IUnitOfWorkService unitOfWorkService, IMediaFileSystemHelper helper)
            : base(logger)
        {
            this.UnitOfWorkService = unitOfWorkService;
            this.MediaFSHelper = helper;
        }

        #endregion

        #region Methods

        public override void Run()
        {
            using var unitOfWork = this.UnitOfWorkService.Create();
            Execute(unitOfWork.Media.Get(m => m.Path != null).ToList());
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
                this.VerifyMedia(media);
            }
        }

        protected Media VerifyMedia(Media media)
        {
            using var unitOfWork = this.UnitOfWorkService.Create();
            if (!File.Exists(this.MediaFSHelper.GetMediaFullPath(media)))
            {
                unitOfWork.DisableMedia(media);
                return media;
            }

            var newHash = this.MediaFSHelper.GetFileHash(this.MediaFSHelper.GetMediaFullPath(media));
            if (newHash != media.Hash)
            {
                return this.HandleMediaHashChange(media, unitOfWork, newHash);
            }

            return null;
        }

        protected Media HandleMediaHashChange(Media media, IUnitOfWork unitOfWork, string newHash)
        {
            var path = this.MediaFSHelper.GetMediaFullPath(media);
            unitOfWork.DisableMedia(media);
            this.addMedia(media);
            var pMedia = unitOfWork.Media.Get().FirstOrDefault(m => m.Hash == newHash);
            if (pMedia == null)
            {
                return this.MediaFSHelper.CreateMediaFromFile(path, unitOfWork);
            }

            return this.HandleMovedMedia(pMedia, path, unitOfWork);
        }

        protected Media HandleMovedMedia(Media media, string newPath, IUnitOfWork unitOfWork)
        {
            media.Path = this.MediaFSHelper.GetRelativePath(newPath);
            return media;
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
