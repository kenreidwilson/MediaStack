using System;
using System.IO;
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

        IMediaFileSystemController FSController { get; }

        #endregion

        #region Methods

        public Thumbnailer(IMediaFileSystemController controller)
        {
            if (string.IsNullOrEmpty(this.ThumbnailDirectory))
            {
                throw new InvalidOperationException("Invalid Media Directory");
            }

            if (Environment.GetEnvironmentVariable("FFMPEG") != null)
            {
                FFmpeg.SetExecutablesPath(Environment.GetEnvironmentVariable("FFMPEG"));
            }

            if (this.ThumbnailDirectory[0] != Path.DirectorySeparatorChar)
            {
                this.ThumbnailDirectory += Path.DirectorySeparatorChar;
            }

            this.FSController = controller;
        }

        public bool HasThumbnail(Media media) => File.Exists(this.GetThumbnailFullPath(media));

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

        public string GetThumbnailFullPath(Media media)
        {
            return this.DetermineThumbnailLocation(media);
        }

        public string GetDefaultThumbnailFullPath()
        {
            return $@"{this.ThumbnailDirectory}default";
        }

        protected async Task<bool> CreateThumbnailFromImage(Media media)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var file = new FileInfo(this.FSController.GetMediaFullPath(media));
                    using (MagickImage image = new MagickImage(file))
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
                               .AddParameter($"-i \"{this.FSController.GetMediaFullPath(media)}\"")
                               .AddParameter("-ss 00:00:01.000")
                               .AddParameter("-vframes 1")
                               .AddParameter("-f image2")
                               .SetOutput(this.DetermineThumbnailLocation(media))
                               .Start();

            await result;
            return result.IsCompletedSuccessfully;
        }

        protected string DetermineThumbnailLocation(Media media)
        {
            return $@"{this.ThumbnailDirectory}{media.Hash}";
        }

        #endregion
    }
}