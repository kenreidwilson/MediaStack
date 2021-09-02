using System.Collections.Generic;
using System.IO;
using MediaStackCore.Models;
using MediaStackCore.Services.HasherService;

namespace MediaStackCore.Services.MediaFilesService
{
    public interface IMediaFilesService
    {
        #region Properties

        public IHasher Hasher { get; }

        #endregion

        #region Methods

        public bool DoesMediaFileExist(Media media);

        public MediaData GetMediaData(Media media);

        public void DeleteMediaData(Media media);

        public IEnumerable<MediaData> GetAllMediaData();

        public MediaData WriteMediaStream(Stream mediaDataStream, Media media = null);

        public IEnumerable<string> GetCategoryNames();

        public IEnumerable<string> GetArtistNames();

        public IDictionary<string, IEnumerable<string>> GetArtistNameAlbumNamesDictionary();

        public MediaType? GetMediaDataStreamType(Stream stream);

        public MediaData MoveMediaFileToProperLocation(Media media);

        public IDictionary<Media, MediaData> MoveAlbumToProperLocation(Album album);

        #endregion
    }
}