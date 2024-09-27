namespace Thirdweb;

/// <summary>
/// Represents a HTTP client for the Thirdweb SDK.
/// </summary>
public class ThirdwebHttpClient : IThirdwebHttpClient
{
    /// <summary>
    /// Gets the headers for the HTTP client.
    /// </summary>
    public Dictionary<string, string> Headers { get; private set; }

    private readonly HttpClient _httpClient;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThirdwebHttpClient"/> class.
    /// </summary>
    public ThirdwebHttpClient()
    {
        this._httpClient = new HttpClient();
        this.Headers = new Dictionary<string, string>();
    }

    /// <summary>
    /// Sets the headers for the HTTP client.
    /// </summary>
    /// <param name="headers">The headers to set.</param>
    public void SetHeaders(Dictionary<string, string> headers)
    {
        this.Headers = new Dictionary<string, string>(headers);
    }

    /// <summary>
    /// Clears all headers from the HTTP client.
    /// </summary>
    public void ClearHeaders()
    {
        this.Headers.Clear();
    }

    /// <summary>
    /// Adds a header to the HTTP client.
    /// </summary>
    /// <param name="key">The header key.</param>
    /// <param name="value">The header value.</param>
    public void AddHeader(string key, string value)
    {
        this.Headers.Add(key, value);
    }

    /// <summary>
    /// Removes a header from the HTTP client.
    /// </summary>
    /// <param name="key">The header key.</param>
    public void RemoveHeader(string key)
    {
        _ = this.Headers.Remove(key);
    }

    private void AddHeaders(HttpRequestMessage request)
    {
        foreach (var header in this.Headers)
        {
            _ = request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    /// <summary>
    /// Sends a GET request to the specified URI.
    /// </summary>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
    public async Task<ThirdwebHttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        this.AddHeaders(request);
        var result = await this._httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods
        var resultContent = new ThirdwebHttpContent(await result.Content.ReadAsByteArrayAsync().ConfigureAwait(false));
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods
        return new ThirdwebHttpResponseMessage((long)result.StatusCode, resultContent, result.IsSuccessStatusCode);
    }

    /// <summary>
    /// Sends a POST request to the specified URI.
    /// </summary>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="content">The HTTP content to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
    public async Task<ThirdwebHttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = content };
        this.AddHeaders(request);
        var result = await this._httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods
        var resultContent = new ThirdwebHttpContent(await result.Content.ReadAsByteArrayAsync().ConfigureAwait(false));
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods
        return new ThirdwebHttpResponseMessage((long)result.StatusCode, resultContent, result.IsSuccessStatusCode);
    }

    /// <summary>
    /// Sends a PUT request to the specified URI.
    /// </summary>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="content">The HTTP content to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
    public Task<ThirdwebHttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Sends a DELETE request to the specified URI.
    /// </summary>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
    public Task<ThirdwebHttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Disposes the HTTP client.
    /// </summary>
    /// <param name="disposing">Whether the client is being disposed.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this._disposed)
        {
            if (disposing)
            {
                this._httpClient.Dispose();
            }
            this._disposed = true;
        }
    }

    /// <summary>
    /// Disposes the HTTP client.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }
}
