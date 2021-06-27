using System.Collections.Generic;
using MediaStack_API.Models.ViewModels;

namespace MediaStack_API.Models.Responses
{
    public class AlbumsSearchResponse : BaseResponse
    {
        #region Constructors

        public AlbumsSearchResponse(IEnumerable<AlbumViewModel> entities, int offset, int count, int total,
            string message = "")
            : base(new {albums = entities, offset, count, total}, message) { }

        #endregion
    }
}