using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaStackCore.Controllers;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.Extensions.Logging;

namespace MediaStack_API.Services.CLI_Background_Services.Background_Services
{
    public class CreateArtistsService : BatchedParallelService<Artist>
    {
        #region Data members

        protected IUnitOfWorkService UnitOfWorkService;

        protected IFileSystemController FSController;

        #endregion

        #region Constructors

        public CreateArtistsService(ILogger logger, IUnitOfWorkService unitOfWorkService,
            IFileSystemController fsController) : base(logger)
        {
            this.FSController = fsController;
            this.UnitOfWorkService = unitOfWorkService;
        }

        #endregion

        #region Methods

        public override Task Execute(CancellationToken cancellationToken)
        {
            Logger.LogDebug("Creating Artists");
            return ExecuteWithData(this.FSController.GetArtistNames(), cancellationToken);
        }

        protected override Task ProcessData(object data)
        {
            if (data is string artistName)
            {
                Logger.LogDebug($"Processing Artist: {artistName}");
                var potentialArtist = this.getArtistIfNotExists(artistName);
                if (potentialArtist != null)
                {
                    BatchedEntities[artistName] = potentialArtist;
                }
            }

            return Task.CompletedTask;
        }

        protected override void Save()
        {
            Logger.LogDebug("Saving Artists");
            using var unitOfWork = this.UnitOfWorkService.Create();
            unitOfWork.Artists.BulkInsert(BatchedEntities.Values.ToList());
            unitOfWork.Save();
            BatchedEntities.Clear();
        }

        private Artist getArtistIfNotExists(string artistName)
        {
            using var unitOfWork = this.UnitOfWorkService.Create();
            if (!unitOfWork.Artists.Get().Any(c => c.Name == artistName))
            {
                return new Artist {Name = artistName};
            }

            return null;
        }

        #endregion
    }
}