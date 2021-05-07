using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaStackCore.Controllers;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.Extensions.Logging;

namespace MediaStack_Importer.Services.ScannerService.ScannerJobs
{
    public abstract class MediaScannerJob : BatchScannerJob<Media>
    {
        #region Data members

        protected IUnitOfWorkService UnitOfWorkService;

        protected IMediaFileSystemController FSController;

        #endregion

        #region Properties

        public IDictionary<string, string> HashCache { get; set; }

        #endregion

        #region Constructors

        protected MediaScannerJob(ILogger logger, IMediaFileSystemController fsController, IUnitOfWorkService unitOfWorkService) : base(logger)
        {
            this.FSController = fsController;
            this.UnitOfWorkService = unitOfWorkService;
            this.HashCache = new ConcurrentDictionary<string, string>();
        }

        #endregion

        #region Methods

        protected Media CreateMediaFromFile(string filePath, IUnitOfWork unitOfWork)
        {
            var media = new Media {
                Path = this.GetRelativePath(filePath)
            };

            try
            {
                this.FindAndSetMediaTypeAndHash(unitOfWork, filePath, media);
            }
            catch (DuplicateMediaException)
            {
                this.Logger.LogWarning($"Duplicate file: {filePath}");
                var fileHash = this.GetFileHash(this.FSController.GetMediaFullPath(media));
                media = unitOfWork.Media.Get().FirstOrDefault(m => m.Hash == fileHash);
                if (media != null)
                {
                    media.Path = this.GetRelativePath(filePath);
                    return media;
                }
            }
            catch (TypeNotRecognizedException)
            {
                this.Logger.LogWarning($"Could not determine type for: {filePath}");
            }
            catch (Exception e)
            {
                this.Logger.LogError(e.ToString());
            }

            this.FindAndSetMediaReferences(unitOfWork, filePath, media);

            return media;
        }

        protected void FindAndSetMediaTypeAndHash(IUnitOfWork unitOfWork, string filePath, Media media)
        {
            using (var stream = File.OpenRead(filePath))
            {
                media.Type = this.FSController.DetermineMediaType(stream);
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
            var mediaReferences = this.FSController.DeriveMediaReferences(filePath);

            if (mediaReferences.Category != null)
            {
                string categoryName = mediaReferences.Category;
                media.CategoryID = unitOfWork.Categories.Get().FirstOrDefault(c => c.Name == categoryName)?.ID;
                if (media.CategoryID != null && mediaReferences.Artist != null)
                {
                    string artistName = mediaReferences.Artist;
                    media.ArtistID = unitOfWork.Artists.Get().FirstOrDefault(a => a.Name == artistName)?.ID;
                    if (media.ArtistID != null && mediaReferences.Album != null)
                    {
                        string albumName = mediaReferences.Album;
                        media.AlbumID = unitOfWork.Albums.Get()
                                                  .FirstOrDefault(a =>
                                                      a.Name == albumName && a.ArtistID == media.ArtistID)?.ID;
                    }
                }
            }
        }

        protected string GetFileHash(string filePath, FileStream stream = null)
        {
            if (!this.HashCache.ContainsKey(this.GetRelativePath(filePath)))
            {
                using (stream ??= File.OpenRead(filePath))
                {
                    this.HashCache[this.GetRelativePath(filePath)] = this.FSController.CalculateHash(stream);
                }
            }

            return this.HashCache[this.GetRelativePath(filePath)];
        }

        protected void AddMedia(Media media)
        {
            if (media == null)
            {
                return;
            }

            if (!BatchedEntities.ContainsKey(media.Hash))
            {
                BatchedEntities[media.Hash] = media;
            }
        }

        protected string GetRelativePath(string path)
        {
            if (path == null)
            {
                return null;
            }

            return Path.IsPathFullyQualified(path) ? path.Replace(this.FSController.MediaDirectory, "") : path;
        }

        protected class DuplicateMediaException : Exception { }

        protected class TypeNotRecognizedException : Exception { }

        #endregion
    }
}
