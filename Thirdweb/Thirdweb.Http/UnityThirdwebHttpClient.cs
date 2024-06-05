#if UNITY_5_3_OR_NEWER
using UnityEngine;
using UnityEngine.Networking;

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
                if (header.Value != null)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }
        }

        public Task<ThirdwebHttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            return SendRequestAsync(() => UnityWebRequest.Get(requestUri), cancellationToken);
        }

        public Task<ThirdwebHttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            return SendRequestAsync(
                () =>
                {
                    var webRequest = new UnityWebRequest(requestUri, UnityWebRequest.kHttpVerbPOST)
                    {
                        uploadHandler = new UploadHandlerRaw(content.ReadAsByteArrayAsync().Result) { contentType = content.Headers.ContentType.ToString() },
                        downloadHandler = new DownloadHandlerBuffer()
                    };
                    return webRequest;
                },
                cancellationToken
            );
        }

        public Task<ThirdwebHttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            return SendRequestAsync(
                () =>
                {
                    var webRequest = UnityWebRequest.Put(requestUri, content.ReadAsByteArrayAsync().Result);
                    webRequest.SetRequestHeader("Content-Type", content.Headers.ContentType.MediaType);
                    return webRequest;
                },
                cancellationToken
            );
        }

        public Task<ThirdwebHttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            return SendRequestAsync(() => UnityWebRequest.Delete(requestUri), cancellationToken);
        }

        private Task<ThirdwebHttpResponseMessage> SendRequestAsync(Func<UnityWebRequest> createRequest, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<ThirdwebHttpResponseMessage>();

            _ = MainThreadExecutor.RunOnMainThread(async () =>
            {
                using var webRequest = createRequest();
                AddHeaders(webRequest);

                var operation = webRequest.SendWebRequest();

                while (!operation.isDone)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        webRequest.Abort();
                        tcs.SetCanceled();
                        return;
                    }
                    await Task.Yield();
                }

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    tcs.SetException(new Exception(webRequest.error));
                }
                else
                {
                    tcs.SetResult(
                        new ThirdwebHttpResponseMessage(
                            statusCode: webRequest.responseCode,
                            content: new ThirdwebHttpContent(webRequest.downloadHandler.text),
                            isSuccessStatusCode: webRequest.responseCode >= 200 && webRequest.responseCode < 300
                        )
                    );
                }
            });

            return tcs.Task;
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

    public static class MainThreadExecutor
    {
        private static readonly Queue<Action> actions = new Queue<Action>();
        private static bool isInitialized;

        public static async Task RunOnMainThread(Action action)
        {
            if (!isInitialized)
            {
                Initialize();
            }

            var tcs = new TaskCompletionSource<bool>();
            lock (actions)
            {
                actions.Enqueue(() =>
                {
                    action();
                    tcs.SetResult(true);
                });
            }

            await tcs.Task;
        }

        private static void Initialize()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                var go = new GameObject("MainThreadExecutor");
                go.AddComponent<MainThreadExecutorBehaviour>();
                GameObject.DontDestroyOnLoad(go);
            }
        }

        private class MainThreadExecutorBehaviour : MonoBehaviour
        {
            void Update()
            {
                lock (actions)
                {
                    while (actions.Count > 0)
                    {
                        actions.Dequeue().Invoke();
                    }
                }
            }
        }
    }
}
#endif
