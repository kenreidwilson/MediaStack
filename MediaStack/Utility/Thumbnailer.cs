using MediaStack_Library.Model;
using System;
using System.Drawing;
using Xabe.FFmpeg;

namespace MediaStack_Library.Utility
{
    public class Thumbnailer
    {
        public bool CreateThumbnail(Media media)
        {
            switch(media.Type)
            {
                case (MediaType.Image):
                    return createThumbnailFromImage(media);
                case (MediaType.Animated_Image):
                    return createThumbnailFromImage(media);
                case (MediaType.Video):
                    return createThumbnailFromVideo(media);
                default:
                    return false;
            }
        }

        private bool createThumbnailFromImage(Media media)
        {
            Image image = Image.FromFile(media.Path);
            Image thumb = image.GetThumbnailImage(150, 125, ()=>false, IntPtr.Zero);
            thumb.Save(MediaFSController.THUMBNAIL_DIRECTORY + media.Hash);
            return true;
        }

        private bool createThumbnailFromVideo(Media media)
        {
            FFmpeg.Conversions.FromSnippet.Snapshot(
                media.Path, 
                MediaFSController.THUMBNAIL_DIRECTORY + media.Hash, 
                TimeSpan.FromSeconds(0)).Start();
            return true;
        }
    }
}
