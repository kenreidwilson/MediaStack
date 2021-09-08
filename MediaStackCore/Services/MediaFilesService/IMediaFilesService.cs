using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using MediaStackCore.Models;
using MediaStackCore.Services.HasherService;
using MediaStackCore.Services.MediaTypeFinder;

namespace MediaStackCore.Services.MediaFilesService
{
    public interface IMediaFilesService
    {
        #region Properties

        public IHasher Hasher { get; }

        public IMediaTypeFinder TypeFinder { get; }

        #endregion

        #region Methods

        public string GetRelativePath(IFileInfo mediaFile);

        public bool DoesMediaFileExist(Media media);

        public IFileInfo GetMediaFileInfo(Media media);

        public void DeleteMediaFile(Media media);

        public IEnumerable<IFileInfo> GetAllMediaFiles();

        public IEnumerable<string> GetCategoryNames();

        public IEnumerable<string> GetArtistNames();

        public IDictionary<string, IEnumerable<string>> GetArtistNameAlbumNamesDictionary();

        public string GetCategoryName(IFileInfo mediaFile);

        public string GetArtistName(IFileInfo mediaFile);

        public string GetAlbumName(IFileInfo mediaFile);

        public IFileInfo WriteMediaFileStream(Stream mediaFileStream, Media media = null);

        public IFileInfo MoveMediaFileToProperLocation(Media media);

        public IDictionary<Media, IFileInfo> MoveAlbumToProperLocation(Album album);
        
        #endregion
    }
}
