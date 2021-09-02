using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaStackCore.Models;
using MediaStackCore.Services.MediaFilesService;
using MediaStackCore.Services.UnitOfWorkFactoryService;
using Microsoft.Extensions.Logging;

namespace MediaStack_API.Services.CLI_Background_Services.Background_Services
{
    public class CreateArtistsService : BatchedParallelService<Artist>
    {
        #region Data members

        protected IUnitOfWorkFactory unitOfWorkFactory;

        protected IMediaFilesService FSController;

        #endregion

        #region Constructors

        public CreateArtistsService(ILogger logger, IUnitOfWorkFactory unitOfWorkFactory,
            IMediaFilesService fsController) : base(logger)
        {
            this.FSController = fsController;
            this.unitOfWorkFactory = unitOfWorkFactory;
        }

        #endregion

        #region Methods

        public override Task Execute(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Creating Artists");
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
            using var unitOfWork = this.unitOfWorkFactory.Create();
            unitOfWork.Artists.BulkInsert(BatchedEntities.Values.ToList());
            unitOfWork.Save();
            BatchedEntities.Clear();
        }

        private Artist getArtistIfNotExists(string artistName)
        {
            using var unitOfWork = this.unitOfWorkFactory.Create();
            if (!unitOfWork.Artists.Get().Any(c => c.Name == artistName))
            {
                return new Artist {Name = artistName};
            }

            return null;
        }

        #endregion
    }
}