using System.IO;
using System.Threading.Tasks;
using MediaStackCore.Models;

namespace MediaStackCore.Services.MediaService
{
    public interface IMediaService
    {
        #region Methods

        public Media DisableMedia(Media media);

        public bool IsMediaDisabled(Media media);

        public Media CreateMedia(MediaData mediaData);

        public Task<Media> CreateMediaAsync(MediaData mediaData);

        public Media CreateNewMediaOrFixMediaPath(MediaData mediaData);

        public Task<Media> CreateNewMediaOrFixMediaPathAsync(MediaData mediaData);

        public Media WriteStreamAndCreateMedia(Stream stream);

        public Task<Media> WriteStreamAndCreateMediaAsync(Stream stream);

        #endregion
    }
}
