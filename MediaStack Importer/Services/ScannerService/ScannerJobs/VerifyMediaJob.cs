using System;
using System.IO;
using System.Linq;
using MediaStackCore.Controllers;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;

namespace MediaStack_Importer.Services.ScannerService.ScannerJobs
{
    public class VerifyMediaJob : MediaScannerJob
    {
        #region Constructors

        public VerifyMediaJob(IMediaFileSystemController fsController, IUnitOfWorkService unitOfWorkService)
            : base(fsController, unitOfWorkService) { }

        #endregion

        #region Methods

        protected override void Save()
        {
            throw new NotImplementedException();
        }

        protected override void ProcessData(object data)
        {
            throw new NotImplementedException();
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
