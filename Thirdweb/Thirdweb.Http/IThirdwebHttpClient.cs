namespace Thirdweb
{
    public interface IThirdwebHttpClient : IDisposable
    {
        void SetHeaders(Dictionary<string, string> headers);
        void ClearHeaders();
        void AddHeader(string key, string value);
        void RemoveHeader(string key);
        Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default);
        Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default);
    }
}
