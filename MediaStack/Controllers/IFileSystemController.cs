using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using MediaStackCore.Models;
using MediaStackCore.Services.HasherService;

namespace MediaStackCore.Controllers
{
    /// <summary>
    ///     Defines functionality for a IMediaFileSystemController.
    ///     Controls how Media is related to files on the filesystem.
    /// </summary>
    public interface IFileSystemController
    {
        #region Properties

        public IHasher Hasher { get; }

        #endregion

        #region Methods

        public bool DoesMediaFileExist(Media media);

        public MediaData GetMediaData(Media media);

        public void DeleteMediaData(Media media);

        public IEnumerable<MediaData> GetAllMediaData();

        public MediaData WriteMediaStream(Media media, Stream mediaDataStream);

        public IEnumerable<string> GetCategoryNames();

        public IEnumerable<string> GetArtistNames();

        public IDictionary<string, IEnumerable<string>> GetArtistNameAlbumNamesDictionary();

        /// <summary>
        ///     Determines the type of the Media,
        ///     null if can't determine.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>System.Nullable&lt;MediaType&gt;.</returns>
        public MediaType? GetMediaDataStreamType(Stream stream);

        public MediaData MoveMediaFileToProperLocation(Media media);

        public IDictionary<Media, MediaData> MoveAlbumToProperLocation(Album album);

        #endregion
    }
}
