#if UNITY_5_3_OR_NEWER
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading;

namespace Thirdweb
{
    public class UnityThirdwebHttpClient : IThirdwebHttpClient
    {
        private Dictionary<string, string> _headers;
        private bool _disposed;

        public UnityThirdwebHttpClient()
        {
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

        private void AddHeaders(UnityWebRequest request)
        {
            foreach (var header in _headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(requestUri))
            {
                AddHeaders(webRequest);
                await webRequest.SendWebRequest().WithCancellation(cancellationToken);
                return ConvertToHttpResponseMessage(webRequest);
            }
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Post(requestUri, content.ToString()))
            {
                AddHeaders(webRequest);
                await webRequest.SendWebRequest().WithCancellation(cancellationToken);
                return ConvertToHttpResponseMessage(webRequest);
            }
        }

        public async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Put(requestUri, content.ToString()))
            {
                AddHeaders(webRequest);
                await webRequest.SendWebRequest().WithCancellation(cancellationToken);
                return ConvertToHttpResponseMessage(webRequest);
            }
        }

        public async Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Delete(requestUri))
            {
                AddHeaders(webRequest);
                await webRequest.SendWebRequest().WithCancellation(cancellationToken);
                return ConvertToHttpResponseMessage(webRequest);
            }
        }

        private HttpResponseMessage ConvertToHttpResponseMessage(UnityWebRequest webRequest)
        {
            var response = new HttpResponseMessage((HttpStatusCode)webRequest.responseCode)
            {
                Content = new StringContent(webRequest.downloadHandler.text)
            };
            return response;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // No need to dispose UnityWebRequest
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

    public static class UnityWebRequestExtensions
    {
        public static async Task WithCancellation(this UnityWebRequestAsyncOperation asyncOperation, CancellationToken cancellationToken)
        {
            while (!asyncOperation.isDone)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    asyncOperation.webRequest.Abort();
                    throw new OperationCanceledException(cancellationToken);
                }

                await Task.Yield();
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}

#endif
