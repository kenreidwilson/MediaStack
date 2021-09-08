using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using MediaStackCore.Models;

namespace MediaStackCore.Services.MediaService
{
    public interface IMediaService
    {
        #region Methods

        public Media DisableMedia(Media media);

        public bool IsMediaDisabled(Media media);

        public Media CreateMedia(IFileInfo mediaData);

        public Task<Media> CreateMediaAsync(IFileInfo mediaFile);

        public Media CreateNewMediaOrFixMediaPath(IFileInfo mediaFile);

        public Task<Media> CreateNewMediaOrFixMediaPathAsync(IFileInfo mediaFile);

        public Media WriteStreamAndCreateMedia(Stream stream);

        public Task<Media> WriteStreamAndCreateMediaAsync(Stream stream);

        #endregion
    }
}
