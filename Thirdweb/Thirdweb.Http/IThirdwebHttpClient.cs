namespace Thirdweb;

/// <summary>
/// Interface for a HTTP client used in the Thirdweb SDK.
/// </summary>
public interface IThirdwebHttpClient : IDisposable
{
    /// <summary>
    /// Gets the headers for the HTTP client.
    /// </summary>
    Dictionary<string, string> Headers { get; }

    /// <summary>
    /// Sets the headers for the HTTP client.
    /// </summary>
    /// <param name="headers">The headers to set.</param>
    void SetHeaders(Dictionary<string, string> headers);

    /// <summary>
    /// Clears all headers from the HTTP client.
    /// </summary>
    void ClearHeaders();

    /// <summary>
    /// Adds a header to the HTTP client.
    /// </summary>
    /// <param name="key">The header key.</param>
    /// <param name="value">The header value.</param>
    void AddHeader(string key, string value);

    /// <summary>
    /// Removes a header from the HTTP client.
    /// </summary>
    /// <param name="key">The header key.</param>
    void RemoveHeader(string key);

    /// <summary>
    /// Sends a GET request to the specified URI.
    /// </summary>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
    Task<ThirdwebHttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a POST request to the specified URI.
    /// </summary>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="content">The HTTP content to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
    Task<ThirdwebHttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a PUT request to the specified URI.
    /// </summary>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="content">The HTTP content to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
    Task<ThirdwebHttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a DELETE request to the specified URI.
    /// </summary>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
    Task<ThirdwebHttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default);
}
