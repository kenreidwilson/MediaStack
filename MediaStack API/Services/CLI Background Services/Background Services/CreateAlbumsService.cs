using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaStackCore.Models;
using MediaStackCore.Services.MediaFilesService;
using MediaStackCore.Services.UnitOfWorkFactoryService;
using Microsoft.Extensions.Logging;

namespace MediaStack_API.Services.CLI_Background_Services.Background_Services
{
    public class CreateAlbumsService : BatchedParallelService<Album>
    {
        #region Data members

        protected IUnitOfWorkFactory unitOfWorkFactory;

        protected IMediaFilesService FSController;

        #endregion

        #region Constructors

        public CreateAlbumsService(ILogger logger, IUnitOfWorkFactory unitOfWorkFactory, 
            IMediaFilesService fsController) : base(logger)
        {
            this.FSController = fsController;
            this.unitOfWorkFactory = unitOfWorkFactory;
        }

        #endregion

        #region Methods

        public override Task Execute(CancellationToken cancellationToken)
        {
            this.Logger.LogInformation("Creating Albums");
            var artistNameAlbumNamesDictionary = this.FSController.GetArtistNameAlbumNamesDictionary();

            List<IEnumerable<string>> listOfAlbumNamesWithArtistNameFirst = new List<IEnumerable<string>>();
            foreach (string artistName in artistNameAlbumNamesDictionary.Keys)
            {
                if (!artistNameAlbumNamesDictionary[artistName].Any())
                {
                    continue;
                }
                List<string> albumNamesWithArtistNameFirst = new List<string> {
                    artistName
                };
                albumNamesWithArtistNameFirst.AddRange(artistNameAlbumNamesDictionary[artistName]);
                listOfAlbumNamesWithArtistNameFirst.Add(albumNamesWithArtistNameFirst);
            }
            return ExecuteWithData(listOfAlbumNamesWithArtistNameFirst, cancellationToken);
        }

        protected override Task ProcessData(object data)
        {
            using var unitOfWork = this.unitOfWorkFactory.Create();
            if (data is IEnumerable<string> albumNamesWithArtistNameFirst)
            {
                Artist artist = null;
                foreach (string name in albumNamesWithArtistNameFirst)
                {
                    if (artist == null)
                    {
                        artist = unitOfWork.Artists.Get().FirstOrDefault(a => a.Name == name);
                        if (artist == null)
                        {
                            Logger.LogWarning($"Could not find Artist: {name}");
                        }
                        this.Logger.LogDebug($"Processing {artist?.Name}'s: Albums");
                    }
                    else
                    {
                        this.Logger.LogDebug($"Processing Album: {name}");
                        var key = $"{artist.Name}/{name}";
                        var potentialAlbum = this.getAlbumIfNotExists(artist, name);
                        if (potentialAlbum != null)
                        {
                            BatchedEntities[key] = potentialAlbum;
                        }
                    }

                }
            }

            return Task.CompletedTask;
        }

        protected override void Save()
        {
            this.Logger.LogDebug("Saving Albums");
            using var unitOfWork = this.unitOfWorkFactory.Create();
            unitOfWork.Albums.BulkInsert(BatchedEntities.Values.ToList());
            unitOfWork.Save();
            BatchedEntities.Clear();
        }

        private Album getAlbumIfNotExists(Artist artist, string albumName)
        {
            using var unitOfWork = this.unitOfWorkFactory.Create();
            if (!unitOfWork.Albums.Get().Any(a => a.ArtistID == artist.ID && a.Name == albumName))
            {
                return new Album {ArtistID = artist.ID, Name = albumName};
            }

            return null;
        }

        #endregion
    }
}