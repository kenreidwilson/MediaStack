using System.Linq;
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

        public override void Execute()
        {
            Logger.LogDebug("Creating Artists");
            ExecuteWithData(this.FSController.GetArtistNames());
        }

        protected override void ProcessData(object data)
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