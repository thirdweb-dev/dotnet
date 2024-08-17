namespace Thirdweb;

/// <summary>
/// Represents an HTTP response message used in the Thirdweb SDK.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ThirdwebHttpResponseMessage"/> class.
/// </remarks>
/// <param name="statusCode">The status code of the HTTP response.</param>
/// <param name="content">The content of the HTTP response.</param>
/// <param name="isSuccessStatusCode">A value indicating whether the HTTP response is successful.</param>
public class ThirdwebHttpResponseMessage(long statusCode, ThirdwebHttpContent content, bool isSuccessStatusCode)
{
    /// <summary>
    /// Gets or sets the status code of the HTTP response.
    /// </summary>
    public long StatusCode { get; set; } = statusCode;

    /// <summary>
    /// Gets or sets the content of the HTTP response.
    /// </summary>
    public ThirdwebHttpContent Content { get; set; } = content;

    /// <summary>
    /// Gets or sets a value indicating whether the HTTP response is successful.
    /// </summary>
    public bool IsSuccessStatusCode { get; set; } = isSuccessStatusCode;

    /// <summary>
    /// Ensures that the HTTP response was successful.
    /// </summary>
    /// <returns>The <see cref="ThirdwebHttpResponseMessage"/> instance.</returns>
    /// <exception cref="Exception">Thrown if the HTTP response was not successful.</exception>
    public ThirdwebHttpResponseMessage EnsureSuccessStatusCode()
    {
        if (!this.IsSuccessStatusCode)
        {
            // TODO: Custom exception
            throw new Exception($"Request failed with status code {this.StatusCode} and content: {this.Content.ReadAsStringAsync().Result}");
        }
        return this;
    }
}
