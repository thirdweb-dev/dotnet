namespace Thirdweb
{
    public class ThirdwebHttpClient : IThirdwebHttpClient
    {
        private readonly HttpClient _httpClient;
        private Dictionary<string, string> _headers;
        private bool _disposed;

        public ThirdwebHttpClient()
        {
            _httpClient = new HttpClient();
            _headers = new Dictionary<string, string>();
        }

        public void SetHeaders(Dictionary<string, string> headers)
        {
            _headers = new Dictionary<string, string>(headers);
        }

        public void ClearHeaders()
        {
            _headers.Clear();
        }

        public void AddHeader(string key, string value)
        {
            _headers.Add(key, value);
        }

        public void RemoveHeader(string key)
        {
            _ = _headers.Remove(key);
        }

        private void AddHeaders(HttpRequestMessage request)
        {
            foreach (var header in _headers)
            {
                _ = request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        public async Task<ThirdwebHttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            AddHeaders(request);
            var result = await _httpClient.SendAsync(request, cancellationToken);
#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods
            var resultContent = new ThirdwebHttpContent(await result.Content.ReadAsByteArrayAsync());
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods
            return new ThirdwebHttpResponseMessage((long)result.StatusCode, resultContent, result.IsSuccessStatusCode);
        }

        public async Task<ThirdwebHttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = content };
            AddHeaders(request);
            var result = await _httpClient.SendAsync(request, cancellationToken);
#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods
            var resultContent = new ThirdwebHttpContent(await result.Content.ReadAsByteArrayAsync());
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods
            return new ThirdwebHttpResponseMessage((long)result.StatusCode, resultContent, result.IsSuccessStatusCode);
        }

        public async Task<ThirdwebHttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, requestUri) { Content = content };
            AddHeaders(request);
            var result = await _httpClient.SendAsync(request, cancellationToken);
#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods
            var resultContent = new ThirdwebHttpContent(await result.Content.ReadAsByteArrayAsync());
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods
            return new ThirdwebHttpResponseMessage((long)result.StatusCode, resultContent, result.IsSuccessStatusCode);
        }

        public async Task<ThirdwebHttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            AddHeaders(request);
            var result = await _httpClient.SendAsync(request, cancellationToken);
#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods
            var resultContent = new ThirdwebHttpContent(await result.Content.ReadAsByteArrayAsync());
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods
            return new ThirdwebHttpResponseMessage((long)result.StatusCode, resultContent, result.IsSuccessStatusCode);
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
