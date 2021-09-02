using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaStackCore.Data_Access_Layer;
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

        public Media CreateMedia(MediaData mediaData)
        {
            using (Stream mediaDataStream = mediaData.GetDataStream())
            {
                MediaType? type = this.MediaFilesService.GetMediaDataStreamType(mediaDataStream);

                if (type == null)
                {
                    throw new ArgumentException("Invalid Media Type");
                }

                using (IUnitOfWork unitOfWork = this.UnitOfWorkFactory.Create())
                {
                    return new()
                    {
                        AlbumID = unitOfWork.Albums.Get().FirstOrDefault(a => a.Name == mediaData.GetAlbumName())?.ID,
                        ArtistID = unitOfWork.Artists.Get().FirstOrDefault(a => a.Name == mediaData.GetArtistName())?.ID,
                        CategoryID = unitOfWork.Categories.Get().FirstOrDefault(c => c.Name == mediaData.GetCategoryName())?.ID,
                        Path = mediaData.RelativePath,
                        Hash = this.MediaFilesService.Hasher.CalculateHash(mediaDataStream, mediaData.FullPath),
                        Type = type
                    };
                }
            }
        }

        public async Task<Media> CreateMediaAsync(MediaData mediaData)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            string hash = null;
            MediaType? type = null;

            var hashAndTypeTask = Task.Run(async () =>
            {
                await using (Stream mediaStream = mediaData.GetDataStream())
                {
                    type = this.MediaFilesService.GetMediaDataStreamType(mediaStream);

                    if (type == null)
                    {
                        tokenSource.Cancel();
                        return;
                    }

                    hash = await this.MediaFilesService.Hasher.CalculateHashAsync(mediaStream);
                }
            }, tokenSource.Token);

            using IUnitOfWork unitOfWork = this.UnitOfWorkFactory.Create();
            var categoryTask = unitOfWork.Categories.Get().FirstOrDefaultAsync(c => c.Name == mediaData.GetCategoryName(), tokenSource.Token);
            var artistTask = unitOfWork.Artists.Get().FirstOrDefaultAsync(a => a.Name == mediaData.GetArtistName(), tokenSource.Token);
            var albumTask = unitOfWork.Albums.Get().FirstOrDefaultAsync(a => a.Name == mediaData.GetAlbumName(), tokenSource.Token);

            await hashAndTypeTask;

            if (type == null)
            {
                throw new ArgumentException("Invalid Media Type");
            }

            return new Media
            {
                Path = mediaData.RelativePath,
                CategoryID = (await categoryTask)?.ID,
                ArtistID = (await artistTask)?.ID,
                AlbumID = (await albumTask)?.ID,
                Hash = hash,
                Type = type
            };
        }

        public Media CreateNewMediaOrFixMediaPath(MediaData mediaData)
        {
            using (Stream mediaDataStream = mediaData.GetDataStream())
            {
                string hash = this.MediaFilesService.Hasher.CalculateHash(mediaDataStream, mediaData.FullPath);
                using IUnitOfWork unitOfWork = this.UnitOfWorkFactory.Create();
                Media potentialMedia = unitOfWork.Media.Get().FirstOrDefault(m => m.Hash == hash);
                if (potentialMedia != null)
                {
                    if (potentialMedia.Path == null)
                    {
                        potentialMedia.Path = mediaData.RelativePath;
                        return potentialMedia;
                    }
                    throw new ArgumentException($"Duplicate Media: {mediaData.RelativePath}");
                }

                return this.CreateMedia(mediaData);
            }
        }

        public async Task<Media> CreateNewMediaOrFixMediaPathAsync(MediaData mediaData)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            string hash = null;
            MediaType? type = null;

            var hashAndTypeTask = Task.Run(async () =>
            {
                await using (Stream mediaStream = mediaData.GetDataStream())
                {
                    type = this.MediaFilesService.GetMediaDataStreamType(mediaStream);

                    if (type == null)
                    {
                        tokenSource.Cancel();
                        return;
                    }

                    hash = await this.MediaFilesService.Hasher.CalculateHashAsync(mediaStream);
                }
            }, tokenSource.Token);

            using IUnitOfWork unitOfWork = this.UnitOfWorkFactory.Create();
            var categoryTask = unitOfWork.Categories.Get().FirstOrDefaultAsync(c => c.Name == mediaData.GetCategoryName(), tokenSource.Token);
            var artistTask = unitOfWork.Artists.Get().FirstOrDefaultAsync(a => a.Name == mediaData.GetArtistName(), tokenSource.Token);
            var albumTask = unitOfWork.Albums.Get().FirstOrDefaultAsync(a => a.Name == mediaData.GetAlbumName(), tokenSource.Token);

            await hashAndTypeTask;

            var potentialDuplicate = await unitOfWork.Media.Get().FirstOrDefaultAsync(m => m.Hash == hash, tokenSource.Token);
            if (potentialDuplicate != null)
            {
                tokenSource.Cancel();
            }

            if (potentialDuplicate != null)
            {
                if (potentialDuplicate.Path != null)
                {
                    throw new ArgumentException("Duplicate Media");
                }
                potentialDuplicate.Path = mediaData.RelativePath;
                return potentialDuplicate;
            }

            if (type == null)
            {
                throw new ArgumentException("Invalid Media Type");
            }

            return new Media
            {
                Path = mediaData.RelativePath,
                CategoryID = (await categoryTask)?.ID,
                ArtistID = (await artistTask)?.ID,
                AlbumID = (await albumTask)?.ID,
                Hash = hash,
                Type = type
            };
        }

        public Media WriteStreamAndCreateMedia(Stream stream)
        {
            return this.CreateMedia(this.MediaFilesService.WriteMediaStream(stream));
        }

        public Task<Media> WriteStreamAndCreateMediaAsync(Stream stream)
        {
            return this.CreateMediaAsync(this.MediaFilesService.WriteMediaStream(stream));
        }

        #endregion
    }
}