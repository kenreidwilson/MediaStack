using System.IO;
using System.Linq;
using MediaStackCore.Controllers;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.Extensions.Logging;

namespace MediaStack_Importer.Services.ScannerService.ScannerJobs
{
    public class VerifyMediaJob : MediaScannerJob
    {
        #region Constructors

        public VerifyMediaJob(ILogger logger, IMediaFileSystemController fsController,
            IUnitOfWorkService unitOfWorkService)
            : base(logger, fsController, unitOfWorkService) { }

        #endregion

        #region Methods

        public void VerifyAllMedia()
        {
            using var unitOfWork = UnitOfWorkService.Create();
            Execute(unitOfWork.Media.Get(m => m.Path != null).ToList());
        }

        protected override void Save()
        {
            using (var unitOfWork = UnitOfWorkService.Create())
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
            using var unitOfWork = UnitOfWorkService.Create();
            if (!File.Exists(FSController.GetMediaFullPath(media)))
            {
                unitOfWork.DisableMedia(media);
                return media;
            }

            var newHash = GetFileHash(FSController.GetMediaFullPath(media));
            if (newHash != media.Hash)
            {
                return this.HandleMediaHashChange(media, unitOfWork, newHash);
            }

            return null;
        }

        protected Media HandleMediaHashChange(Media media, IUnitOfWork unitOfWork, string newHash)
        {
            var path = FSController.GetMediaFullPath(media);
            unitOfWork.DisableMedia(media);
            AddMedia(media);
            var pMedia = unitOfWork.Media.Get().FirstOrDefault(m => m.Hash == newHash);
            if (pMedia == null)
            {
                return CreateMediaFromFile(path, unitOfWork);
            }

            return this.HandleMovedMedia(pMedia, path, unitOfWork);
        }

        protected Media HandleMovedMedia(Media media, string newPath, IUnitOfWork unitOfWork)
        {
            media.Path = GetRelativePath(newPath);
            return media;
        }

        #endregion
    }
}