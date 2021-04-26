namespace MediaStack_API.Responses
{
    public class SearchResponse : ResponseWrapper
    {
        public int Offset { get; set; }

        public int Count { get; set; }

        public int Total { get; set; }

        public SearchResponse(object data, string message = "") : base(data, message) { }
    }
}
