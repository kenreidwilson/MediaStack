using System.Collections.Generic;

namespace MediaStack_API.Models.Responses
{
    public class SearchResponse<T> : BaseResponse
    {
        #region Constructors

        public SearchResponse(IEnumerable<T> data, int offset, int count, int total,
            string message = "") : base(new {data, offset, count, total}, message) { }

        #endregion
    }
}