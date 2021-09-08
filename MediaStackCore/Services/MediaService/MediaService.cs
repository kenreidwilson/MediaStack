using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaStackCore.Models;
using MediaStackCore.Services.MediaFilesService;
using MediaStackCore.Services.UnitOfWorkFactoryService;
using Microsoft.EntityFrameworkCore;

namespace MediaStackCore.Services.MediaService
{
    public class MediaService : IMediaService
    {
        #region Data members

        protected IMediaFilesService MediaFilesService;

        protected IUnitOfWorkFactory UnitOfWorkFactory;

        #endregion

        #region Constructors

        public MediaService(IUnitOfWorkFactory unitOfWorkFactory, IMediaFilesService mediaFilesService)
        {
            this.MediaFilesService = mediaFilesService;
            this.UnitOfWorkFactory = unitOfWorkFactory;
        }

        #endregion

        #region Methods

        public Media DisableMedia(Media media)
        {
            media.Path = null;
            return media;
        }

        public bool IsMediaDisabled(Media media)
        {
            return media.Path == null;
        }

        public Media CreateMedia(IFileInfo mediaFile)
        {
            using (var mediaDataStream = mediaFile.OpenRead())
            {
                var type = this.MediaFilesService.TypeFinder.GetMediaFileStreamType(mediaDataStream);

                if (type == null)
                {
                    throw new ArgumentException("Invalid Media Type");
                }

                using (var unitOfWork = this.UnitOfWorkFactory.Create())
                {
                    var artistId = unitOfWork.Artists.Get()
                                             .FirstOrDefault(a =>
                                                 a.Name == this.MediaFilesService.GetArtistName(mediaFile))?.ID;
                    return new Media() {
                        CategoryID = unitOfWork.Categories.Get()
                                               .FirstOrDefault(c =>
                                                   c.Name == this.MediaFilesService.GetCategoryName(mediaFile))?.ID,
                        ArtistID = artistId,
                        AlbumID = unitOfWork.Albums.Get().FirstOrDefault(a =>
                            a.ArtistID == artistId && a.Name == this.MediaFilesService.GetAlbumName(mediaFile))?.ID,
                        Path = this.MediaFilesService.GetRelativePath(mediaFile),
                        Hash = this.MediaFilesService.Hasher.CalculateHash(mediaDataStream, mediaFile.FullName),
                        Type = type
                    };
                }
            }
        }

        public async Task<Media> CreateMediaAsync(IFileInfo mediaFile)
        {
            var tokenSource = new CancellationTokenSource();

            string hash = null;
            MediaType? type = null;

            var hashAndTypeTask = Task.Run(async () =>
            {
                await using (var mediaStream = mediaFile.OpenRead())
                {
                    type = this.MediaFilesService.TypeFinder.GetMediaFileStreamType(mediaStream);

                    if (type == null)
                    {
                        tokenSource.Cancel();
                        return;
                    }

                    hash = await this.MediaFilesService.Hasher.CalculateHashAsync(mediaStream);
                }
            }, tokenSource.Token);

            int? artistId = null;
            int? albumId = null;
            var artistAndAlbumTask = Task.Run(async () =>
            {
                using var taskUnitOfWork = this.UnitOfWorkFactory.Create();
                artistId = (await taskUnitOfWork.Artists.Get()
                                                .FirstOrDefaultAsync(
                                                    a => a.Name == this.MediaFilesService.GetArtistName(mediaFile),
                                                    tokenSource.Token))?.ID;
                if (artistId != null)
                {
                    albumId = (await taskUnitOfWork.Albums.Get().FirstOrDefaultAsync(
                        a => a.ArtistID == artistId && a.Name == this.MediaFilesService.GetAlbumName(mediaFile),
                        tokenSource.Token))?.ID;
                }
            }, tokenSource.Token);

            using var unitOfWork = this.UnitOfWorkFactory.Create();
            var categoryTask = unitOfWork.Categories.Get()
                                         .FirstOrDefaultAsync(
                                             c => c.Name == this.MediaFilesService.GetCategoryName(mediaFile),
                                             tokenSource.Token);

            await hashAndTypeTask;

            if (type == null)
            {
                tokenSource.Cancel();
                throw new ArgumentException("Invalid Media Type");
            }

            await artistAndAlbumTask;

            return new Media {
                Path = this.MediaFilesService.GetRelativePath(mediaFile),
                CategoryID = (await categoryTask)?.ID,
                ArtistID = artistId,
                AlbumID = albumId,
                Hash = hash,
                Type = type
            };
        }

