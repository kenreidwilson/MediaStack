namespace MediaStack_API.Models.Responses
{
    public class BaseResponse
    {
        #region Properties

        public object data { get; set; }

        public string message { get; set; }

        #endregion

        #region Constructors

        public BaseResponse(object data, string message = "")
        {
            this.data = data;
            this.message = message;
        }

        #endregion
    }
}