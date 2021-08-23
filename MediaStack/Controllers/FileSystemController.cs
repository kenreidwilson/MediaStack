using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Linq.Expressions;
using MediaStackCore.Models;
using MediaStackCore.Services.HasherService;
using Microsoft.Extensions.Logging;
using MimeDetective.Extensions;

namespace MediaStackCore.Controllers
{
    public class FileSystemController : IFileSystemController
    {
        #region Data members

        #endregion

        #region Properties

        protected string MediaDirectory { get; } = Environment.GetEnvironmentVariable("MEDIA_DIRECTORY");

        protected IFileSystem FileSystem { get; set; }

        protected ILogger Logger { get; set; }

        public IHasher Hasher { get; }

        #endregion

        #region Constructors

        public FileSystemController(IFileSystem fileSystem, IHasher hasher, ILogger logger)
        {
            if (string.IsNullOrEmpty(this.MediaDirectory))
            {
                throw new InvalidOperationException("Invalid Media Directory");
            }

            if (this.MediaDirectory.Last() != Path.DirectorySeparatorChar)
            {
                this.MediaDirectory += Path.DirectorySeparatorChar;
            }

            this.FileSystem = fileSystem;
            this.Hasher = hasher;
            this.Logger = logger;
        }

        #endregion

        #region Methods

        public string GetMediaDataRelativePath(MediaData data)
        {
            string path = data.Path;
            if (path == null)
            {
                return null;
            }

            return Path.IsPathFullyQualified(path) ? path.Replace(this.MediaDirectory, "") : path;
        }

        public bool DoesMediaFileExist(Media media)
        {
            throw new NotImplementedException();
        }

        public MediaData GetMediaData(Media media)
        {
            throw new NotImplementedException();
        }

        public void DeleteMediaData(Media media)
        {
            throw new NotImplementedException();
        }

        public IQueryable<MediaData> GetAllMediaData(Expression<Func<MediaData, bool>> expression = null)
        {
            throw new NotImplementedException();
        }

        public MediaData WriteMediaData(Stream mediaDataStream)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetCategoryNames(Expression<Func<string, bool>> expression = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetArtistNames(Expression<Func<string, bool>> expression = null)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, IEnumerable<string>> GetArtistNameAlbumNamesDictionary(Expression<Func<string, bool>> expression = null)
        {
            throw new NotImplementedException();
        }

        public MediaType? GetMediaDataStreamType(Stream stream)
        {
            var fileType = stream.GetFileType();

            if (fileType == null)
            {
                // webm and webp are not recognized types, this is a workaround...
                if (stream is FileStream fStream)
                {
                    var extension = fStream.Name.Substring(Math.Max(0, fStream.Name.Length - 5));
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

        public void MoveMediaFileToProperLocation(Media media)
        {
            throw new NotImplementedException();
        }

        public void MoveAlbumToProperLocation(Album album)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Moves the Media file to the location specified.
        /// </summary>
        /// <param name="media"></param>
        public string MoveMedia(Media media)
        {
            var newPath = this.determineMediaFilePath(media);
            var directoryInfo = new FileInfo(newPath).Directory;
            directoryInfo?.Create();
            File.Move(this.GetMediaFullPath(media), newPath);
            return newPath;
        }

        /// <summary>
        ///     Moves all Media in an Album to the location specified.
        /// </summary>
        /// <param name="album"></param>
        public string MoveAlbum(Album album)
        {
            var newPath = "";

            foreach (var media in album.Media)
            {
                newPath = this.MoveMedia(media);
            }

            var pathSplit = newPath.Split(Path.DirectorySeparatorChar);
            return string.Join(Path.DirectorySeparatorChar, pathSplit.Take(pathSplit.Length - 1));
        }

        /// <summary>
        ///     Determines where the Media should be stored on disk
        ///     based on its Category, Aritst, and Album.
        /// </summary>
        /// <param name="media"></param>
        /// <returns></returns>
        private string determineMediaFilePath(Media media)
        {
            if (media.Path == null)
            {
                throw new ArgumentException();
            }

            var fileName = media.Path.Split(Path.DirectorySeparatorChar).Last();
            var newPath = this.MediaDirectory;

            if (media.CategoryID != null)
            {
                newPath += $@"{media.Category.Name}{Path.DirectorySeparatorChar}";
                if (media.ArtistID != null)
                {
                    newPath += $@"{media.Artist.Name}{Path.DirectorySeparatorChar}";
                    if (media.AlbumID != null)
                    {
                        newPath += $@"{media.Album.Name}{Path.DirectorySeparatorChar}";
                    }
                }
            }

            return $@"{newPath}{fileName}";
        }

        private string GetMediaFullPath(Media media)
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

        #endregion
    }
}