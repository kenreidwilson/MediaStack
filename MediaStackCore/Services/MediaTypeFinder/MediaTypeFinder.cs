using System;
using System.IO;
using MediaStackCore.Models;
using MimeDetective.Extensions;

namespace MediaStackCore.Services.MediaTypeFinder
{
    public class MediaTypeFinder : IMediaTypeFinder
    {
        #region Methods

        public MediaType? GetMediaFileStreamType(Stream stream)
        {
            var fileType = stream.GetFileType();

            if (fileType == null)
            {
                // webm and webp are not recognized types, this is a workaround...
                if (stream is FileStream fStream)
                {
                    var extension = fStream.Name.Substring(Math.Max(0, fStream.Name.Length - 5));
                    if (extension == ".webm")
                    {
                        return MediaType.Video;
                    }

                    if (extension == ".webp")
                    {
                        return MediaType.Image;
                    }
                }

                return null;
            }

            switch (fileType.Mime)
            {
                case "video/mp4":
                    return MediaType.Video;
                case "video/mkv":
                    return MediaType.Video;
                case "video/x-m4v":
                    return MediaType.Video;
                case "video/webm": // Not Working
                    return MediaType.Video;
                case "image/jpeg":
                    return MediaType.Image;
                case "image/png":
                    return MediaType.Image;
                case "image/webp": // Not Working
                    return MediaType.Image;
                case "image/gif":
                    return MediaType.Animated_Image;
                default:
                    return null;
            }
        }

        #endregion
    }
}