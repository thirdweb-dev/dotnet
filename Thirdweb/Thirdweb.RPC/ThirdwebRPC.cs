using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Thirdweb
{
    public class ThirdwebRPC
    {
        private const int _batchSizeLimit = 100;

        private readonly Uri _rpcUrl;
        private readonly TimeSpan _rpcTimeout;
        private readonly Timer _batchSendTimer;
        private readonly TimeSpan _batchSendInterval;

        private List<RpcRequest> _pendingBatch = new List<RpcRequest>();
        private Dictionary<int, TaskCompletionSource<object>> _responseCompletionSources = new Dictionary<int, TaskCompletionSource<object>>();
        private int _requestIdCounter = 1;

        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly Dictionary<string, ThirdwebRPC> _rpcs = new Dictionary<string, ThirdwebRPC>();

        private readonly string _clientId;
        private readonly string _secretKey;
        private readonly string _bundleId;

        public static ThirdwebRPC GetRpcInstance(ThirdwebClient client, BigInteger chainId)
        {
            var key = $"{client.ClientId}_{chainId}_{client.FetchTimeoutOptions.GetTimeout(TimeoutType.Rpc)}";

            if (!_rpcs.ContainsKey(key))
            {
                lock (_rpcs)
                {
                    if (!_rpcs.ContainsKey(key))
                    {
                        _rpcs[key] = new ThirdwebRPC(client, chainId);
                    }
                }
            }

            return _rpcs[key];
        }

        public async Task<TResponse> SendRequestAsync<TResponse>(string method, params object[] parameters)
        {
            var tcs = new TaskCompletionSource<object>();
            int requestId;

            lock (_pendingBatch)
            {
                requestId = _requestIdCounter++;
                _pendingBatch.Add(
                    new RpcRequest
                    {
                        Method = method,
                        Params = parameters,
                        Id = requestId
                    }
                );
                _responseCompletionSources.Add(requestId, tcs);

                if (_pendingBatch.Count >= _batchSizeLimit)
                {
                    SendBatchNow();
                }
            }

            object result = await tcs.Task;
            return (TResponse)result;
        }

        static ThirdwebRPC()
        {
            _httpClient.DefaultRequestHeaders.Add("x-sdk-name", "Thirdweb.NET");
            _httpClient.DefaultRequestHeaders.Add("x-sdk-os", System.Runtime.InteropServices.RuntimeInformation.OSDescription);
            _httpClient.DefaultRequestHeaders.Add("x-sdk-platform", "dotnet");
            _httpClient.DefaultRequestHeaders.Add("x-sdk-version", Constants.VERSION);
        }

        private ThirdwebRPC(ThirdwebClient client, BigInteger chainId)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            if (chainId == 0)
                throw new ArgumentException("Chain ID must be provided");

            _clientId = client.ClientId;
            _secretKey = client.SecretKey;
            _bundleId = client.BundleId;
            _rpcUrl = new Uri($"https://{chainId}.rpc.thirdweb.com/");
            _rpcTimeout = TimeSpan.FromMilliseconds(client.FetchTimeoutOptions.GetTimeout(TimeoutType.Rpc));
            _batchSendInterval = TimeSpan.FromMilliseconds(100);
            _batchSendTimer = new Timer(_ => SendBatchNow(), null, _batchSendInterval, _batchSendInterval);
        }

        private void SendBatchNow()
        {
            if (_pendingBatch.Count == 0)
                return;

            List<RpcRequest> batchToSend;
            lock (_pendingBatch)
            {
                batchToSend = new List<RpcRequest>(_pendingBatch);
                _pendingBatch.Clear();
            }

            _ = SendBatchAsync(batchToSend);
        }

        private async Task SendBatchAsync(List<RpcRequest> batch)
        {
            var batchJson = JsonConvert.SerializeObject(batch);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _rpcUrl) { Content = new StringContent(batchJson, Encoding.UTF8, "application/json") };
            if (!string.IsNullOrEmpty(_clientId))
                requestMessage.Headers.Add("x-client-id", _clientId);
            if (!string.IsNullOrEmpty(_secretKey))
                requestMessage.Headers.Add("x-secret-key", _secretKey);
            if (!string.IsNullOrEmpty(_bundleId))
                requestMessage.Headers.Add("x-bundle-id", _bundleId);

            try
            {
                using var cts = new CancellationTokenSource(_rpcTimeout);
                var response = await _httpClient.SendAsync(requestMessage, cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetail = $"Batch request failed with HTTP status code: {response.StatusCode}";
                    throw new HttpRequestException(errorDetail);
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var responses = JsonConvert.DeserializeObject<List<RpcResponse<object>>>(responseJson);

                if (responses == null)
                {
                    throw new InvalidOperationException("Failed to deserialize RPC response.");
                }

                foreach (var rpcResponse in responses)
                {
                    if (_responseCompletionSources.TryGetValue(rpcResponse.Id, out var tcs))
                    {
                        if (rpcResponse.Error != null)
                        {
                            tcs.SetException(new Exception($"RPC Error for request {rpcResponse.Id}: {rpcResponse.Error.Message}"));
                        }
                        else
                        {
                            tcs.SetResult(rpcResponse.Result);
                        }
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                var timeoutErrorDetail = $"Batch request timed out. Timeout duration: {_rpcTimeout} ms.";
                var timeoutException = new TimeoutException(timeoutErrorDetail, ex);

                foreach (var requestId in batch.Select(b => b.Id))
                {
                    if (_responseCompletionSources.TryGetValue(requestId, out var tcs))
                    {
                        tcs.TrySetException(timeoutException);
                    }
                }

                throw timeoutException;
            }
            catch (Exception ex)
            {
                foreach (var kvp in _responseCompletionSources)
                {
                    kvp.Value.TrySetException(ex);
                }
            }
        }

        public void Dispose()
        {
            _batchSendTimer.Dispose();
        }
    }
}
