using MediaStack_Library.Data_Access_Layer;
using MediaStack_Library.Model;
using MediaStack_Library.Utility;
using MimeDetective.Extensions;
using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace MediaStack_Library.Controllers
{
    public class MediaFSController
    {
        public static string MEDIA_DIRECTORY = @$"{Environment.GetEnvironmentVariable("MEDIA_DIRECTORY")}";
        public static string THUMBNAIL_DIRECTORY = @$"{Environment.GetEnvironmentVariable("THUMBNAIL_DIRECTORY")}";

        protected IUnitOfWork unitOfWork;
        protected Thumbnailer thumbnailer = new Thumbnailer();

        public MediaFSController(IUnitOfWork unitOfWork)
        {
            // TODO: Verifiy Media and Thumbnail directories.
            this.unitOfWork = unitOfWork;
        }

        /// <summary>
        ///     Uses provided filePath to initialize a uniquely hashed media:
        ///         Hash the file.
        ///         Determine file type.
        ///         Creates thumbnail.
        ///         Finds/Creates Category, Artist, Album derived from filePath.
        ///     Returns null if file cannot be hashed or thumbnailed.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>Media or null.</returns>
        public Media InitializeMedia(string filePath, string hash = null)
        {
            Media media = new Media{ Path = filePath };

            using (var stream = File.OpenRead(filePath))
            {
                media.Hash = hash == null ? CalculateHash(stream) : hash;
                media.Type = DetermineMediaType(stream);
                if (media.Type == null)
                {
                    return null;
                }
            }

            if (!File.Exists(GetMediaThumbnailPath(media)))
            {
                if (!this.thumbnailer.CreateThumbnail(media))
                {
                    return null;
                }
            }

            this.UpdateMedia(media, filePath);

            if (media.ID == 0)
            {
                this.unitOfWork.Media.Insert(media);
            }
            else
            {
                this.unitOfWork.Media.Update(media);
            }

            return media;
        }

        /// <summary>
        ///     Finds/Creates Category, Artist, Album derived from filePath.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>Updated Media</returns>
        public Media UpdateMedia(Media media, string filePath = null)
        {
            if (filePath != null)
            {
                media.Path = filePath;
            }

            if (media.Path == null)
            {
                throw new ArgumentException("No Media Path.");
            }

            dynamic mediaReferences = this.deriveMediaReferences(filePath);

            if (mediaReferences.Category != null)
            {
                media.Category = this.findOrCreateCategory(mediaReferences.Category);
                if (mediaReferences.Artist != null)
                {
                    media.Artist = this.findOrCreateArtist(mediaReferences.Artist);
                    if (mediaReferences.Album != null)
                    {
                        media.Album = this.findOrCreateAlbum(mediaReferences.Album, media.Artist);
                    }
                }
            }

            return media;
        }

        /// <summary>
        ///     Moves the Media file to the location specified.
        /// </summary>
        /// <param name="media"></param>
        public void MoveMedia(Media media)
        {
            // TODO: Implement.
            return;
        }

        /// <summary>
        ///     Moves all Media in an Album to the location specified.
        /// </summary>
        /// <param name="album"></param>
        public void MoveAlbum(Album album)
        {
            // TODO: Implement.
            return;  
        }

        /// <summary>
        ///     Determines where the Media should be stored on disk
        ///     based on its Category, Aritst, and Album.
        /// </summary>
        /// <param name="media"></param>
        /// <returns></returns>
        public static string GetMediaFilePath(Media media)
        {
            return $@"{MEDIA_DIRECTORY}{media.Category.Name}{media.Artist.Name}{media.Album.Name}";
        }

        /// <summary>
        ///     Determins where the Media's thumbnail should be stored
        ///     based on its hash.
        /// </summary>
        /// <param name="media"></param>
        /// <returns></returns>
        public static string GetMediaThumbnailPath(Media media)
        {
            return $@"{THUMBNAIL_DIRECTORY}{Path.DirectorySeparatorChar}{media.Hash}";
        }

        /// <summary>
        ///     Returns a unique file hash from the provided stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string CalculateHash(FileStream stream)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        public static MediaType? DetermineMediaType(FileStream stream)
        {
            var fileType = stream.GetFileType();

            if (fileType == null)
            {
                return null;
            }

            switch (fileType.Mime)
            {
                case ("video/mp4"):
                    return MediaType.Video;
                case ("video/mkv"):
                    return MediaType.Video;
                default:
                    return MediaType.Image;
            }
        }

        private dynamic deriveMediaReferences(string filePath)
        {
            dynamic mediaReferences = new ExpandoObject();

            string path = filePath.Replace(MEDIA_DIRECTORY, "");
            string[] pathSplit = path.Split(Path.DirectorySeparatorChar);
            pathSplit = pathSplit.Take(pathSplit.Count() - 1).ToArray();

            mediaReferences.Category = null;
            mediaReferences.Artist = null;
            mediaReferences.Album = null;

            foreach (string name in pathSplit)
            {
                if (mediaReferences.Category == null)
                {
                    mediaReferences.Category = name;
                } 
                else if (mediaReferences.Artist == null)
                {
                    mediaReferences.Artist = name;
                } 
                else if (mediaReferences.Album == null)
                {
                    mediaReferences.Album = name;
                }
            }

            return mediaReferences;
        }

        private Category findOrCreateCategory(string name)
        {
            Category category = this.unitOfWork.Categories.Get()
                .Where(category => category.Name == name)
                .FirstOrDefault();

            if (category == null)
            {
                category = new Category { Name = name };
                this.unitOfWork.Categories.Insert(category);
            }

            return category;
        }

        private Album findOrCreateAlbum(string name, Artist artist)
        {
            Album album = this.unitOfWork.Albums.Get()
                .Where(album => album.Name == name && album.ArtistID == artist.ID)
                .FirstOrDefault();

            if (album == null)
            {
                album = new Album { Name = name, Artist = artist };
                this.unitOfWork.Albums.Insert(album);
            }

            return album;
        }

        private Artist findOrCreateArtist(string name)
        {
            Artist artist = this.unitOfWork.Artists.Get()
                .Where(artist => artist.Name == name)
                .FirstOrDefault();

            if (artist == null)
            {
                artist = new Artist { Name = name };
                this.unitOfWork.Artists.Insert(artist);
            }

            return artist;
        }
    }
}
