using System.Collections.Generic;
using System.Linq;
using MediaStackCore.Controllers;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.Extensions.Logging;

namespace MediaStack_Importer.Services.ScannerService.ScannerJobs
{
    public class CreateAlbumsJob : BatchScannerJob<Album>
    {
        #region Data members

        protected IUnitOfWorkService UnitOfWorkService;

        protected IFileSystemController FSController;

        #endregion

        #region Constructors

        public CreateAlbumsJob(ILogger logger, IUnitOfWorkService unitOfWorkService, 
            IFileSystemController fsController) : base(logger)
        {
            this.FSController = fsController;
            this.UnitOfWorkService = unitOfWorkService;
        }

        #endregion

        #region Methods

        public override void Run()
        {
            this.Logger.LogDebug("Creating Albums");
            Execute(this.FSController.GetArtistAlbumNames());
        }

        protected override void ProcessData(object data)
        {
            using var unitOfWork = this.UnitOfWorkService.Create();
            if (data is string albumName)
            {
                this.Logger.LogDebug($"Processing {artistDirectory.Name}'s: Albums");
                var potentialArtist = unitOfWork.Artists.Get().FirstOrDefault(a => a.Name == artistDirectory.Name);
                if (potentialArtist != null)
                {
                    foreach (var albumName in artistDirectory.GetDirectories().Select(d => d.Name))
                    {
                        this.Logger.LogDebug($"Processing Album: {albumName}");
                        var key = $"{potentialArtist.Name}{Path.DirectorySeparatorChar}{albumName}";
                        var potentialAlbum = this.getAlbumIfNotExists(potentialArtist, albumName);
                        if (potentialAlbum != null)
                        {
                            BatchedEntities[key] = potentialAlbum;
                        }
                    }
                }
                else
                {
                    Logger.LogWarning($"Could not find Artist: {artistDirectory.Name}");
                }
            }
        }

        protected override void Save()
        {
            this.Logger.LogDebug("Saving Albums");
            using var unitOfWork = this.UnitOfWorkService.Create();
            unitOfWork.Albums.BulkInsert(BatchedEntities.Values.ToList());
            unitOfWork.Save();
            BatchedEntities.Clear();
        }

        private Album getAlbumIfNotExists(Artist artist, string albumName)
        {
            using var unitOfWork = this.UnitOfWorkService.Create();
            if (!unitOfWork.Albums.Get().Any(a => a.ArtistID == artist.ID && a.Name == albumName))
            {
                return new Album {ArtistID = artist.ID, Name = albumName};
            }

            return null;
        }

        #endregion
    }
}