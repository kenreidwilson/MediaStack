using System.Collections.Generic;
using MediaStack_API.Models.ViewModels;

namespace MediaStack_API.Models.Responses
{
    public class TagsSearchResponse : BaseResponse
    {
        #region Constructors

        public TagsSearchResponse(IEnumerable<TagViewModel> entities, int offset, int count, int total,
            string message = "")
            : base(new {tags = entities, offset, count, total}, message) { }

        #endregion
    }
}