        public Media CreateNewMediaOrFixMediaPath(IFileInfo mediaFile)
        {
            using (var mediaDataStream = mediaFile.OpenRead())
            {
                var hash = this.MediaFilesService.Hasher.CalculateHash(mediaDataStream, mediaFile.FullName);
                using var unitOfWork = this.UnitOfWorkFactory.Create();
                var potentialMedia = unitOfWork.Media.Get().FirstOrDefault(m => m.Hash == hash);
                if (potentialMedia != null)
                {
                    if (potentialMedia.Path == null)
                    {
                        potentialMedia.CategoryID = unitOfWork.Categories.Get()
                                                              .FirstOrDefault(
                                                                  c => c.Name ==
                                                                       this.MediaFilesService
                                                                           .GetCategoryName(mediaFile))?.ID;
                        potentialMedia.ArtistID = unitOfWork.Artists.Get()
                                                            .FirstOrDefault(a =>
                                                                a.Name == this.MediaFilesService.GetArtistName(
                                                                    mediaFile))?.ID;
                        potentialMedia.AlbumID = unitOfWork.Albums.Get()
                                                           .FirstOrDefault(a =>
                                                               a.ArtistID == potentialMedia.ArtistID &&
                                                               a.Name == this.MediaFilesService.GetAlbumName(mediaFile))
                                                           ?.ID;
                        potentialMedia.Path = this.MediaFilesService.GetRelativePath(mediaFile);
                        return potentialMedia;
                    }

                    throw new ArgumentException(
                        $"Duplicate Media: {this.MediaFilesService.GetRelativePath(mediaFile)}");
                }

                return this.CreateMedia(mediaFile);
            }
        }

        public async Task<Media> CreateNewMediaOrFixMediaPathAsync(IFileInfo mediaFile)
        {
            var tokenSource = new CancellationTokenSource();

            string hash = null;
            MediaType? type = null;

            var hashAndTypeTask = Task.Run(async () =>
            {
                await using (var mediaStream = mediaFile.OpenRead())
                {
                    type = this.MediaFilesService.TypeFinder.GetMediaFileStreamType(mediaStream);

                    if (type == null)
                    {
                        tokenSource.Cancel();
                        return;
                    }

                    hash = await this.MediaFilesService.Hasher.CalculateHashAsync(mediaStream);
                }
            }, tokenSource.Token);

            using var unitOfWork = this.UnitOfWorkFactory.Create();
            var categoryTask = unitOfWork.Categories.Get()
                                         .FirstOrDefaultAsync(
                                             c => c.Name == this.MediaFilesService.GetCategoryName(mediaFile),
                                             tokenSource.Token);

            int? artistId = null;
            int? albumId = null;
            var artistAndAlbumTask = Task.Run(async () =>
            {
                using var taskUnitOfWork = this.UnitOfWorkFactory.Create();
                artistId = (await taskUnitOfWork.Artists.Get()
                                                .FirstOrDefaultAsync(
                                                    a => a.Name == this.MediaFilesService.GetArtistName(mediaFile),
                                                    tokenSource.Token))?.ID;
                if (artistId != null)
                {
                    albumId = (await taskUnitOfWork.Albums.Get().FirstOrDefaultAsync(
                        a => a.ArtistID == artistId && a.Name == this.MediaFilesService.GetAlbumName(mediaFile),
                        tokenSource.Token))?.ID;
                }
            }, tokenSource.Token);

            await hashAndTypeTask;

            var potentialDuplicate =
                await unitOfWork.Media.Get().FirstOrDefaultAsync(m => m.Hash == hash, tokenSource.Token);

            if (potentialDuplicate != null)
            {
                if (potentialDuplicate.Path != null)
                {
                    tokenSource.Cancel();
                    throw new ArgumentException("Duplicate Media");
                }

                await artistAndAlbumTask;
                potentialDuplicate.Path = this.MediaFilesService.GetRelativePath(mediaFile);
                potentialDuplicate.CategoryID = (await categoryTask)?.ID;
                potentialDuplicate.ArtistID = artistId;
                potentialDuplicate.AlbumID = albumId;
                return potentialDuplicate;
            }

            if (type == null)
            {
                throw new ArgumentException("Invalid Media Type");
            }

            await artistAndAlbumTask;
            return new Media {
                Path = this.MediaFilesService.GetRelativePath(mediaFile),
                CategoryID = (await categoryTask)?.ID,
                ArtistID = artistId,
                AlbumID = albumId,
                Hash = hash,
                Type = type
            };
        }

        public Media WriteStreamAndCreateMedia(Stream stream)
        {
            return this.CreateMedia(this.MediaFilesService.WriteMediaFileStream(stream));
        }

        public Task<Media> WriteStreamAndCreateMediaAsync(Stream stream)
        {
            return this.CreateMediaAsync(this.MediaFilesService.WriteMediaFileStream(stream));
        }

        #endregion
    }
}