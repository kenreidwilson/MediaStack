using System;
using System.IO;
using System.Linq;
using MediaStack_Importer.Utility;
using MediaStackCore.Controllers;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;

namespace MediaStack_Importer.Services.ScannerService.ScannerJobs
{
    public class CreateAlbumsJob : BatchScannerJob<Album>
    {
        #region Data members

        protected IUnitOfWorkService UnitOfWorkService;

        protected IMediaFileSystemController FSController;

        #endregion

        #region Constructors

        public CreateAlbumsJob(IMediaFileSystemController fsController, IUnitOfWorkService unitOfWorkService)
        {
            this.FSController = fsController;
            this.UnitOfWorkService = unitOfWorkService;
        }

        #endregion

        #region Methods

        public void CreateAlbums()
        {
            Execute(IOUtilities.GetDirectoriesAtLevel(this.FSController.MediaDirectory, 1));
        }

        protected override void ProcessData(object data)
        {
            using var unitOfWork = this.UnitOfWorkService.Create();
            if (data is DirectoryInfo artistDirectory)
            {
                var potentialArtist = unitOfWork.Artists.Get().FirstOrDefault(a => a.Name == artistDirectory.Name);
                if (potentialArtist != null)
                {
                    foreach (var albumName in artistDirectory.GetDirectories().Select(d => d.Name))
                    {
                        var key = $"{potentialArtist.Name}{Path.DirectorySeparatorChar}{albumName}";
                        BatchedEntities[key] = this.getAlbumIfNotExists(potentialArtist, albumName);
                    }
                }
                else
                {
                    Console.WriteLine($"Could not find artist {artistDirectory.Name}");
                }
            }
        }

        protected override void Save()
        {
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