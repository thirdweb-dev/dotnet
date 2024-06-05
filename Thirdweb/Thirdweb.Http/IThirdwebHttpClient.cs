namespace Thirdweb
{
    public interface IThirdwebHttpClient : IDisposable
    {
        void SetHeaders(Dictionary<string, string> headers);
        void ClearHeaders();
        void AddHeader(string key, string value);
        void RemoveHeader(string key);
        Task<ThirdwebHttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default);
        Task<ThirdwebHttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default);
        Task<ThirdwebHttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default);
        Task<ThirdwebHttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default);
    }
}
