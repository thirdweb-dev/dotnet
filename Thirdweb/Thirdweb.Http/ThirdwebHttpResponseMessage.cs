namespace Thirdweb
{
    /// <summary>
    /// Represents an HTTP response message used in the Thirdweb SDK.
    /// </summary>
    public class ThirdwebHttpResponseMessage
    {
        /// <summary>
        /// Gets or sets the status code of the HTTP response.
        /// </summary>
        public long StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the content of the HTTP response.
        /// </summary>
        public ThirdwebHttpContent Content { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the HTTP response is successful.
        /// </summary>
        public bool IsSuccessStatusCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThirdwebHttpResponseMessage"/> class.
        /// </summary>
        /// <param name="statusCode">The status code of the HTTP response.</param>
        /// <param name="content">The content of the HTTP response.</param>
        /// <param name="isSuccessStatusCode">A value indicating whether the HTTP response is successful.</param>
        public ThirdwebHttpResponseMessage(long statusCode, ThirdwebHttpContent content, bool isSuccessStatusCode)
        {
            StatusCode = statusCode;
            Content = content;
            IsSuccessStatusCode = isSuccessStatusCode;
        }

        /// <summary>
        /// Ensures that the HTTP response was successful.
        /// </summary>
        /// <returns>The <see cref="ThirdwebHttpResponseMessage"/> instance.</returns>
        /// <exception cref="Exception">Thrown if the HTTP response was not successful.</exception>
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
