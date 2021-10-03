using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkFactoryService;
using Microsoft.Extensions.Logging;

namespace MediaStack_API.Services.CLI_Background_Services.Background_Services
{
    public class RegisterAlbumCoversService : BatchedParallelService<Media>
    {
        #region Data members

        protected IUnitOfWorkFactory unitOfWorkFactory;

        #endregion

        #region Constructors

        public RegisterAlbumCoversService(ILogger logger, IUnitOfWorkFactory unitOfWorkFactory) : base(logger)
        {
            this.unitOfWorkFactory = unitOfWorkFactory;
        }

        #endregion

        #region Methods

        public override Task Execute(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Registering Album covers");
            using (var unitOfWork = this.unitOfWorkFactory.Create())
            {
                return ExecuteWithData(unitOfWork.Albums.Get(a => !a.Media.Any(m => m.Path != null && m.AlbumOrder == 0)),
                    cancellationToken);
            }
        }

        protected override Task ProcessData(object data)
        {
            if (data is Album album)
            {
                using (var unitOfWork = this.unitOfWorkFactory.Create())
                {
                    var coverMedia = unitOfWork.Media
                                               .Get(m => m.AlbumID == album.ID)
                                               .OrderBy(m => m.Path)
                                               .FirstOrDefault();
                    if (coverMedia != null)
                    {
                        coverMedia.AlbumOrder = 0;
                        BatchedEntities[coverMedia.Hash] = coverMedia;
                    }
                }
            }

            return Task.CompletedTask;
        }

        protected override void Save()
        {
            using (var unitOfWork = this.unitOfWorkFactory.Create())
            {
                Logger.LogDebug("Saving Album covers");
                unitOfWork.Media.BulkUpdate(BatchedEntities.Values.ToList());
                unitOfWork.Save();
            }

            BatchedEntities.Clear();
        }

        protected override void OnFinish()
        {
            this.Save();
            Logger.LogInformation("Done Registering Album covers");
        }

        #endregion
    }
}