using System.Linq;
using MediaStack_Importer.Utility;
using MediaStackCore.Controllers;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;

namespace MediaStack_Importer.Services.ScannerService.ScannerJobs
{
    public class CreateArtistsJob : BatchScannerJob<Artist>
    {
        #region Data members

        protected IUnitOfWorkService UnitOfWorkService;

        protected IMediaFileSystemController FSController;

        #endregion

        #region Constructors

        public CreateArtistsJob(IMediaFileSystemController fsController, IUnitOfWorkService unitOfWorkService)
        {
            this.FSController = fsController;
            this.UnitOfWorkService = unitOfWorkService;
        }

        #endregion

        #region Methods

        public void CreateArtists()
        {
            Execute(IOUtilities.GetDirectoriesAtLevel(this.FSController.MediaDirectory, 1).Select(d => d.Name));
        }

        protected override void ProcessData(object data)
        {
            if (data is string artistName)
            {
                var potentialArtist = this.getArtistIfNotExists(artistName);
                if (potentialArtist != null)
                {
                    BatchedEntities[artistName] = potentialArtist;
                }
            }
        }

        protected override void Save()
        {
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