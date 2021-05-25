using System.Collections.Generic;
using MediaStack_API.Models.ViewModels;

namespace MediaStack_API.Models.Responses
{
    public class MediaSearchResponse : BaseResponse
    {
        #region Constructors

        public MediaSearchResponse(IEnumerable<MediaViewModel> media, int offset, int count, int total, string message = "") 
            : base(new {media, offset, count, total}, message) { }

        #endregion
    }
}