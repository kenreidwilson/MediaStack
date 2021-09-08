using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using MediaStackCore.Extensions;
using MediaStackCore.Models;
using MediaStackCore.Services.HasherService;
using MediaStackCore.Services.MediaTypeFinder;
using Microsoft.Extensions.Logging;

namespace MediaStackCore.Services.MediaFilesService
{
    public class MediaFilesService : IMediaFilesService
    {
        #region Properties

        protected string MediaDirectory { get; } = Environment.GetEnvironmentVariable("MEDIA_DIRECTORY");

        protected IFileSystem FileSystem { get; set; }

        protected ILogger Logger { get; set; }

        public IHasher Hasher { get; }

        public IMediaTypeFinder TypeFinder { get; }

        #endregion

        #region Constructors

        public MediaFilesService(IFileSystem fileSystem, IHasher hasher, IMediaTypeFinder typeFinder,
            ILogger<MediaFilesService> logger)
        {
            this.FileSystem = fileSystem;

            if (string.IsNullOrEmpty(this.MediaDirectory))
            {
                throw new InvalidOperationException("Invalid Media Directory");
            }

            if (this.MediaDirectory.Last() != this.FileSystem.Path.DirectorySeparatorChar)
            {
                this.MediaDirectory += this.FileSystem.Path.DirectorySeparatorChar;
            }

            this.Hasher = hasher;
            this.TypeFinder = typeFinder;
            this.Logger = logger;
        }

        #endregion

        #region Methods

        public string GetRelativePath(IFileInfo mediaFile)
        {
            return mediaFile.FullName.Replace(this.MediaDirectory, "");
        }

        public bool DoesMediaFileExist(Media media)
        {
            return this.GetMediaFileInfo(media).Exists;
        }

        public IFileInfo GetMediaFileInfo(Media media)
        {
            return this.FileSystem.FileInfo.FromFileName(this.getMediaFullPath(media));
        }

        public void DeleteMediaFile(Media media)
        {
            this.FileSystem.File.Delete(this.getMediaFullPath(media));
        }

        public IEnumerable<IFileInfo> GetAllMediaFiles()
        {
            return this.FileSystem.DirectoryInfo.FromDirectoryName(this.MediaDirectory)
                       .GetFiles("*.*", SearchOption.AllDirectories);
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

            foreach (var artistDirectory in this.FileSystem.GetDirectoriesAtLevel(this.MediaDirectory, 1))
            {
                var albumNames = artistDirectory.GetDirectories().Select(d => d.Name);
                if (!artistAlbumsDictionary.ContainsKey(artistDirectory.Name))
                {
                    artistAlbumsDictionary[artistDirectory.Name] = albumNames;
                }
                else
                {
                    IList<string> currentAlbumNames = artistAlbumsDictionary[artistDirectory.Name].ToList();
                    artistAlbumsDictionary[artistDirectory.Name] = currentAlbumNames
                        .Concat(albumNames.Where(name => !currentAlbumNames.Contains(name)));
                }
            }

            return artistAlbumsDictionary;
        }

        public string GetCategoryName(IFileInfo mediaFile)
        {
            return this.GetRelativePath(mediaFile).Split($"{this.FileSystem.Path.DirectorySeparatorChar}")
                       .FirstOrDefault();
        }

        public string GetArtistName(IFileInfo mediaFile)
        {
            return this.GetRelativePath(mediaFile).Split($"{this.FileSystem.Path.DirectorySeparatorChar}")
                       .Skip(1)
                       .FirstOrDefault();
        }

        public string GetAlbumName(IFileInfo mediaFile)
        {
            return this.GetRelativePath(mediaFile).Split($"{this.FileSystem.Path.DirectorySeparatorChar}")
                       .Skip(2)
                       .FirstOrDefault();
        }

        public IFileInfo WriteMediaFileStream(Stream mediaFileStream, Media media = null)
        {
            var filePath = media == null
                ? $@"{this.MediaDirectory}{this.Hasher.CalculateHash(mediaFileStream)}"
                : this.determineMediaFullFilePath(media);

            // TODO: Find a better way.
            while (this.FileSystem.File.Exists(filePath))
            {
                filePath += "_";
            }

            var newFileStream = this.FileSystem.File.Create(filePath);
            mediaFileStream.Seek(0, SeekOrigin.Begin);
            mediaFileStream.CopyTo(newFileStream);
            newFileStream.Close();
            return this.FileSystem.FileInfo.FromFileName(filePath);
        }

        public IFileInfo MoveMediaFileToProperLocation(Media media)
        {
            var newFullPath = this.determineMediaFullFilePath(media);
            var directoryInfo = this.FileSystem.FileInfo.FromFileName(newFullPath).Directory;
            directoryInfo?.Create();
            this.FileSystem.File.Move(this.getMediaFullPath(media), newFullPath);
            return this.FileSystem.FileInfo.FromFileName(newFullPath);
        }

        public IDictionary<Media, IFileInfo> MoveAlbumToProperLocation(Album album)
        {
            IDictionary<Media, IFileInfo> mediaDataDictionary = new Dictionary<Media, IFileInfo>();

            foreach (var media in album.Media)
            {
                mediaDataDictionary[media] = this.MoveMediaFileToProperLocation(media);
            }

            return mediaDataDictionary;
        }

        private string determineMediaFullFilePath(Media media)
        {
            if (media.Path == null)
            {
                throw new ArgumentException();
            }

            var fileName = media.Path.Split(this.FileSystem.Path.DirectorySeparatorChar).Last();
            var newPath = this.MediaDirectory;

            if (media.CategoryID != null)
            {
                newPath += $@"{media.Category.Name}{this.FileSystem.Path.DirectorySeparatorChar}";
                if (media.ArtistID != null)
                {
                    newPath += $@"{media.Artist.Name}{this.FileSystem.Path.DirectorySeparatorChar}";
                    if (media.AlbumID != null)
                    {
                        newPath += $@"{media.Album.Name}{this.FileSystem.Path.DirectorySeparatorChar}";
                    }
                }
            }

            return $@"{newPath}{fileName}";
        }

        private string getMediaFullPath(Media media)
        {
            return $"{this.MediaDirectory}{media.Path}";
        }

        #endregion
    }
}