namespace Thirdweb
{
    public class ThirdwebHttpClient : IThirdwebHttpClient
    {
        public Dictionary<string, string> Headers { get; private set; }

        private readonly HttpClient _httpClient;
        private bool _disposed;

        public ThirdwebHttpClient()
        {
            _httpClient = new HttpClient();
            Headers = new Dictionary<string, string>();
        }

        public void SetHeaders(Dictionary<string, string> headers)
        {
            Headers = new Dictionary<string, string>(headers);
        }

        public void ClearHeaders()
        {
            Headers.Clear();
        }

        public void AddHeader(string key, string value)
        {
            Headers.Add(key, value);
        }

        public void RemoveHeader(string key)
        {
            _ = Headers.Remove(key);
        }

        private void AddHeaders(HttpRequestMessage request)
        {
            foreach (var header in Headers)
            {
                _ = request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        public async Task<ThirdwebHttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            AddHeaders(request);
            var result = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods
            var resultContent = new ThirdwebHttpContent(await result.Content.ReadAsByteArrayAsync().ConfigureAwait(false));
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods
            return new ThirdwebHttpResponseMessage((long)result.StatusCode, resultContent, result.IsSuccessStatusCode);
        }

        public async Task<ThirdwebHttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = content };
            AddHeaders(request);
            var result = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods
            var resultContent = new ThirdwebHttpContent(await result.Content.ReadAsByteArrayAsync().ConfigureAwait(false));
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods
            return new ThirdwebHttpResponseMessage((long)result.StatusCode, resultContent, result.IsSuccessStatusCode);
        }

        public Task<ThirdwebHttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ThirdwebHttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
