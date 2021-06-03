using System.IO;
using System.Linq;
using MediaStack_Importer.Controllers;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.Extensions.Logging;

namespace MediaStack_Importer.Services.ScannerService.ScannerJobs
{
    public class CreateNewMediaJob : BatchScannerJob<Media>
    {
        #region Data members

        protected IUnitOfWorkService UnitOfWorkService;

        protected IMediaFileSystemHelper MediaFSHelper;

        #endregion

        #region Constructors

        public CreateNewMediaJob(ILogger logger, IUnitOfWorkService unitOfWorkService,
            IMediaFileSystemHelper fsHelper) : base(logger)
        {
            this.UnitOfWorkService = unitOfWorkService;
            this.MediaFSHelper = fsHelper;
        }

        #endregion

        #region Methods

        public override void Run()
        {
            Logger.LogDebug("Creating New Media");
            var filePaths = Directory.GetFiles(this.MediaFSHelper.MediaDirectory, "*", SearchOption.AllDirectories);
            Execute(filePaths);
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

        protected override void ProcessData(object data)
        {
            if (data is string mediaFilePath)
            {
                Logger.LogDebug($"Processing Media: {mediaFilePath}");
                this.addMedia(this.CreateOrUpdateMediaFromFile(mediaFilePath));
            }
        }

        protected Media CreateOrUpdateMediaFromFile(string filePath)
        {
            using var unitOfWork = this.UnitOfWorkService.Create();

            if (!unitOfWork.Media.Get().Any(m => string.Equals(m.Path, this.MediaFSHelper.GetRelativePath(filePath))))
            {
                string fileHash = this.MediaFSHelper.GetFileHash(filePath);
                Media media = unitOfWork.Media.Get().FirstOrDefault(m => m.Hash == fileHash);
                if (media == null)
                {
                    return this.MediaFSHelper.CreateMediaFromFile(filePath, unitOfWork);
                }
                else
                {
                    return this.MediaFSHelper.UpdateMediaFromFile(filePath, unitOfWork);
                }
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
