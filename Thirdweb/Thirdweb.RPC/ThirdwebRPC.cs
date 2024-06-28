using System.Numerics;
using System.Text;
using Newtonsoft.Json;

namespace Thirdweb
{
    /// <summary>
    /// Represents the Thirdweb RPC client for sending requests and handling responses.
    /// </summary>
    public class ThirdwebRPC
    {
        private const int _batchSizeLimit = 100;
        private readonly TimeSpan _batchInterval = TimeSpan.FromMilliseconds(100);

        private readonly Uri _rpcUrl;
        private readonly TimeSpan _rpcTimeout;
        private readonly Dictionary<string, (object Response, DateTime Timestamp)> _cache = new();
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMilliseconds(250);
        private readonly List<RpcRequest> _pendingBatch = new();
        private readonly Dictionary<int, TaskCompletionSource<object>> _responseCompletionSources = new();
        private readonly object _batchLock = new();
        private readonly object _responseLock = new();
        private readonly object _cacheLock = new();
        private readonly ThirdwebRPCTimer _batchTimer;

        private int _requestIdCounter = 1;

        private static readonly Dictionary<string, ThirdwebRPC> _rpcs = new();

        private readonly IThirdwebHttpClient _httpClient;

        /// <summary>
        /// Gets an instance of the ThirdwebRPC client for the specified ThirdwebClient and chain ID.
        /// </summary>
        /// <param name="client">The Thirdweb client.</param>
        /// <param name="chainId">The chain ID.</param>
        /// <returns>An instance of the ThirdwebRPC client.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the client is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the chain ID is invalid.</exception>
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

        /// <summary>
        /// Sends an RPC request asynchronously and returns the response.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="method">The RPC method name.</param>
        /// <param name="parameters">The parameters for the RPC request.</param>
        /// <returns>The RPC response.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the response cannot be deserialized.</exception>
        public async Task<TResponse> SendRequestAsync<TResponse>(string method, params object[] parameters)
        {
            lock (_cacheLock)
            {
                var cacheKey = GetCacheKey(method, parameters);
                if (_cache.TryGetValue(cacheKey, out var cachedItem) && (DateTime.Now - cachedItem.Timestamp) < _cacheDuration)
                {
                    return (TResponse)cachedItem.Response;
                }
            }

            var tcs = new TaskCompletionSource<object>();
            int requestId;

            lock (_batchLock)
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
                lock (_responseLock)
                {
                    _responseCompletionSources.Add(requestId, tcs);
                }

                if (_pendingBatch.Count >= _batchSizeLimit)
                {
                    SendBatchNow();
                }
            }

            var result = await tcs.Task.ConfigureAwait(false);
            if (result is TResponse response)
            {
                lock (_cacheLock)
                {
                    var cacheKey = GetCacheKey(method, parameters);
                    _cache[cacheKey] = (response, DateTime.Now);
                }
                return response;
            }
            else
            {
                try
                {
                    var deserializedResponse = JsonConvert.DeserializeObject<TResponse>(JsonConvert.SerializeObject(result));
                    lock (_cacheLock)
                    {
                        var cacheKey = GetCacheKey(method, parameters);
                        _cache[cacheKey] = (deserializedResponse, DateTime.Now);
                    }
                    return deserializedResponse;
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
            _batchTimer = new ThirdwebRPCTimer(_batchInterval);
            _batchTimer.Elapsed += SendBatchNow;
            _batchTimer.Start();
        }

        private void SendBatchNow()
        {
            List<RpcRequest> batchToSend;
            lock (_batchLock)
            {
                if (_pendingBatch.Count == 0)
                {
                    return;
                }

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
                var response = await _httpClient.PostAsync(_rpcUrl.ToString(), content, cts.Token).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetail = $"Batch request failed with HTTP status code: {response.StatusCode}";
                    throw new HttpRequestException(errorDetail);
                }

                var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var responses = JsonConvert.DeserializeObject<List<RpcResponse<object>>>(responseJson);

                foreach (var rpcResponse in responses)
                {
                    lock (_responseLock)
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

                            _responseCompletionSources.Remove(rpcResponse.Id);
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
                    lock (_responseLock)
                    {
                        if (_responseCompletionSources.TryGetValue(requestId, out var tcs))
                        {
                            _ = tcs.TrySetException(timeoutException);
                            _responseCompletionSources.Remove(requestId);
                        }
                    }
                }

                throw timeoutException;
            }
            catch (Exception ex)
            {
                lock (_responseLock)
                {
                    foreach (var kvp in _responseCompletionSources)
                    {
                        _ = kvp.Value.TrySetException(ex);
                    }
                }
            }
        }

        private string GetCacheKey(string method, params object[] parameters)
        {
            var keyBuilder = new StringBuilder();

            _ = keyBuilder.Append(method);

            foreach (var param in parameters)
            {
                _ = keyBuilder.Append(param?.ToString());
            }

            return keyBuilder.ToString();
        }
    }
}
