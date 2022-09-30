using System.IO;
using MediaStackCore.Models;

namespace MediaStackCore.Services.MediaTypeFinder
{
    public interface IMediaTypeFinder
    {
        #region Methods

        public MediaType? GetMediaFileStreamType(Stream stream);

        public string GetStreamFileExtension(Stream stream);

        #endregion
    }
}
