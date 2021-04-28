using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;

namespace MediaStack_API.Models.Requests
{
    public class MediaEditRequest
    {
        #region Properties

        public int? CategoryID { get; set; }

        public int? ArtistID { get; set; }

        public int? AlbumID { get; set; }

        public List<int> TagIDs { get; set; }

        [Range(0, 5)] 
        public int? Score { get; set; }

        public string Source { get; set; }

        public int? AlbumOrder { get; set; }

        #endregion

        #region Methods

        public Media UpdateMedia(IUnitOfWork unitOfWork, Media media)
        {
            if (this.CategoryID != null)
            {
                if (!unitOfWork.Categories.Get().Any(c => c.ID == this.CategoryID))
                {
                    throw new BadRequestException();
                }
                media.CategoryID = this.CategoryID;
            }

            if (this.ArtistID != null)
            {
                if (media.CategoryID == null || !unitOfWork.Artists.Get().Any(a => a.ID == this.ArtistID))
                {
                    throw new BadRequestException();
                }
                media.ArtistID = this.ArtistID;
            }

            if (this.AlbumID != null)
            {
                if (media.CategoryID == null || media.ArtistID == null || unitOfWork.Albums.Get().Any(a => a.ID == this.AlbumID))
                {
                    throw new BadRequestException();
                }
                media.AlbumID = this.AlbumID;
            }

            if (this.AlbumOrder != null)
            {
                if (media.AlbumID == null)
                {
                    throw new BadRequestException();
                }

                media.AlbumOrder = (int) this.AlbumOrder;
            }

            if (this.Score != null) media.Score = (int) this.Score;
            if (this.Source != null) media.Source = this.Source;

            if (this.TagIDs != null)
            {
                media.Tags.Clear();
                media.Tags = unitOfWork.Tags.Get(t => this.TagIDs.Contains(t.ID)).ToList();
                if (this.TagIDs.Count != media.Tags.Count)
                {
                    throw new BadRequestException();
                }
            }
            return media;
        }

        public class BadRequestException : Exception { }

        #endregion
    }
}