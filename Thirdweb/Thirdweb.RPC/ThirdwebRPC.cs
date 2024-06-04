using System.Numerics;
using System.Text;
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

        private static readonly Dictionary<string, ThirdwebRPC> _rpcs = new Dictionary<string, ThirdwebRPC>();

        private readonly IThirdwebHttpClient _httpClient;

        public static ThirdwebRPC GetRpcInstance(ThirdwebClient client, BigInteger chainId)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (chainId == 0)
            {
                throw new ArgumentException("Invalid Chain ID");
            }

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

            var result = await tcs.Task;
            if (result is TResponse response)
            {
                return response;
            }
            else
            {
                try
                {
                    return JsonConvert.DeserializeObject<TResponse>(JsonConvert.SerializeObject(result));
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to deserialize RPC response.", ex);
                }
            }
        }

        private ThirdwebRPC(ThirdwebClient client, BigInteger chainId)
        {
            _httpClient = client.HttpClient;
            _rpcUrl = new Uri($"https://{chainId}.rpc.thirdweb.com/");
            _rpcTimeout = TimeSpan.FromMilliseconds(client.FetchTimeoutOptions.GetTimeout(TimeoutType.Rpc));
            _batchSendInterval = TimeSpan.FromMilliseconds(100);
            _batchSendTimer = new Timer(_ => SendBatchNow(), null, _batchSendInterval, _batchSendInterval);
        }

        private void SendBatchNow()
        {
            if (_pendingBatch.Count == 0)
            {
                return;
            }

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
            var content = new StringContent(batchJson, Encoding.UTF8, "application/json");

            try
            {
                using var cts = new CancellationTokenSource(_rpcTimeout);
                var response = await _httpClient.PostAsync(_rpcUrl.ToString(), content, cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetail = $"Batch request failed with HTTP status code: {response.StatusCode}";
                    throw new HttpRequestException(errorDetail);
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var responses = JsonConvert.DeserializeObject<List<RpcResponse<object>>>(responseJson);

                foreach (var rpcResponse in responses)
                {
                    if (_responseCompletionSources.TryGetValue(rpcResponse.Id, out var tcs))
                    {
                        if (rpcResponse.Error != null)
                        {
                            var revertMsg = "";
                            if (rpcResponse.Error.Data != null)
                            {
                                try
                                {
                                    revertMsg = new Nethereum.ABI.FunctionEncoding.FunctionCallDecoder().DecodeFunctionErrorMessage(rpcResponse.Error.Data);
                                    revertMsg = string.IsNullOrWhiteSpace(revertMsg) ? rpcResponse.Error.Data : revertMsg;
                                }
                                catch
                                {
                                    revertMsg = rpcResponse.Error.Data;
                                }
                            }
                            tcs.SetException(new Exception($"RPC Error for request {rpcResponse.Id}: {rpcResponse.Error.Message} {revertMsg}"));
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
    }
}
