﻿using System;
using System.IO;
using System.Linq;
using MediaStackCore.Controllers;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;

namespace MediaStack_Importer.Services
{
    public abstract class BaseImporterService
    {
        #region Data members

        protected IMediaFileSystemController FSController;

        #endregion

        #region Constructors

        protected BaseImporterService(IMediaFileSystemController controller)
        {
            this.FSController = controller;
        }

        #endregion

        #region Methods

        protected string GetRelativePath(string path)
        {
            if (path == null)
            {
                return null;
            }
            return Path.IsPathFullyQualified(path) ? path.Replace(this.FSController.MediaDirectory, "") : path;
        }

        protected Media CreateOrUpdateMediaFromFile(string filePath, IUnitOfWork unitOfWork, object unitOfWorkLock = null)
        {
            if (unitOfWorkLock == null)
            {
                unitOfWorkLock = new object();
            }

            string relativePath = this.GetRelativePath(filePath);

            Media media = null;
            using (var stream = File.OpenRead(filePath))
            {
                var hash = this.FSController.CalculateHash(stream);

                lock (unitOfWorkLock)
                {
                    media = unitOfWork.Media.Get().FirstOrDefault(m => m.Hash == hash);
                }

                if (media == null)
                {
                    var type = this.FSController.DetermineMediaType(stream);
                    if (type == null)
                    {
                        return null;
                    }

                    media = new Media {Path = relativePath, Hash = hash, Type = type};
                    lock (unitOfWorkLock)
                    {
                        unitOfWork.Media.Insert(media);
                    }
                }
                else if (relativePath.Equals(media.Path))
                {
                    return media;
                }
                else
                {
                    if (File.Exists(this.FSController.GetMediaFullPath(media)))
                    {
                        lock (unitOfWorkLock)
                        {
                            var potentialDuplicate = unitOfWork.Media
                                                               .Get()
                                                               .FirstOrDefault(m =>
                                                                   m.Path == media.Path && m.Hash == hash);
                            if (potentialDuplicate != null)
                            {
                                return null;
                            }
                        }
                    }

                    media.Path = relativePath;
                }
            }

            lock (unitOfWorkLock)
            {
                this.UpdateMedia(media, unitOfWork);
            }

            if (media.ID != 0)
            {
                lock (unitOfWorkLock)
                {
                    unitOfWork.Media.Update(media);
                }
            }

            return media;
        }

        protected Media UpdateMedia(Media media, IUnitOfWork unitOfWork)
        {
            if (media.Path == null)
            {
                throw new ArgumentException("No Media Path.");
            }

            var mediaReferences = this.FSController.DeriveMediaReferences(media.Path);

            if (mediaReferences.Category != null)
            {
                media.Category = unitOfWork.FindOrCreateCategory((string) mediaReferences.Category);
                if (mediaReferences.Artist != null)
                {
                    media.Artist = unitOfWork.FindOrCreateArtist((string) mediaReferences.Artist);
                    if (mediaReferences.Album != null)
                    {
                        media.Album = unitOfWork.FindOrCreateAlbum(media.Artist, (string) mediaReferences.Album);
                    }
                }
            }

            return media;
        }

        #endregion
    }
}