using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using ImageMagick;
using MediaStackCore.Controllers;
using MediaStackCore.Models;
using Xabe.FFmpeg;

namespace MediaStack_API.Services.Thumbnailer
{
    public class Thumbnailer : IThumbnailer
    {
        #region Properties

        public string ThumbnailDirectory { get; } = @$"{Environment.GetEnvironmentVariable("THUMBNAIL_DIRECTORY")}";

        private readonly int thumbnailHeight = 150;

        private readonly int thumbnailWidth = 125;

        protected IFileSystem FileSystem { get; }

        protected IFileSystemController FSController { get; }

        #endregion

        #region Methods

        public Thumbnailer(IFileSystem fileSystem, IFileSystemController controller)
        {
            if (string.IsNullOrEmpty(this.ThumbnailDirectory))
            {
                throw new InvalidOperationException("Invalid Thumbnail Directory");
            }

            if (Environment.GetEnvironmentVariable("FFMPEG") != null)
            {
                FFmpeg.SetExecutablesPath(Environment.GetEnvironmentVariable("FFMPEG"));
            }

            if (this.ThumbnailDirectory[0] != Path.DirectorySeparatorChar)
            {
                this.ThumbnailDirectory += Path.DirectorySeparatorChar;
            }

            this.FileSystem = fileSystem;
            this.FSController = controller;
        }

        public bool HasThumbnail(Media media) => this.FileSystem.File.Exists(this.GetThumbnailFullPath(media));

        public virtual async Task<bool> CreateThumbnail(Media media)
        {
            if (media.Path == null)
            {
                return false;
            }

            if (this.HasThumbnail(media))
            {
                return true;
            }

            switch (media.Type)
            {
                case MediaType.Image:
                    return await this.CreateThumbnailFromImage(media);
                case MediaType.Animated_Image:
                    return await this.CreateThumbnailFromImage(media);
                case MediaType.Video:
                    return await this.CreateThumbnailFromVideo(media);
                default:
                    return false;
            }
        }

        public byte[] GetMediaThumbnailBytes(Media media)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GetMediaThumbnailBytesAsync(Media media)
        {
            throw new NotImplementedException();
        }

        public byte[] GetDefaultThumbnailBytes()
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GetDefaultThumbnailBytesAsync()
        {
            throw new NotImplementedException();
        }

        protected async Task<bool> CreateThumbnailFromImage(Media media)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (MagickImage image = new MagickImage(this.FSController.GetMediaData(media).GetDataStream()))
                    {
                        image.Thumbnail(new MagickGeometry(this.thumbnailWidth, this.thumbnailHeight));
                        image.Write(this.DetermineThumbnailLocation(media));
                    }

                    return true;
                }
                catch (OutOfMemoryException)
                {
                    return false;
                }
                catch (IOException)
                {
                    return false;
                }
                catch (MagickBlobErrorException)
                {
                    return false;
                }
            });
        }

        protected async Task<bool> CreateThumbnailFromVideo(Media media)
        {
            var result = FFmpeg.Conversions.New()
                               .AddParameter($"-i \"{this.FSController.GetMediaData(media).Path}\"")
                               .AddParameter("-ss 00:00:01.000")
                               .AddParameter("-vframes 1")
                               .AddParameter("-f image2")
                               .SetOutput(this.DetermineThumbnailLocation(media))
                               .Start();

            await result;
            return result.IsCompletedSuccessfully;
        }

        private string DetermineThumbnailLocation(Media media)
        {
            return $@"{this.ThumbnailDirectory}{media.Hash}";
        }

        private string GetThumbnailFullPath(Media media)
        {
            return this.DetermineThumbnailLocation(media);
        }

        private string GetDefaultThumbnailFullPath()
        {
            return $@"{this.ThumbnailDirectory}default";
        }

        #endregion
    }
}
