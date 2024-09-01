using System.Numerics;
using System.Text;
using Newtonsoft.Json;

namespace Thirdweb;

/// <summary>
/// Represents the Thirdweb RPC client for sending requests and handling responses.
/// </summary>
public class ThirdwebRPC : IDisposable
{
    private readonly Uri _rpcUrl;
    private readonly TimeSpan _rpcTimeout;
    private readonly Dictionary<string, (object Response, DateTime Timestamp)> _cache = new();
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMilliseconds(100);
    private static readonly Dictionary<string, ThirdwebRPC> _rpcs = new();
    private readonly IThirdwebHttpClient _httpClient;
    private readonly object _cacheLock = new();

    private int _requestIdCounter = 1;

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
        var cacheKey = GetCacheKey(method, parameters);

        lock (this._cacheLock)
        {
            if (this._cache.TryGetValue(cacheKey, out var cachedItem) && (DateTime.Now - cachedItem.Timestamp) < this._cacheDuration)
            {
                return (TResponse)cachedItem.Response;
            }
        }

        var requestId = this._requestIdCounter++;
        var request = new RpcRequest
        {
            Method = method,
            Params = parameters,
            Id = requestId
        };

        var response = await this.SendSingleRequestAsync(request).ConfigureAwait(false);

        if (response is TResponse typedResponse)
        {
            lock (this._cacheLock)
            {
                this._cache[cacheKey] = (typedResponse, DateTime.Now);
            }
            return typedResponse;
        }

        try
        {
            var deserializedResponse = JsonConvert.DeserializeObject<TResponse>(JsonConvert.SerializeObject(response));
            lock (this._cacheLock)
            {
                this._cache[cacheKey] = (deserializedResponse, DateTime.Now);
            }
            return deserializedResponse;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to deserialize RPC response.", ex);
        }
    }

    /// <summary>
    /// Sends a batch of RPC requests asynchronously and returns the responses.
    /// </summary>
    /// <param name="requests">The list of RPC requests to be sent in a batch.</param>
    /// <returns>A list of responses corresponding to the requests.</returns>
    public async Task<List<object>> SendBatchRequestAsync(List<RpcRequest> requests)
    {
        var requestJson = JsonConvert.SerializeObject(requests);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        try
        {
            using var cts = new CancellationTokenSource(this._rpcTimeout);
            var response = await this._httpClient.PostAsync(this._rpcUrl.ToString(), content, cts.Token).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Batch request failed with HTTP status code: {response.StatusCode}");
            }

            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var rpcResponses = JsonConvert.DeserializeObject<List<RpcResponse<object>>>(responseJson);

            var results = new List<object>();
            foreach (var rpcResponse in rpcResponses)
            {
                if (rpcResponse.Error != null)
                {
                    throw new Exception($"RPC Error for request {rpcResponse.Id}: {rpcResponse.Error.Message}");
                }
                results.Add(rpcResponse.Result);
            }

            return results;
        }
        catch (TaskCanceledException ex)
        {
            throw new TimeoutException($"Batch request timed out. Timeout duration: {this._rpcTimeout.TotalMilliseconds} ms.", ex);
        }
    }

    /// <summary>
    /// Disposes the resources used by the ThirdwebRPC instance.
    /// </summary>
    public void Dispose()
    {
        // No background tasks to cancel or dispose
    }

    private ThirdwebRPC(ThirdwebClient client, BigInteger chainId)
    {
        this._httpClient = client.HttpClient;
        this._rpcUrl = new Uri($"https://{chainId}.rpc.thirdweb-dev.com/");
        this._rpcTimeout = TimeSpan.FromMilliseconds(client.FetchTimeoutOptions.GetTimeout(TimeoutType.Rpc));
    }

    private async Task<object> SendSingleRequestAsync(RpcRequest request)
    {
        var requestJson = JsonConvert.SerializeObject(request);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        try
        {
            using var cts = new CancellationTokenSource(this._rpcTimeout);
            var response = await this._httpClient.PostAsync(this._rpcUrl.ToString(), content, cts.Token).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with HTTP status code: {response.StatusCode}");
            }

            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var rpcResponse = JsonConvert.DeserializeObject<RpcResponse<object>>(responseJson);

            if (rpcResponse.Error != null)
            {
                var revertMsg = rpcResponse.Error.Data ?? rpcResponse.Error.Message;
                throw new Exception($"RPC Error: {revertMsg}");
            }

            return rpcResponse.Result;
        }
        catch (TaskCanceledException ex)
        {
            throw new TimeoutException($"Request timed out. Timeout duration: {this._rpcTimeout.TotalMilliseconds} ms.", ex);
        }
    }

    private static string GetCacheKey(string method, params object[] parameters)
    {
        var keyBuilder = new StringBuilder().Append(method);
        foreach (var param in parameters)
        {
            _ = keyBuilder.Append(param?.ToString());
        }
        return keyBuilder.ToString();
    }
}
