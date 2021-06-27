using System.Collections.Generic;
using MediaStack_API.Models.ViewModels;

namespace MediaStack_API.Models.Responses
{
    public class CategorySearchResponse : BaseResponse
    {
        #region Constructors

        public CategorySearchResponse(IEnumerable<CategoryViewModel> entities, int offset, int count, int total,
            string message = "")
            : base(new {categories = entities, offset, count, total}, message) { }

        #endregion
    }
}