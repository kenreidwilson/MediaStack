using System.IO;
using MediaStack_Library.Model;

namespace MediaStack_Library.Controllers
{
    /// <summary>
    ///     Defines functionality for a MediaFileSystemController.
    ///     Controls how Media is related to files on the filesystem.
    /// </summary>
    public interface IMediaFileSystemController
    {
        #region Properties

        public string MediaDirectory { get; }

        #endregion

        #region Methods

        /// <summary>
        ///     Determines where the Media should exist on disk
        ///     and moves the file there.
        /// </summary>
        /// <param name="media">The Media.</param>
        public void MoveMedia(Media media);

        /// <summary>
        ///     Determines where the Album should exist on disk
        ///     and moves all Media in the album to there.
        /// </summary>
        /// <param name="album">The Album.</param>
        public void MoveAlbum(Album album);

        /// <summary>
        ///     Returns the full path of the Media.
        /// </summary>
        /// <param name="media">The Media.</param>
        /// <returns>System.String.</returns>
        public string GetMediaFullPath(Media media);

        /// <summary>
        ///     Determines where a Media should be stored based
        ///     on its attributes and properties.
        /// </summary>
        /// <param name="media">The Media.</param>
        /// <returns>System.String.</returns>
        public string DetermineMediaFilePath(Media media);

        /// <summary>
        ///     Returns the hash of a file stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>System.String.</returns>
        public string CalculateHash(FileStream stream);

        /// <summary>
        ///     Determines the type of the Media,
        ///     null if can't determine.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>System.Nullable&lt;MediaType&gt;.</returns>
        public MediaType? DetermineMediaType(FileStream stream);

        /// <summary>
        ///     Returns a dynamic object where the properties of
        ///     the dynamic object correspond to the expected properties
        ///     of the Media found at the file path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>dynamic.</returns>
        public dynamic DeriveMediaReferences(string filePath);

        #endregion
    }
}