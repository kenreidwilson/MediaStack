using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaStackCore.Controllers;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Extensions;
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
            this.UnitOfWorkService = unitOfWorkService;
            this.fileSystemFSHelper = fsHelper;
        }

        #endregion

        #region Methods

        public override Task Execute(CancellationToken cancellationToken)
        {
            Logger.LogDebug("Disabling Missing Media");
            var media = this.UnitOfWorkService.Create().Media.Get(m => m.Path != null);
            return ExecuteWithData(media, cancellationToken);
        }

        protected override Task ProcessData(object data)
        {
            using (IUnitOfWork unitOfWork = this.UnitOfWorkService.Create())
            {
                if (data is Media media)
                {
                    if (!this.fileSystemFSHelper.DoesMediaFileExist(media))
                    {
                        unitOfWork.DisableMedia(media);
                        this.addMedia(media);
                    }
                }
            }
            
            return Task.CompletedTask;
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