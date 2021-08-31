using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.Extensions.Logging;

namespace MediaStack_API.Services.CLI_Background_Services.Background_Services
{
    public class OrganizeAlbumService : BatchedParallelService<Media>
    {
        protected IUnitOfWorkService UnitOfWorkService;

        public OrganizeAlbumService(ILogger logger, IUnitOfWorkService unitOfWorkService) : base(logger)
        {
            this.UnitOfWorkService = unitOfWorkService;
        }

        public override Task Execute(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Organizing Albums");
            using (IUnitOfWork unitOfWork = this.UnitOfWorkService.Create())
            {
                return ExecuteWithData(unitOfWork.Albums.Get(), cancellationToken);
            }
        }

        protected override Task ProcessData(object data)
        {
            if (data is Album album)
            {
                using (IUnitOfWork unitOfWork = this.UnitOfWorkService.Create())
                {
                    IQueryable<Media> mediaQuery = unitOfWork.Media.Get(m => m.AlbumID == album.ID);
                    if (mediaQuery.All(m => m.AlbumOrder == -1))
                    {
                        IList medias = mediaQuery.OrderBy(m => m.Path).ToList();
                        foreach (Media media in medias)
                        {
                            media.AlbumOrder = medias.IndexOf(media);
                            BatchedEntities[media.Hash] = media;
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

        protected override void Save()
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                Logger.LogDebug("Saving Organized Media");
                unitOfWork.Media.BulkUpdate(BatchedEntities.Values.ToList());
                unitOfWork.Save();
            }

            BatchedEntities.Clear();
        }
    }
}
