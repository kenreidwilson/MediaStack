using MediaStack_Library.Data_Access_Layer;
using MediaStack_Library.Model;
using MimeDetective.Extensions;
using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace MediaStack_Library.Utility
{
    public class MediaFSController
    {
        public static string MEDIA_DIRECTORY = "";
        public static string THUMBNAIL_DIRECTORY = "";

        protected IUnitOfWork unitOfWork;
        protected Thumbnailer thumbnailer = new Thumbnailer();

        public MediaFSController(IUnitOfWork unitOfWork)
        {
            // TODO: Verifiy Media and Thumbnail directories.
            this.unitOfWork = unitOfWork;
        }

        /// <summary>
        ///     Uses provided filePath to do the following:
        ///         Hash the file.
        ///         Determine file type.
        ///         Find/Create Category derived from filePath.
        ///         Find/Create Artist derived from filePath.
        ///         Find/Create Album derived from filePath.
        ///         Create thumbnail.
        ///     Returns null if file cannot be hashed or thumbnailed.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>Media.</returns>
        public Media InitialMedia(string filePath)
        {
            Media media = new Media{ Path = filePath };

            if (!this.thumbnailer.CreateThumbnail(media))
            {
                return null;
            }

            using (var stream = File.OpenRead(filePath))
            {
                media.Hash = CalculateHash(stream);
                media.Type = (MediaType) determineMediaType(stream);
            }

            dynamic mediaReferences = this.deriveMediaReferences(filePath);

            media.Category = this.findOrCreateCategory(mediaReferences.Category);
            media.Artist = this.findOrCreateArtist(mediaReferences.Artist);
            media.Album = this.findOrCreateAlbum(mediaReferences.Album);

            return media;
        }

        public string DetermineMediaPath(Media media)
        {
            return null;
        }

        private dynamic deriveMediaReferences(string filePath)
        {
            dynamic mediaReferences = new ExpandoObject();

            string path = filePath.Replace(MEDIA_DIRECTORY, "");
            string[] pathSplit = path.Split(Path.DirectorySeparatorChar);
            pathSplit.Select(name => name != pathSplit.Last());

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

        public static string CalculateHash(FileStream stream)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        private static MediaType? determineMediaType(FileStream stream)
        {
            switch(stream.GetFileType().Mime)
            {
                default:
                    return MediaType.Image;
            }
        }

        private Category findOrCreateCategory(string name)
        {
            Category category = this.unitOfWork.Categories.Get()
                .Where(category => category.Name == name)
                .FirstOrDefault();

            if (category == null)
            {
                category = new Category { Name = name };
            }

            this.unitOfWork.Categories.Insert(category);
            return category;
        }

        private Album findOrCreateAlbum(string name)
        {
            Album album = this.unitOfWork.Albums.Get()
                .Where(album => album.Name == name)
                .FirstOrDefault();

            if (album == null)
            {
                album = new Album { Name = name };
            }

            this.unitOfWork.Albums.Insert(album);
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
            }

            this.unitOfWork.Artists.Insert(artist);
            return artist;
        }
    }
}
