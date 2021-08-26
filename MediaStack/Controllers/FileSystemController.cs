using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using MediaStackCore.Extensions;
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

            if (this.MediaDirectory.Last() != this.FileSystem.Path.DirectorySeparatorChar)
            {
                this.MediaDirectory += this.FileSystem.Path.DirectorySeparatorChar;
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

            return this.FileSystem.Path.IsPathFullyQualified(path) ? path.Replace(this.MediaDirectory, "") : path;
        }

        public bool DoesMediaFileExist(Media media)
        {
            return this.FileSystem.File.Exists(this.getMediaFullPath(media));
        }

        public MediaData GetMediaData(Media media)
        {
            return new(this.FileSystem, this.getMediaFullPath(media));
        }

        public void DeleteMediaData(Media media)
        {
            this.FileSystem.File.Delete(this.getMediaFullPath(media));
        }

        public IEnumerable<MediaData> GetAllMediaData()
        {
            IDirectoryInfo mediaDirectory = this.FileSystem.DirectoryInfo.FromDirectoryName(this.MediaDirectory);
            return mediaDirectory.GetFiles().Select(file => new MediaData(this.FileSystem, file.FullName));
        }

        public MediaData WriteMediaStream(Media media, Stream mediaDataStream)
        {
            string filePath = this.determineMediaFullFilePath(media);

            // TODO: Find a better way.
            while (this.FileSystem.File.Exists(filePath))
            {
                filePath += "_";
            }

            Stream newFileStream = this.FileSystem.File.Create(filePath);
            mediaDataStream.Seek(0, SeekOrigin.Begin);
            mediaDataStream.CopyTo(newFileStream);
            newFileStream.Close();
            return new MediaData(this.FileSystem, filePath);
        }

        public IEnumerable<string> GetCategoryNames()
        {
            return this.FileSystem.Directory.GetDirectories(this.MediaDirectory, "*", SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<string> GetArtistNames()
        {
            return this.FileSystem.GetDirectoriesAtLevel(this.MediaDirectory, 1).Select(d => d.Name);
        }

        public IDictionary<string, IEnumerable<string>> GetArtistNameAlbumNamesDictionary()
        {
            IDictionary<string, IEnumerable<string>> artistAlbumsDictionary =
                new Dictionary<string, IEnumerable<string>>();

            foreach (IDirectoryInfo artistDirectory in this.FileSystem.GetDirectoriesAtLevel(this.MediaDirectory, 1))
            {
                artistAlbumsDictionary[artistDirectory.Name] = artistDirectory.GetDirectories().Select(d => d.Name);
            }

            return artistAlbumsDictionary;
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

        public MediaData MoveMediaFileToProperLocation(Media media)
        {
            var newFullPath = this.determineMediaFullFilePath(media);
            var directoryInfo = this.FileSystem.FileInfo.FromFileName(newFullPath).Directory;
            directoryInfo?.Create();
            this.FileSystem.File.Move(this.getMediaFullPath(media), newFullPath);
            return new MediaData(this.FileSystem, newFullPath);
        }

        public IDictionary<Media, MediaData> MoveAlbumToProperLocation(Album album)
        {
            IDictionary<Media, MediaData> mediaDataDictionary = new Dictionary<Media, MediaData>();

            foreach (Media media in album.Media)
            {
                mediaDataDictionary[media] = this.MoveMediaFileToProperLocation(media);
            }

            return mediaDataDictionary;
        }

        /// <summary>
        ///     Determines where the Media should be stored on disk
        ///     based on its Category, Aritst, and Album.
        /// </summary>
        /// <param name="media"></param>
        /// <returns></returns>
        private string determineMediaFullFilePath(Media media)
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

        private string getMediaFullPath(Media media)
        {
            if (media?.Path == null)
            {
                throw new ArgumentException("Media is null or has no path.");
            }

            if (this.FileSystem.Path.IsPathFullyQualified(media.Path))
            {
                this.Logger.LogWarning($"Media \"{media.ID}\" is using full path!");
                return media.Path;
            }

            return $@"{this.MediaDirectory}{media.Path}";
        }

        #endregion
    }
}