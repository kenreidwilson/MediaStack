using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaStackCore.Controllers;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaStackCore.Extensions
{
    public static class UnitOfWorkExtensions
    {
        #region Methods

        public static Media DisableMedia(this IUnitOfWork unitOfWork, Media media)
        {
            media.Path = null;
            return media;
        }

        public static bool IsMediaDisabled(this IUnitOfWork unitOfWor, Media media)
        {
            return media.Path == null;
        }

        public static Media FindMediaFromMediaData(this IUnitOfWork unitOfWork, MediaData mediaData)
        {
            return unitOfWork.Media.Get().FirstOrDefault(m => m.Path == mediaData.RelativePath);
        }

        public static Media CreateMedia(this IUnitOfWork unitOfWork, IFileSystemController fsController,
            MediaData mediaData)
        {
            using (Stream mediaDataStream = mediaData.GetDataStream())
            {
                MediaType? type = fsController.GetMediaDataStreamType(mediaDataStream);

                if (type == null)
                {
                    throw new ArgumentException("Invalid Media Type");
                }

                return new()
                {
                    AlbumID = unitOfWork.Albums.Get().FirstOrDefault(a => a.Name == mediaData.GetAlbumName())?.ID,
                    ArtistID = unitOfWork.Artists.Get().FirstOrDefault(a => a.Name == mediaData.GetArtistName())?.ID,
                    CategoryID = unitOfWork.Categories.Get().FirstOrDefault(c => c.Name == mediaData.GetCategoryName())?.ID,
                    Path = mediaData.RelativePath,
                    Hash = fsController.Hasher.CalculateHash(mediaDataStream, mediaData.FullPath),
                    Type = type
                };
            }
        }

        public static async Task<Media> CreateMediaAsync(this IUnitOfWork unitOfWork,
            IFileSystemController fsController, MediaData mediaData)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            string hash = null;
            MediaType? type = null;

            var hashAndTypeTask = Task.Run(async () =>
            {
                await using (Stream mediaStream = mediaData.GetDataStream())
                {
                    type = fsController.GetMediaDataStreamType(mediaStream);

                    if (type == null)
                    {
                        tokenSource.Cancel();
                        return;
                    }

                    hash = await fsController.Hasher.CalculateHashAsync(mediaStream);
                }
            }, tokenSource.Token);

            var categoryTask = unitOfWork.Categories.Get().FirstOrDefaultAsync(c => c.Name == mediaData.GetCategoryName(), tokenSource.Token);
            var artistTask = unitOfWork.Artists.Get().FirstOrDefaultAsync(a => a.Name == mediaData.GetArtistName(), tokenSource.Token);
            var albumTask = unitOfWork.Albums.Get().FirstOrDefaultAsync(a => a.Name == mediaData.GetAlbumName(), tokenSource.Token);

            await hashAndTypeTask;

            if (type == null)
            {
                throw new ArgumentException("Invalid Media Type");
            }

            return new Media {
                Path = mediaData.RelativePath,
                CategoryID = (await categoryTask)?.ID,
                ArtistID = (await artistTask)?.ID,
                AlbumID = (await albumTask)?.ID,
                Hash = hash,
                Type = type
            };
        }

        public static Media CreateNewMediaOrFixMediaPath(this IUnitOfWork unitOfWork,
            IFileSystemController fsController, MediaData mediaData)
        {
            using (Stream mediaDataStream = mediaData.GetDataStream())
            {
                string hash = fsController.Hasher.CalculateHash(mediaDataStream, mediaData.FullPath);
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

                return unitOfWork.CreateMedia(fsController, mediaData);
            }
        }

        public static async Task<Media> CreateNewMediaOrFixMediaPathAsync(this IUnitOfWork unitOfWork,
            IFileSystemController fsController, MediaData mediaData)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            string hash = null;
            MediaType? type = null;

            var hashAndTypeTask = Task.Run(async () =>
            {
                await using (Stream mediaStream = mediaData.GetDataStream())
                {
                    type = fsController.GetMediaDataStreamType(mediaStream);

                    if (type == null)
                    {
                        tokenSource.Cancel();
                        return;
                    }

                    hash = await fsController.Hasher.CalculateHashAsync(mediaStream);
                }
            }, tokenSource.Token);

            var categoryTask = unitOfWork.Categories.Get().FirstOrDefaultAsync(c => c.Name == mediaData.GetCategoryName(), tokenSource.Token);
            var artistTask = unitOfWork.Artists.Get().FirstOrDefaultAsync(a => a.Name == mediaData.GetArtistName(), tokenSource.Token);
            var albumTask = unitOfWork.Albums.Get().FirstOrDefaultAsync(a => a.Name == mediaData.GetAlbumName(), tokenSource.Token);

            await hashAndTypeTask;

            var potentialDuplicate = await unitOfWork.Media.Get().FirstOrDefaultAsync(m => m.Hash == hash, tokenSource.Token);
            if (potentialDuplicate != null)
            {
                tokenSource.Cancel();
            }

            if (type == null)
            {
                throw new ArgumentException("Invalid Media Type");
            }

            if (potentialDuplicate != null)
            {
                if (potentialDuplicate.Path != null)
                {
                    throw new ArgumentException("Duplicate Media");
                }
                else
                {
                    potentialDuplicate.Path = mediaData.RelativePath;
                    return potentialDuplicate;
                }
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

        public static Media WriteStreamAndCreateMedia(this IUnitOfWork unitOfWork, IFileSystemController fsController, Stream stream)
        {
            return unitOfWork.CreateMedia(fsController, fsController.WriteMediaStream(stream));
        }

        public static Task<Media> WriteStreamAndCreateMediaAsync(this IUnitOfWork unitOfWork, IFileSystemController fsController,
            Stream stream)
        {
            return unitOfWork.CreateMediaAsync(fsController, fsController.WriteMediaStream(stream));
        }

        #endregion
    }
}