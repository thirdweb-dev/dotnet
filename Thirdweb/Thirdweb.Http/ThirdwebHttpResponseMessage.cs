namespace Thirdweb
{
    public class ThirdwebHttpResponseMessage
    {
        public long StatusCode { get; set; }
        public ThirdwebHttpContent Content { get; set; }
        public bool IsSuccessStatusCode { get; set; }

        public ThirdwebHttpResponseMessage(long statusCode, ThirdwebHttpContent content, bool isSuccessStatusCode)
        {
            StatusCode = statusCode;
            Content = content;
            IsSuccessStatusCode = isSuccessStatusCode;
        }

        public ThirdwebHttpResponseMessage EnsureSuccessStatusCode()
        {
            if (!IsSuccessStatusCode)
            {
                // TODO: Custom exception
                throw new Exception($"Request failed with status code {StatusCode} and content: {Content.ReadAsStringAsync().Result}");
            }
            return this;
        }
    }
}
