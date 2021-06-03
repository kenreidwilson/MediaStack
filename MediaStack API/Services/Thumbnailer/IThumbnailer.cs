using System.Threading.Tasks;
using MediaStackCore.Models;

namespace MediaStack_API.Services.Thumbnailer
{
    public interface IThumbnailer
    {
        #region Properties

        public string ThumbnailDirectory { get; }

        #endregion

        #region Methods

        public bool HasThumbnail(Media media);

        public Task<bool> CreateThumbnail(Media media);

        public string GetThumbnailFullPath(Media media);

        public string GetDefaultThumbnailFullPath();

        #endregion
    }
}