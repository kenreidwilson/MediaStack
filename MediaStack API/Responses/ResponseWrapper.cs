namespace MediaStack_API.Responses
{
    public class ResponseWrapper
    {
        #region Properties

        public object data { get; set; }

        public string message { get; set; }

        #endregion

        #region Constructors

        public ResponseWrapper(object data, string message = "")
        {
            this.data = data;
            this.message = message;
        }

        #endregion
    }
}