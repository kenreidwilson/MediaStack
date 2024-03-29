﻿using System.Collections.Generic;
using System.Linq;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;

namespace MediaStack_API.Models.Requests
{
    public class MediaSearchQuery
    {
        public int? CategoryID { get; set; }

        public List<int> BlacklistCategoryIDs { get; set; }

        public int? ArtistID { get; set; }

        public List<int> BlacklistArtistIDs { get; set; }

        public int? AlbumID { get; set; }

        public List<int> BlacklistAlbumIDs { get; set; }

        public List<int> WhitelistTagIDs { get; set; }

        public List<int> BlacklistTagIDs { get; set; }

        public int? Score { get; set; }

        public int? LessThanScore { get; set; }

        public int? GreaterThanScore { get; set; }

        public MediaType? Type { get; set; }

        public string SortBy { get; set; }

        public int Offset { get; set; } = 0;

        public int Count { get; set; } = 5;

        public SearchMode Mode { get; set; } = SearchMode.AllMedia;

        public IQueryable<Media> GetQuery(IUnitOfWork unitOfWork)
        {
            IQueryable<Media> query = unitOfWork.Media.Get(m => m.Path != null);

            switch (this.Mode)
            {
                case SearchMode.AllMedia:
                    break;
                case SearchMode.MediaAndAlbumCover:
                    query = query.Where(m => m.AlbumID == null || (m.AlbumID != null && m.AlbumOrder == 0));
                    break;
                case SearchMode.MediaNoAlbum:
                    query = query.Where(m => m.AlbumID == null);
                    break;
            }

            if (this.CategoryID != null) query = query.Where(m => m.CategoryID == this.CategoryID);
            if (this.ArtistID != null) query = query.Where(m => m.ArtistID == this.ArtistID);
            if (this.AlbumID != null) query = query.Where(m => m.AlbumID == this.AlbumID);
            if (this.Score != null) query = query.Where(m => m.Score == this.Score);
            if (this.GreaterThanScore != null) query = query.Where(m => m.Score > this.GreaterThanScore);
            if (this.LessThanScore != null) query = query.Where(m => m.Score < this.LessThanScore);
            if (this.Type != null) query = query.Where(m => m.Type == this.Type);
            this.BlacklistAlbumIDs?.ForEach(id => query = query.Where(m => m.AlbumID != id));
            this.BlacklistArtistIDs?.ForEach(id => query = query.Where(m => m.ArtistID != id));
            this.BlacklistCategoryIDs?.ForEach(id => query = query.Where(m => m.CategoryID != id));
            
            if (this.Mode == SearchMode.MediaAndAlbumCover)
            {
                this.WhitelistTagIDs?.ForEach(
                    id => query = query.Where(m =>
                        (m.AlbumID == null && m.Tags.Select(t => t.ID).Contains(id)) ||
                        (m.AlbumID != null &&
                         m.AlbumOrder == 0 &&
                         m.Album.Media.Any(me => me.Tags.Select(t => t.ID).Contains(id)))
                    )
                );

                this.BlacklistTagIDs?.ForEach(
                    id => query = query.Where(m =>
                        (m.AlbumID == null && !m.Tags.Select(t => t.ID).Contains(id)) ||
                        (m.AlbumID != null &&
                         m.AlbumOrder == 0 &&
                         m.Album.Media.All(me => !me.Tags.Select(t => t.ID).Contains(id)))
                    )
                );
            }
            else
            {
                this.WhitelistTagIDs?.ForEach(id => query = query.Where(m => m.Tags.Select(t => t.ID).Contains(id)));
                this.BlacklistTagIDs?.ForEach(id => query = query.Where(m => !m.Tags.Select(t => t.ID).Contains(id)));
            }

            switch (this.SortBy)
            {
                case nameof(Media.Album):
                    query = query.OrderBy(m => m.AlbumID);
                    break;
                case nameof(Media.Artist):
                    query = query.OrderBy(m => m.ArtistID);
                    break;
                case nameof(Media.Category):
                    query = query.OrderBy(m => m.CategoryID);
                    break;
                case nameof(Media.Score):
                    query = query.OrderBy(m => m.Score);
                    break;
                case nameof(Media.Created):
                    query = query.OrderBy(m => m.Created);
                    break;
                case nameof(Media.Type):
                    query = query.OrderBy(m => m.Type);
                    break;
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
