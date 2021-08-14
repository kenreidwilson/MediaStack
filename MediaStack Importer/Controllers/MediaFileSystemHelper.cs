using System;
using System.IO;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MediaStackCore.Controllers;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;
using Microsoft.Extensions.Logging;

namespace MediaStack_Importer.Controllers
{
    public class MediaFileSystemHelper : MediaFSController, IMediaFileSystemHelper
    {
        #region Data members

        protected ILogger Logger;

        #endregion

        #region Properties

        private IDictionary<string, string> hashCache { get; }

        private IDictionary<string, int> categoryNameIdCache { get; }

        private IDictionary<string, int> artistNameIdCache { get; }

        private IDictionary<int, IDictionary<string, int>> albumArtistIdAlbumNameIdCache { get; }

        #endregion

        #region Constructors

        public MediaFileSystemHelper(ILogger logger)
        {
            this.Logger = logger;
            this.hashCache = new ConcurrentDictionary<string, string>();
            this.categoryNameIdCache = new ConcurrentDictionary<string, int>();
            this.artistNameIdCache = new ConcurrentDictionary<string, int>();
            this.albumArtistIdAlbumNameIdCache = new ConcurrentDictionary<int, IDictionary<string, int>>();
        }

        #endregion

        #region Methods

        public Media CreateMediaFromFile(string filePath, IUnitOfWork unitOfWork)
        {
            var media = new Media {
                Path = this.GetRelativePath(filePath)
            };

            try
            {
                this.FindAndSetMediaTypeAndHash(unitOfWork, filePath, media);
            }
            catch (TypeNotRecognizedException)
            {
                this.Logger.LogWarning($"Invalid type: {filePath}");
            }
            catch (Exception e)
            {
                this.Logger.LogError(e.ToString());
            }

            this.FindAndSetMediaReferences(unitOfWork, filePath, media);

            return media;
        }

        public Media UpdateMediaFromFile(string filePath, IUnitOfWork unitOfWork)
        {
            string mediaHash = this.GetFileHash(filePath);
            Media media = unitOfWork.Media.Get().First(m => m.Hash == mediaHash);

            if (File.Exists(GetMediaFullPath(media)))
            {
                this.Logger.LogWarning($"Duplicate Media Found: {media.Path}");
                return null;
            }

            media.Path = this.GetRelativePath(filePath);
            this.FindAndSetMediaReferences(unitOfWork, filePath, media);
            return media;
        }

        public string GetFileHash(string filePath, FileStream stream = null)
        {
            if (!this.hashCache.ContainsKey(this.GetRelativePath(filePath)))
            {
                using (stream ??= File.OpenRead(filePath))
                {
                    this.hashCache[this.GetRelativePath(filePath)] = this.CalculateHash(stream);
                }
            }

            return this.hashCache[this.GetRelativePath(filePath)];
        }

        public string GetRelativePath(string path)
        {
            if (path == null)
            {
                return null;
            }

            return Path.IsPathFullyQualified(path) ? path.Replace(this.MediaDirectory, "") : path;
        }

        protected void FindAndSetMediaTypeAndHash(IUnitOfWork unitOfWork, string filePath, Media media)
        {
            using (var stream = File.OpenRead(filePath))
            {
                media.Type = this.DetermineMediaType(stream);
                if (media.Type == null)
                {
                    throw new TypeNotRecognizedException();
                }

                stream.Position = 0;
                media.Hash = this.GetFileHash(filePath, stream);
                if (unitOfWork.Media.Get().Any(m => m.Hash == media.Hash))
                {
                    throw new DuplicateMediaException();
                }
            }
        }

        protected void FindAndSetMediaReferences(IUnitOfWork unitOfWork, string filePath, Media media)
        {
            var mediaReferences = this.DeriveMediaReferences(filePath);

            media.CategoryID = null;
            media.ArtistID = null;
            media.AlbumID = null;

            if (mediaReferences.Category != null)
            {
                string categoryName = mediaReferences.Category;
                media.CategoryID = this.getCategoryIdByName(categoryName, unitOfWork);
                if (media.CategoryID != null && mediaReferences.Artist != null)
                {
                    string artistName = mediaReferences.Artist;
                    media.ArtistID = this.getArtistIdByName(artistName, unitOfWork);
                    if (media.ArtistID != null && mediaReferences.Album != null)
                    {
                        string albumName = mediaReferences.Album;
                        media.AlbumID = this.getAlbumIdByNameAndArtistId((int)media.ArtistID, albumName, unitOfWork);
                    }
                }
            }
        }

        private int? getCategoryIdByName(string categoryName, IUnitOfWork unitOfWork)
        {
            if (this.categoryNameIdCache.ContainsKey(categoryName))
            {
                return this.categoryNameIdCache[categoryName];
            }

            int? id = unitOfWork.Categories.Get().FirstOrDefault(c => c.Name == categoryName)?.ID;
            if (id != null)
            {
                this.categoryNameIdCache[categoryName] = (int)id;
            }
            return id;
        }

        private int? getArtistIdByName(string artistName, IUnitOfWork unitOfWork)
        {
            if (this.artistNameIdCache.ContainsKey(artistName))
            {
                return this.artistNameIdCache[artistName];
            }

            int? id = unitOfWork.Artists.Get().FirstOrDefault(c => c.Name == artistName)?.ID;
            if (id != null)
            {
                this.artistNameIdCache[artistName] = (int)id;
            }
            return id;
        }

        private int? getAlbumIdByNameAndArtistId(int artistId, string albumName, IUnitOfWork unitOfWork)
        {
            if (this.albumArtistIdAlbumNameIdCache.ContainsKey(artistId) && this.albumArtistIdAlbumNameIdCache[artistId].ContainsKey(albumName))
            {
                return this.albumArtistIdAlbumNameIdCache[artistId][albumName];
            }
            int? id = unitOfWork.Albums.Get()
                                .FirstOrDefault(a =>
                                    a.Name == albumName && a.ArtistID == artistId)?.ID;

            if (id != null)
            {
                if (!this.albumArtistIdAlbumNameIdCache.ContainsKey(artistId))
                {
                    this.albumArtistIdAlbumNameIdCache[artistId] = new ConcurrentDictionary<string, int>();
                }
                this.albumArtistIdAlbumNameIdCache[artistId][albumName] = (int)id;
            }

            return id;
        }

        public class DuplicateMediaException : Exception { }

        public class TypeNotRecognizedException : Exception { }

        #endregion
    }
}