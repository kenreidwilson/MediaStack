using System.Linq;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;

namespace MediaStack_API.Models.Requests
{
    public class AlbumSearchQuery
    {
        #region Properties

        public int Count { get; set; } = 5;

        public int Offset { get; set; } = 0;

        public int? ArtistID { get; set; }

        public string Name { get; set; }

        public string FuzzyName { get; set; }

        #endregion

        #region Methods

        public IQueryable<Album> GetQuery(IUnitOfWork unitOfWork)
        {
            var query = unitOfWork.Albums.Get();

            if (this.ArtistID != null) query = query.Where(a => a.ArtistID == this.ArtistID);
            if (this.Name != null) query = query.Where(a => a.Name == this.Name);
            if (this.FuzzyName != null) query = query.Where(a => a.Name.Contains(this.FuzzyName));

            return query;
        }

        #endregion
    }
}
