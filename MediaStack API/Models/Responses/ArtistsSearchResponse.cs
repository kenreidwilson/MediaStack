using System.Collections.Generic;
using MediaStack_API.Models.ViewModels;

namespace MediaStack_API.Models.Responses
{
    public class ArtistsSearchResponse : BaseResponse
    {
        #region Constructors

        public ArtistsSearchResponse(IEnumerable<ArtistViewModel> entities, int offset, int count, int total,
            string message = "")
            : base(new {artists = entities, offset, count, total}, message) { }

        #endregion
    }
}