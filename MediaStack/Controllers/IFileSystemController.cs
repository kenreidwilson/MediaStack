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

        public string GetMediaDataRelativePath(MediaData data);

        public bool DoesMediaFileExist(Media media);

        public MediaData GetMediaData(Media media);

        public void DeleteMediaData(Media media);

        public IQueryable<MediaData> GetAllMediaData(Expression<Func<MediaData, bool>> expression = null);

        public MediaData WriteMediaData(Stream mediaDataStream);

        public IEnumerable<string> GetCategoryNames(Expression<Func<string, bool>> expression = null);

        public IEnumerable<string> GetArtistNames(Expression<Func<string, bool>> expression = null);

        public IDictionary<string, IEnumerable<string>> GetArtistNameAlbumNamesDictionary(Expression<Func<string, bool>> expression = null);

        /// <summary>
        ///     Determines the type of the Media,
        ///     null if can't determine.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>System.Nullable&lt;MediaType&gt;.</returns>
        public MediaType? GetMediaDataStreamType(Stream stream);

        public void MoveMediaFileToProperLocation(Media media);

        public void MoveAlbumToProperLocation(Album album);

        #endregion
    }
}
