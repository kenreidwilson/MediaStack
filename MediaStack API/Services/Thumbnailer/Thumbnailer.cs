using System;
using System.Drawing;
using System.IO;
using MediaStackCore.Controllers;
using MediaStackCore.Models;
using Xabe.FFmpeg;

namespace MediaStack_API.Services.Thumbnailer
{
    public class Thumbnailer : IThumbnailer
    {
        #region Properties

        public string ThumbnailDirectory { get; } = @$"{Environment.GetEnvironmentVariable("THUMBNAIL_DIRECTORY")}";

        IMediaFileSystemController FSController { get; }

        #endregion

        #region Methods

        public Thumbnailer(IMediaFileSystemController controller)
        {
            if (string.IsNullOrEmpty(this.ThumbnailDirectory))
            {
                throw new InvalidOperationException("Invalid Media Directory");
            }

            if (this.ThumbnailDirectory[0] != Path.DirectorySeparatorChar)
            {
                this.ThumbnailDirectory += Path.DirectorySeparatorChar;
            }

            this.FSController = controller;
        }

        public bool HasThumbnail(Media media) => File.Exists(this.GetThumbnailFullPath(media));

        public virtual bool CreateThumbnail(Media media)
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
                    return this.CreateThumbnailFromImage(media);
                case MediaType.Animated_Image:
                    return this.CreateThumbnailFromImage(media);
                case MediaType.Video:
                    return this.CreateThumbnailFromVideo(media);
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

        protected bool CreateThumbnailFromImage(Media media)
        {
            try
            {
                var image = Image.FromFile(this.FSController.GetMediaFullPath(media));
                var thumb = image.GetThumbnailImage(150, 125, () => false, IntPtr.Zero);
                thumb.Save(this.DetermineThumbnailLocation(media));
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
        }

        protected bool CreateThumbnailFromVideo(Media media)
        {
            FFmpeg.Conversions.FromSnippet.Snapshot(
                this.FSController.GetMediaFullPath(media),
                this.DetermineThumbnailLocation(media),
                TimeSpan.FromSeconds(0));
            return true;
        }

        protected string DetermineThumbnailLocation(Media media)
        {
            return $@"{this.ThumbnailDirectory}{media.Hash}";
        }

        #endregion
    }
}