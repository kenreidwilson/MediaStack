namespace MediaStack_API.Models.Responses
{
    public class MediaSearchResponse : BaseResponse
    {
        #region Properties

        public int Offset { get; set; }

        public int Count { get; set; }

        public int Total { get; set; }

        #endregion

        #region Constructors

        public MediaSearchResponse(object data, string message = "") : base(data, message) { }

        #endregion
    }
}