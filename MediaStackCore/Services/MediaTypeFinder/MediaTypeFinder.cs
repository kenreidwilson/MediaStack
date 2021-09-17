using System.Collections.Generic;
using System.IO;
using MediaStackCore.Models;
using MimeDetective.Extensions;

namespace MediaStackCore.Services.MediaTypeFinder
{
    public class MediaTypeFinder : IMediaTypeFinder
    {
        protected IDictionary<string, MediaType> MimeMediaTypeDictionary = new Dictionary<string, MediaType> {
            {"video/mp4", MediaType.Video },
            {"video/mkv", MediaType.Video },
            {"video/x-m4v", MediaType.Video },
            {"image/jpeg", MediaType.Image },
            {"image/png", MediaType.Image },
            {"image/gif", MediaType.Animated_Image },
        };

        #region Methods

        public MediaType? GetMediaFileStreamType(Stream stream)
        {
            var fileType = stream.GetFileType();

            if (fileType == null)
            {

                if (this.IsWebM(stream))
                {
                    return MediaType.Video;
                }

                if (this.IsWebP(stream))
                {
                    return MediaType.Image;
                }

                return null;
            }

            if (this.MimeMediaTypeDictionary.ContainsKey(fileType.Mime))
            {
                return this.MimeMediaTypeDictionary[fileType.Mime];
            }

            return null;
        }

        protected bool IsWebM(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            int[] magicBytes = new int[4];

            for (int i = 0; i < 4; i++)
            {
                magicBytes[i] = stream.ReadByte();
            }

            return magicBytes[0] == 26 && magicBytes[1] == 69 && magicBytes[2] == 223 && magicBytes[3] == 163;
        }

        protected bool IsWebP(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            int[] magicBytes = new int[4];

            for (int i = 0; i < 4; i++)
            {
                magicBytes[i] = stream.ReadByte();
            }

            return magicBytes[0] == 87 && magicBytes[1] == 69 && magicBytes[2] == 66 && magicBytes[3] == 80;
        }

        #endregion
    }
}