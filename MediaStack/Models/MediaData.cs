using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace MediaStackCore.Models
{
    public class MediaData
    {
        #region Properties

        protected readonly IFileSystem FileSystem;

        public readonly string Path;

        public MediaData(IFileSystem fs, string path)
        {
            this.FileSystem = fs;
            this.Path = path;
        }

        public Stream GetDataStream()
        {
            throw new NotImplementedException();
        }

        public byte[] GetDataBytes()
        {
            throw new NotImplementedException();
        }

        public async Task<byte[]> GetDataBytesAsync()
        {
            throw new NotImplementedException();
        }

        public string GetCategoryName()
        {
            throw new NotImplementedException();
        }

        public string GetArtistName()
        {
            throw new NotImplementedException();
        }

        public string GetAlbumName()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
