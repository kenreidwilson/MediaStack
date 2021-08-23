using System.Linq;
using MediaStackCore.Controllers;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.Extensions.Logging;

namespace MediaStack_API.Services.CLI_Background_Services.Background_Services
{
    public class DisableMissingMediaService : BatchedParallelService<Media>
    {
        #region Data members

        protected IUnitOfWorkService UnitOfWorkService;

        protected IFileSystemController fileSystemFSHelper;

        #endregion

        #region Constructors

        public DisableMissingMediaService(ILogger logger, IUnitOfWorkService unitOfWorkService,
            IFileSystemController fsHelper) : base(logger)
        {
        }

        #endregion

        #region Methods

        public override void Execute()
        {
            Logger.LogDebug("Disabling Missing Media");
            var filePaths = this.UnitOfWorkService.Create().Media.Get().Select(m => m.Path).ToList();
            ExecuteWithData(filePaths);
        }

        protected override void ProcessData(object data)
        {
            if (data is string mediaFilePath)
            {

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

        #endregion
    }
}