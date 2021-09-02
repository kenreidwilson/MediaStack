using System.Threading.Tasks;
using MediaStackCore.Models;

namespace MediaStackCore.Services.MediaScannerService
{
    public interface IMediaScanner
    {
        #region Types and Delegates

        public delegate void MissingMediaDelegate(Media media);

        public delegate void NewMediaFileDelegate(MediaData mediaData);

        #endregion

        #region Methods

        public event NewMediaFileDelegate OnNewMediaFileFound;

        public event MissingMediaDelegate OnMissingMediaFound;

        public Task FindNewMedia();

        public Task FindMissingMedia();

        public Task FindChangedMediaFiles();

        #endregion
    }
}
