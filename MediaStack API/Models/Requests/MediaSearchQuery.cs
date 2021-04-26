using System.Collections.Generic;
using System.Linq;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;

namespace MediaStack_API.Models.Requests
{
    public class MediaSearchQuery
    {
        public int? CategoryID { get; set; }

        public IEnumerable<int> BlacklistCategoryIDs { get; set; }

        public int? ArtistID { get; set; }

        public IEnumerable<int> BlacklistArtistIDs { get; set; }

        public int? AlbumID { get; set; }

        public IEnumerable<int> BlacklistAlbumIDs { get; set; }

        public IEnumerable<int> WhitelistTagIDs { get; set; }

        public IEnumerable<int> BlacklistTagIDs { get; set; }

        public int? Score { get; set; }

        public int? LessThanScore { get; set; }

        public int? GreaterThanScore { get; set; }

        public int Offset { get; set; } = 0;

        public int Count { get; set; } = 5;

        public SearchMode Mode { get; set; } = SearchMode.AllMedia;

        public IQueryable<Media> GetQuery(IUnitOfWork unitOfWork)
        {
            IQueryable<Media> query = unitOfWork.Media.Get();

            switch (this.Mode)
            {
                case SearchMode.AllMedia:
                    break;
                case SearchMode.MediaAndAlbumCover:
                    // TODO
                    break;
                case SearchMode.MediaNoAlbum:
                    query = query.Where(m => m.AlbumID == null);
                    break;
            }

            if (this.CategoryID != null)
            {
                query = query.Where(m => m.CategoryID == this.CategoryID);
            }

            if (this.ArtistID != null)
            {
                query = query.Where(m => m.ArtistID == this.ArtistID);
            }

            if (this.AlbumID != null)
            {
                query = query.Where(m => m.AlbumID == this.AlbumID);
            }

            if (this.Score != null)
            {
                query = query.Where(m => m.Score == this.Score);
            }

            return query;
        }

        public enum SearchMode
        {
            AllMedia = 1,
            MediaAndAlbumCover = 2,
            MediaNoAlbum = 3
        }
    }
}
