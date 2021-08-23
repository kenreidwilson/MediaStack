﻿using System.IO;
using System.Linq;
using MediaStackCore.Controllers;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.Extensions.Logging;

namespace MediaStack_Importer.Services.ScannerService.ScannerJobs
{
    public class CreateNewMediaJob : BatchScannerJob<Media>
    {
        #region Data members

        protected IUnitOfWorkService UnitOfWorkService;

        protected IFileSystemController fileSystemFSHelper;

        #endregion

        #region Constructors

        public CreateNewMediaJob(ILogger logger, IUnitOfWorkService unitOfWorkService,
            IFileSystemController fsHelper) : base(logger)
        {
            this.UnitOfWorkService = unitOfWorkService;
            this.fileSystemFSHelper = fsHelper;
        }

        #endregion

        #region Methods

        public override void Run()
        {
            Logger.LogDebug("Creating New Media");
            var filePaths = Directory.GetFiles(this.fileSystemFSHelper.MediaDirectory, "*", SearchOption.AllDirectories);
            Execute(filePaths);
        }

        protected override void ProcessData(object data)
        {
            if (data is string mediaFilePath)
            {
                Logger.LogDebug($"Processing Media: {mediaFilePath}");
                this.addMedia(this.CreateOrUpdateMediaFromFile(mediaFilePath));
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

        protected Media CreateOrUpdateMediaFromFile(string filePath)
        {
            using var unitOfWork = this.UnitOfWorkService.Create();

            if (!unitOfWork.Media.Get().Any(m => m.Path == this.fileSystemFSHelper.GetRelativePath(filePath)))
            {
                string fileHash = this.fileSystemFSHelper.Hasher.GetFileHash(filePath);
                Media media = unitOfWork.Media.Get().FirstOrDefault(m => m.Hash == fileHash);
                if (media == null)
                {
                    return this.fileSystemFSHelper.CreateNewMedia(filePath, unitOfWork);
                }
                return this.fileSystemFSHelper.UpdateMedia(filePath, unitOfWork);
            }

            return null;
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
