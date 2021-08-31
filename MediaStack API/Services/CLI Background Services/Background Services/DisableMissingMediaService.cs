using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            this.UnitOfWorkService = unitOfWorkService;
            this.fileSystemFSHelper = fsHelper;
        }

        #endregion

        #region Methods

        public override async Task Execute(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Disabling Missing Media");
            await ExecuteWithData(await this.getMissingMedia(), cancellationToken);
        }

        protected override Task ProcessData(object data)
        {
            if (data is Media media)
            {
                media.Path = null;
                BatchedEntities[media.Hash] = media;
            }

            return Task.CompletedTask;
        }

        protected override void Save()
        {
            Logger.LogDebug("Saving Disabled Media");
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                unitOfWork.Media.BulkUpdate(BatchedEntities.Values.ToList());
                unitOfWork.Save();
            }

            BatchedEntities.Clear();
        }

        private async Task<IEnumerable<Media>> getMissingMedia()
        {
            var missingMedia = new List<Media>();
            var mfc = new MediaFilesController(this.fileSystemFSHelper, this.UnitOfWorkService);
            mfc.OnMissingMediaFound += missingMedia.Add;
            await mfc.FindMissingMedia();
            return missingMedia;
        }

        #endregion
    }
}
