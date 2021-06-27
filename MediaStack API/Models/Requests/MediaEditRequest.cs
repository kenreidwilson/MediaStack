using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaStack_API.Models.Requests
{
    public class MediaEditRequest
    {
        #region Properties

        [Required]
        public int? ID { get; set; }

        public int? CategoryID { get; set; }

        public int? ArtistID { get; set; }

        public int? AlbumID { get; set; }

        public int[] TagIDs { get; set; } = null;

        [Range(0, 5)] 
        public int? Score { get; set; }

        public string Source { get; set; }

        public int? AlbumOrder { get; set; }

        #endregion

        #region Methods

        public async Task<Media> UpdateMedia(IUnitOfWork unitOfWork)
        {
            var media = await unitOfWork.Media.Get()
                                        .Include(m => m.Tags)
                                        .FirstOrDefaultAsync(m => m.ID == this.ID);
            if (media == null)
            {
                throw new MediaNotFoundException();
            }

            if (this.CategoryID != null)
            {
                if (!await unitOfWork.Categories.Get().AnyAsync(c => c.ID == this.CategoryID))
                {
                    throw new BadRequestException();
                }
                media.CategoryID = this.CategoryID;
            }

            if (this.ArtistID != null)
            {
                if (media.CategoryID == null || !await unitOfWork.Artists.Get().AnyAsync(a => a.ID == this.ArtistID))
                {
                    throw new BadRequestException();
                }
                media.ArtistID = this.ArtistID;
            }

            if (this.AlbumID != null)
            {
                if (media.CategoryID == null || media.ArtistID == null || await unitOfWork.Albums.Get().AnyAsync(a => a.ID == this.AlbumID))
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
                List<Tag> newTags = new List<Tag>();

                foreach (int tagId in this.TagIDs)
                {
                    try
                    {
                        newTags.Add(unitOfWork.Tags.Get().First(t => t.ID == tagId));
                    }
                    catch (InvalidOperationException)
                    {
                        throw new BadRequestException();
                    }
                }

                media.Tags = newTags;

                if (this.TagIDs.Length != media.Tags.Count)
                {
                    throw new BadRequestException();
                }
            }

            return media;
        }

        public class MediaNotFoundException : Exception { }

        public class BadRequestException : Exception { }

        #endregion
    }
}