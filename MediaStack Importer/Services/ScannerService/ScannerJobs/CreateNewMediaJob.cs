using System.IO;
using System.Linq;
using MediaStackCore.Controllers;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.Extensions.Logging;

namespace MediaStack_Importer.Services.ScannerService.ScannerJobs
{
    public class CreateNewMediaJob : MediaScannerJob
    {
        #region Constructors

        public CreateNewMediaJob(ILogger logger, IMediaFileSystemController fsController, IUnitOfWorkService unitOfWorkService)
            : base(logger, fsController, unitOfWorkService) { }

        #endregion

        #region Methods

        public void CreateNewMedia()
        {
            var filePaths = Directory.GetFiles(FSController.MediaDirectory, "*", SearchOption.AllDirectories);
            Execute(filePaths);
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
        }

        protected override void ProcessData(object data)
        {
            if (data is string mediaFilePath)
            {
                AddMedia(this.CreateMediaFromFileIfNotExists(mediaFilePath));
            }
        }

        protected Media CreateMediaFromFileIfNotExists(string filePath)
        {
            using var unitOfWork = UnitOfWorkService.Create();
            return unitOfWork.Media.Get().Any(m => string.Equals(m.Path, GetRelativePath(filePath)))
                ? null
                : CreateMediaFromFile(filePath, unitOfWork);
        }

        #endregion
    }
}
