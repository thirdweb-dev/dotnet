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

        public async Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            AddHeaders(request);
            return await _httpClient.SendAsync(request, cancellationToken);
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = content };
            AddHeaders(request);
            return await _httpClient.SendAsync(request, cancellationToken);
        }

        public async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, requestUri) { Content = content };
            AddHeaders(request);
            return await _httpClient.SendAsync(request, cancellationToken);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            AddHeaders(request);
            return await _httpClient.SendAsync(request, cancellationToken);
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
