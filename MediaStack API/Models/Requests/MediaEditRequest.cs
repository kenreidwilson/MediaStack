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

        public int? CategoryID { get; set; } = 0;

        public int? ArtistID { get; set; } = 0;

        public int? AlbumID { get; set; } = 0;

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
                                        .AsTracking()
                                        .FirstOrDefaultAsync(m => m.ID == this.ID);
            if (media == null)
            {
                throw new MediaNotFoundException();
            }

            if (this.CategoryID != 0)
            {
                Category newCategory = null;
                if (this.CategoryID != null)
                {
                    newCategory = await unitOfWork.Categories.Get().FirstOrDefaultAsync(c => c.ID == this.CategoryID);
                    if (newCategory == null)
                    {
                        throw new BadRequestException();
                    }
                }

                media.Category = newCategory;
                media.CategoryID = newCategory?.ID;
            }

            if (this.ArtistID != 0)
            {
                Artist newArtist = null;
                if (this.ArtistID != null)
                {
                    if (media.CategoryID == null)
                    {
                        throw new BadRequestException();
                    }
                    newArtist = await unitOfWork.Artists.Get().FirstOrDefaultAsync(a => a.ID == this.ArtistID);
                    if (newArtist == null)
                    {
                        throw new BadRequestException();
                    }
                }

                media.Artist = newArtist;
                media.ArtistID = newArtist?.ID;
            }

            if (this.AlbumID != 0)
            {
                Album newAlbum = null;
                if (this.AlbumID != null)
                {
                    if (media.CategoryID == null || media.ArtistID == null)
                    {
                        throw new BadRequestException();
                    }
                    newAlbum = await unitOfWork.Albums.Get().FirstOrDefaultAsync(a => a.ID == this.AlbumID);
                    if (newAlbum == null || newAlbum.ArtistID != this.ArtistID)
                    {
                        throw new BadRequestException();
                    }
                }

                media.Album = newAlbum;
                media.AlbumID = newAlbum?.ID;
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
                ICollection<Tag> newMediaTags = media.Tags.Where(t => this.TagIDs.Contains(t.ID)).ToList();
                try
                {
                    foreach (int id in this.TagIDs)
                    {
                        if (!media.Tags.Select(t => t.ID).Contains(id))
                        {
                            newMediaTags.Add(unitOfWork.Tags.Get().First(t => t.ID == id));
                        }
                    }

                    media.Tags = newMediaTags;
                }
                catch (InvalidOperationException)
                {
                    throw new BadRequestException();
                }

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