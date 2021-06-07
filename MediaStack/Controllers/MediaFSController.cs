using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using MediaStackCore.Models;
using MimeDetective.Extensions;

namespace MediaStackCore.Controllers
{
    public class MediaFSController : IMediaFileSystemController
    {
        #region Properties

        public string MediaDirectory { get; } = @$"{Environment.GetEnvironmentVariable("MEDIA_DIRECTORY")}";

        #endregion

        #region Constructors

        public MediaFSController()
        {
            if (string.IsNullOrEmpty(this.MediaDirectory))
            {
                throw new InvalidOperationException("Invalid Media Directory");
            }

            if (this.MediaDirectory.Last() != Path.DirectorySeparatorChar)
            {
                this.MediaDirectory += Path.DirectorySeparatorChar;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Moves the Media file to the location specified.
        /// </summary>
        /// <param name="media"></param>
        public void MoveMedia(Media media)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Moves all Media in an Album to the location specified.
        /// </summary>
        /// <param name="album"></param>
        public void MoveAlbum(Album album)
        {
            throw new NotImplementedException();
        }

        public string GetMediaFullPath(Media media)
        {
            if (media?.Path == null)
            {
                return null;
            }

            if (Path.IsPathFullyQualified(media.Path))
            {
                return media.Path;
            }

            return $@"{this.MediaDirectory}{media.Path}";
        }

        /// <summary>
        ///     Determines where the Media should be stored on disk
        ///     based on its Category, Aritst, and Album.
        /// </summary>
        /// <param name="media"></param>
        /// <returns></returns>
        public string DetermineMediaFilePath(Media media)
        {
            return $@"{this.MediaDirectory}{media.Category.Name}{media.Artist.Name}{media.Album.Name}";
        }

        /// <summary>
        ///     Returns a unique file hash from the provided stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public string CalculateHash(Stream stream)
        {
            using (var md5 = SHA1.Create())
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        public MediaType? DetermineMediaType(Stream stream)
        {
            var fileType = stream.GetFileType();

            if (fileType == null)
            {
                // webm and webp are not recognized types, this is a workaround...
                if (stream is FileStream fStream)
                {
                    string extension = fStream.Name.Substring(Math.Max(0, fStream.Name.Length - 5));
                    if (extension == ".webm")
                    {
                        return MediaType.Video;
                    }

                    if (extension == ".webp")
                    {
                        return MediaType.Image;
                    }

                }

                return null;
            }

            switch (fileType.Mime)
            {
                case "video/mp4":
                    return MediaType.Video;
                case "video/mkv":
                    return MediaType.Video;
                case "video/x-m4v":
                    return MediaType.Video;
                case "video/webm": // Not Working
                    return MediaType.Video;
                case "image/jpeg":
                    return MediaType.Image;
                case "image/png":
                    return MediaType.Image;
                case "image/webp": // Not Working
                    return MediaType.Image;
                case "image/gif":
                    return MediaType.Animated_Image;
                default:
                    return null;
            }
        }

        public dynamic DeriveMediaReferences(string filePath)
        {
            dynamic mediaReferences = new ExpandoObject();

            var path = filePath.Replace(this.MediaDirectory, "");
            var pathSplit = path.Split(Path.DirectorySeparatorChar);
            pathSplit = pathSplit.Take(pathSplit.Count() - 1).ToArray();

            mediaReferences.Category = null;
            mediaReferences.Artist = null;
            mediaReferences.Album = null;

            foreach (var name in pathSplit)
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

        #endregion
    }
}
