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
        //public static string THUMBNAIL_DIRECTORY = @$"{Environment.GetEnvironmentVariable("THUMBNAIL_DIRECTORY")}";

        private readonly object unitOfWorkLock = new object();

        public MediaFSController()
        {
            if (MEDIA_DIRECTORY[0] != Path.DirectorySeparatorChar)
            {
                MEDIA_DIRECTORY += Path.DirectorySeparatorChar;
            }
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
        public Media CreateOrUpdateMediaFromFile(string filePath, IUnitOfWork unitOfWork)
        {
            string relativePath = null;
            if (Path.IsPathFullyQualified(filePath))
            {
                relativePath = filePath.Replace(MEDIA_DIRECTORY, "");
            }
            else
            {
                relativePath = filePath;
            }

            Media media = null;
            using (var stream = File.OpenRead(filePath))
            {
                string hash = calculateHash(stream);

                lock (unitOfWorkLock)
                {
                    media = unitOfWork.Media.Get().Where(media => media.Hash == hash).FirstOrDefault();
                }

                if (media == null)
                {
                    MediaType? type = determineMediaType(stream);
                    if (type == null)
                    {
                        return null;
                    }

                    media = new Media { Path = relativePath, Hash = hash, Type = type };
                    lock (unitOfWorkLock)
                    {
                        if (unitOfWork.Media.GetLocal().Where(media => media.Hash == hash).FirstOrDefault() != null)
                        {
                            return null;
                        }
                        unitOfWork.Media.Insert(media);
                    }
                }
                else if (relativePath.Equals(media.Path))
                {
                    return media;
                }
                else
                {
                    if (File.Exists(GetMediaFullPath(media)))
                    {
                        lock (unitOfWorkLock)
                        {
                            Media potentialDuplicate = unitOfWork.Media.Get()
                                .Where(m => m.Path == media.Path && m.Hash == hash)
                                .FirstOrDefault();
                            if (potentialDuplicate != null)
                            {
                                return null;
                            }
                        }
                    }
                    media.Path = relativePath;
                }
            }

            this.UpdateMedia(media, unitOfWork);

            if (media.ID != 0)
            {
                lock (unitOfWorkLock)
                {
                    unitOfWork.Media.Update(media);
                }
            }

            return media;
        }

        /// <summary>
        ///     Finds/Creates Category, Artist, Album derived from filePath.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>Updated Media</returns>
        public Media UpdateMedia(Media media, IUnitOfWork unitOfWork)
        {
            if (media.Path == null)
            {
                throw new ArgumentException("No Media Path.");
            }

            dynamic mediaReferences = this.deriveMediaReferences(media.Path);

            if (mediaReferences.Category != null)
            {
                media.Category = this.findOrCreateCategory(mediaReferences.Category, unitOfWork);
                if (mediaReferences.Artist != null)
                {
                    media.Artist = this.findOrCreateArtist(mediaReferences.Artist, unitOfWork);
                    if (mediaReferences.Album != null)
                    {
                        media.Album = this.findOrCreateAlbum(mediaReferences.Album, media.Artist, unitOfWork);
                    }
                }
            }

            return media;
        }

        public void Save(IUnitOfWork unitOfWork)
        {
            lock (unitOfWorkLock)
            {
                unitOfWork.Save();
            }
        }

        public void DisableMedia(Media media, IUnitOfWork unitOfWork)
        {
            media.Path = null;
            lock(unitOfWorkLock)
            {
                unitOfWork.Media.Update(media);
            }
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

        public static string GetMediaFullPath(Media media)
        {
            if (media != null && media.Path != null && Path.IsPathFullyQualified(media.Path))
            {
                return media.Path;
            }
            return $@"{MEDIA_DIRECTORY}{media.Path}";
        }

        /// <summary>
        ///     Determines where the Media should be stored on disk
        ///     based on its Category, Aritst, and Album.
        /// </summary>
        /// <param name="media"></param>
        /// <returns></returns>
        private string DetermineMediaFilePath(Media media)
        {
            return $@"{MEDIA_DIRECTORY}{media.Category.Name}{media.Artist.Name}{media.Album.Name}";
        }

        /// <summary>
        ///     Returns a unique file hash from the provided stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static string calculateHash(FileStream stream)
        {
            using (var md5 = SHA1.Create())
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        private static MediaType? determineMediaType(FileStream stream)
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
                case ("video/x-m4v"):
                    return MediaType.Video;
                case ("image/jpeg"):
                    return MediaType.Image;
                case ("image/png"):
                    return MediaType.Image;
                case ("image/gif"):
                    return MediaType.Animated_Image;
                default:
                    return null;
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

        private Category findOrCreateCategory(string name, IUnitOfWork unitOfWork)
        {
            lock (unitOfWorkLock)
            {
                Category category = unitOfWork.Categories.Get()
                    .Where(category => category.Name == name)
                    .FirstOrDefault();

                if (category == null)
                {
                    category = unitOfWork.Categories.GetLocal()
                        .Where(category => category.Name == name)
                        .FirstOrDefault();
                }

                if (category == null)
                {
                    category = new Category { Name = name };
                    unitOfWork.Categories.Insert(category);
                }

                return category;
            }
        }

        private Artist findOrCreateArtist(string name, IUnitOfWork unitOfWork)
        {
            lock (unitOfWorkLock)
            {
                Artist artist = unitOfWork.Artists.Get()
                    .Where(artist => artist.Name == name)
                    .FirstOrDefault();

                if (artist == null)
                {
                    artist = unitOfWork.Artists.GetLocal()
                        .Where(artist => artist.Name == name)
                        .FirstOrDefault();
                }

                if (artist == null)
                {
                    artist = new Artist { Name = name };
                    unitOfWork.Artists.Insert(artist);
                }

                return artist;
            }
        }

        private Album findOrCreateAlbum(string name, Artist artist, IUnitOfWork unitOfWork)
        {
            lock (unitOfWorkLock)
            {
                Album album = unitOfWork.Albums.Get()
                    .Where(album => album.Name == name && album.ArtistID == artist.ID)
                    .FirstOrDefault();

                if (album == null)
                {
                    album = unitOfWork.Albums.GetLocal()
                        .Where(album => album.Name == name)
                        .FirstOrDefault();
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
}
