using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace MediaStackCore.Models
{
    public class MediaData
    {
        #region Properties

        protected readonly IFileSystem FileSystem;

        protected readonly string MediaDirectory;

        protected readonly string Path;

        public string RelativePath => this.getRelativePath();

        public string FullPath => this.getFullPath();

        public MediaData(IFileSystem fileSystem, string mediaDirectory, string path)
        {
            if (fileSystem == null || mediaDirectory == null || path == null)
            {
                throw new ArgumentException();
            }

            this.FileSystem = fileSystem;
            this.MediaDirectory = mediaDirectory;
            this.Path = path;
        }

        public Stream GetDataStream()
        {
            return this.FileSystem.File.Open(this.FullPath, FileMode.Open);
        }

        public byte[] GetDataBytes()
        {
            return this.FileSystem.File.ReadAllBytes(this.FullPath);
        }

        public async Task<byte[]> GetDataBytesAsync()
        {
            return await this.FileSystem.File.ReadAllBytesAsync(this.FullPath);
        }

        public string GetCategoryName()
        {
            return this.RelativePath.Split($"{this.FileSystem.Path.DirectorySeparatorChar}")
                       .FirstOrDefault();
        }

        public string GetArtistName()
        {
            return this.RelativePath.Split($"{this.FileSystem.Path.DirectorySeparatorChar}")
                       .Skip(1)
                       .FirstOrDefault();
        }

        public string GetAlbumName()
        {
            return this.RelativePath.Split($"{this.FileSystem.Path.DirectorySeparatorChar}")
                       .Skip(2)
                       .FirstOrDefault();
        }

        private string getRelativePath()
        {
            return !this.FileSystem.Path.IsPathFullyQualified(this.Path) ? 
                this.Path : 
                this.Path.Replace(this.MediaDirectory, "");
        }

        private string getFullPath()
        {
            return this.FileSystem.Path.IsPathFullyQualified(this.Path) ? 
                this.Path : 
                $"{this.MediaDirectory}{this.Path}";
        }

        #endregion
    }
}
