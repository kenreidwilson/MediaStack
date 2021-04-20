using System.Linq;
using MediaStackCore.Models;

namespace MediaStackCore.Data_Access_Layer
{
    public static class UnitOfWorkExtensions
    {
        public static void DisableMedia(this IUnitOfWork unitOfWork, Media media)
        {
            media.Path = null;
        }

        public static bool IsMediaDisabled(this IUnitOfWork unitOfWor, Media media)
        {
            return media.Path == null;
        }

        public static Category FindOrCreateCategory(this IUnitOfWork unitOfWork, string name)
        {
            Category category = unitOfWork.Categories
                                          .Get()
                                          .FirstOrDefault(c => c.Name == name);

            if (category == null)
            {
                category = unitOfWork.Categories
                                     .GetLocal()
                                     .FirstOrDefault(c => c.Name == name);
            }

            if (category == null)
            {
                category = new Category { Name = name };
                unitOfWork.Categories.Insert(category);
            }

            return category;
        }

        public static Artist FindOrCreateArtist(this IUnitOfWork unitOfWork, string name)
        {
            Artist artist = unitOfWork.Artists
                                      .Get()
                                      .FirstOrDefault(a => a.Name == name);

            if (artist == null)
            {
                artist = unitOfWork.Artists
                                   .GetLocal()
                                   .FirstOrDefault(a => a.Name == name);
            }

            if (artist == null)
            {
                artist = new Artist { Name = name };
                unitOfWork.Artists.Insert(artist);
            }

            return artist;
        }

        public static Album FindOrCreateAlbum(this IUnitOfWork unitOfWork, Artist artist, string name)
        {
            Album album = unitOfWork.Albums
                                    .Get()
                                    .FirstOrDefault(a => a.Name == name && a.ArtistID == artist.ID);

            if (album == null)
            {
                album = unitOfWork.Albums
                                  .GetLocal()
                                  .FirstOrDefault(a => a.Name == name);
            }

            if (album == null)
            {
                album = new Album { Name = name, Artist = artist };
                unitOfWork.Albums.Insert(album);
            }

            return album;
        }
    }
}
