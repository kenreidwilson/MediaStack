using System.Collections.Generic;
using MediaStack_API.Models.ViewModels;

namespace MediaStack_API.Models.Responses
{
    public class MediaSearchResponse : BaseResponse
    {
        #region Constructors

        public MediaSearchResponse(IEnumerable<MediaViewModel> entities, int offset, int count, int total,
            string message = "")
            : base(new {media = entities, offset, count, total}, message) { }

        #endregion
    }
}