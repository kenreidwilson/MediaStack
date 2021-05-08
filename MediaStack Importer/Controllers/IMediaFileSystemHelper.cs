using System.IO;
using MediaStackCore.Controllers;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;

namespace MediaStack_Importer.Controllers
{
    public interface IMediaFileSystemHelper : IMediaFileSystemController
    {
        #region Methods

        public Media CreateMediaFromFile(string filePath, IUnitOfWork unitOfWork);

        public string GetFileHash(string filePath, FileStream stream = null);

        public string GetRelativePath(string path);

        #endregion
    }
}
