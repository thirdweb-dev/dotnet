using System.Numerics;
using System.Text;
using Nethereum.ABI.EIP712;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Thirdweb;

/// <summary>
/// Enclave based secure cross ecosystem wallet.
/// </summary>
public partial class EngineWallet : IThirdwebWallet
{
    public ThirdwebClient Client { get; }
    public ThirdwebAccountType AccountType => ThirdwebAccountType.ExternalAccount;
    public string WalletId => "engine";

    private readonly string _engineUrl;
    private readonly string _walletAddress;
    private readonly IThirdwebHttpClient _engineClient;
    private readonly int? _timeoutSeconds;

    internal EngineWallet(ThirdwebClient client, IThirdwebHttpClient engineClient, string engineUrl, string walletAddress, int? timeoutSeconds)
    {
        this.Client = client;
        this._engineUrl = engineUrl;
        this._walletAddress = walletAddress;
        this._engineClient = engineClient;
        this._timeoutSeconds = timeoutSeconds;
    }

    #region Creation

    /// <summary>
    /// Creates an instance of the EngineWallet.
    /// </summary>
    /// <param name="client">The Thirdweb client.</param>
    /// <param name="engineUrl">The URL of the engine.</param>
    /// <param name="authToken">The access token to use for the engine.</param>
    /// <param name="walletAddress">The backend wallet address to use.</param>
    /// <param name="timeoutSeconds">The timeout in seconds for the transaction. Defaults to no timeout.</param>
    /// <param name="additionalHeaders">Additional headers to include in requests. Authorization and X-Backend-Wallet-Address automatically included.</param>
    public static EngineWallet Create(ThirdwebClient client, string engineUrl, string authToken, string walletAddress, int? timeoutSeconds = null, Dictionary<string, string> additionalHeaders = null)
    {
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client), "Client cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(engineUrl))
        {
            throw new ArgumentNullException(nameof(engineUrl), "Engine URL cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(authToken))
        {
            throw new ArgumentNullException(nameof(authToken), "Auth token cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(walletAddress))
        {
            throw new ArgumentNullException(nameof(walletAddress), "Wallet address cannot be null or empty.");
        }

        if (engineUrl.EndsWith('/'))
        {
            engineUrl = engineUrl[..^1];
        }

        walletAddress = walletAddress.ToChecksumAddress();

        var engineClient = Utils.ReconstructHttpClient(client.HttpClient, new Dictionary<string, string> { { "Authorization", $"Bearer {authToken}" }, });
        engineClient.AddHeader("X-Backend-Wallet-Address", walletAddress);
        if (additionalHeaders != null)
        {
            foreach (var header in additionalHeaders)
            {
                engineClient.AddHeader(header.Key, header.Value);
            }
        }
        var wallet = new EngineWallet(client, engineClient, engineUrl, walletAddress, timeoutSeconds);
        Utils.TrackConnection(wallet);
        return wallet;
    }

    #endregion

    #region Wallet Specific

    public static async Task<string> WaitForQueueId(IThirdwebHttpClient httpClient, string engineUrl, string queueId)
    {
        var transactionHash = string.Empty;
        while (string.IsNullOrEmpty(transactionHash))
        {
            await ThirdwebTask.Delay(100);

            var statusResponse = await httpClient.GetAsync($"{engineUrl}/transaction/status/{queueId}");
            var content = await statusResponse.Content.ReadAsStringAsync();
            var response = JObject.Parse(content);

            var isErrored = response["result"]?["status"]?.ToString() is "errored" or "cancelled";
            if (isErrored)
            {
                throw new Exception($"Failed to send transaction: {response["result"]?["errorMessage"]?.ToString()}");
            }

            transactionHash = response["result"]?["transactionHash"]?.ToString();
        }
        return transactionHash;
    }

    private object ToEngineTransaction(ThirdwebTransactionInput transaction)
    {
        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        return new
        {
            toAddress = transaction.To,
            data = transaction.Data,
            value = transaction.Value?.HexValue ?? "0x00",
            authorizationList = transaction.AuthorizationList != null && transaction.AuthorizationList.Count > 0
                ? transaction.AuthorizationList
                    .Select(
                        authorization =>
                            new
                            {
                                chainId = authorization.ChainId.HexToNumber(),
                                address = authorization.Address,
                                nonce = authorization.Nonce.HexToNumber(),
                                yParity = authorization.YParity.HexToNumber(),
                                r = authorization.R,
                                s = authorization.S
                            }
                    )
                    .ToArray()
                : null,
            txOverrides = this._timeoutSeconds != null || transaction.Gas != null || transaction.GasPrice != null || transaction.MaxFeePerGas != null || transaction.MaxPriorityFeePerGas != null
                ? new
                {
                    gas = transaction.Gas?.Value.ToString(),
                    gasPrice = transaction.GasPrice?.Value.ToString(),
                    maxFeePerGas = transaction.MaxFeePerGas?.Value.ToString(),
                    maxPriorityFeePerGas = transaction.MaxPriorityFeePerGas?.Value.ToString(),
                    timeoutSeconds = this._timeoutSeconds,
                }
                : null,
        };
    }

    #endregion

    #region IThirdwebWallet

    public Task<string> GetAddress()
    {
        if (!string.IsNullOrEmpty(this._walletAddress))
        {
            return Task.FromResult(this._walletAddress.ToChecksumAddress());
        }
        else
        {
            return Task.FromResult(this._walletAddress);
        }
    }

    public Task<string> EthSign(byte[] rawMessage)
    {
        if (rawMessage == null)
        {
            throw new ArgumentNullException(nameof(rawMessage), "Message to sign cannot be null.");
        }

        throw new NotImplementedException();
    }

    public Task<string> EthSign(string message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message), "Message to sign cannot be null.");
        }

        throw new NotImplementedException();
    }

    public async Task<string> PersonalSign(byte[] rawMessage)
    {
        if (rawMessage == null)
        {
            throw new ArgumentNullException(nameof(rawMessage), "Message to sign cannot be null.");
        }

        var url = $"{this._engineUrl}/backend-wallet/sign-message";
        var payload = new { messagePayload = new { message = rawMessage.BytesToHex(), isBytes = true } };

        var requestContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

        var response = await this._engineClient.PostAsync(url, requestContent).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JObject.Parse(content)["result"].Value<string>();
    }

    public async Task<string> PersonalSign(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentNullException(nameof(message), "Message to sign cannot be null.");
        }

        var url = $"{this._engineUrl}/backend-wallet/sign-message";
        var payload = new { messagePayload = new { message, isBytes = false } };

        var requestContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

        var response = await this._engineClient.PostAsync(url, requestContent).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JObject.Parse(content)["result"].Value<string>();
    }

    public async Task<string> SignTypedDataV4(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            throw new ArgumentNullException(nameof(json), "Json to sign cannot be null.");
        }

        var processedJson = Utils.PreprocessTypedDataJson(json);
        // TODO: remove this sanitization when engine is upgraded to match spec
        processedJson = processedJson.Replace("message", "value");
        var tempObj = JObject.Parse(processedJson);
        _ = tempObj["types"].Value<JObject>().Remove("EIP712Domain");
        processedJson = tempObj.ToString();

        var url = $"{this._engineUrl}/backend-wallet/sign-typed-data";

        var requestContent = new StringContent(processedJson, Encoding.UTF8, "application/json");

        var response = await this._engineClient.PostAsync(url, requestContent).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JObject.Parse(content)["result"].Value<string>();
    }

    public async Task<string> SignTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData)
        where TDomain : IDomain
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data), "Data to sign cannot be null.");
        }

        var safeJson = Utils.ToJsonExternalWalletFriendly(typedData, data);
        return await this.SignTypedDataV4(safeJson).ConfigureAwait(false);
    }

    public async Task<string> SignTransaction(ThirdwebTransactionInput transaction)
    {
        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        object payload = new { transaction = this.ToEngineTransaction(transaction), };

        var url = $"{this._engineUrl}/backend-wallet/sign-transaction";

        var requestContent = new StringContent(JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), Encoding.UTF8, "application/json");

        var response = await this._engineClient.PostAsync(url, requestContent).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JObject.Parse(content)["result"].Value<string>();
    }

    public Task<bool> IsConnected()
    {
        return Task.FromResult(this._walletAddress != null);
    }

    public async Task<string> SendTransaction(ThirdwebTransactionInput transaction)
    {
        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        var payload = this.ToEngineTransaction(transaction);

        var url = $"{this._engineUrl}/backend-wallet/{transaction.ChainId.Value}/send-transaction";

        var requestContent = new StringContent(JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), Encoding.UTF8, "application/json");

        var response = await this._engineClient.PostAsync(url, requestContent).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var queueId = JObject.Parse(content)["result"]?["queueId"]?.ToString() ?? throw new Exception("Failed to queue the transaction");
        return await WaitForQueueId(this._engineClient, this._engineUrl, queueId).ConfigureAwait(false);
    }

    public async Task<ThirdwebTransactionReceipt> ExecuteTransaction(ThirdwebTransactionInput transactionInput)
    {
        var hash = await this.SendTransaction(transactionInput);
        return await ThirdwebTransaction.WaitForTransactionReceipt(this.Client, transactionInput.ChainId.Value, hash).ConfigureAwait(false);
    }

    public Task Disconnect()
    {
        return Task.CompletedTask;
    }

    public virtual Task<string> RecoverAddressFromEthSign(string message, string signature)
    {
        throw new InvalidOperationException();
    }

    public virtual Task<string> RecoverAddressFromPersonalSign(string message, string signature)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentNullException(nameof(message), "Message to sign cannot be null.");
        }

        if (string.IsNullOrEmpty(signature))
        {
            throw new ArgumentNullException(nameof(signature), "Signature cannot be null.");
        }

        var signer = new EthereumMessageSigner();
        var address = signer.EncodeUTF8AndEcRecover(message, signature);
        return Task.FromResult(address);
    }

    public virtual Task<string> RecoverAddressFromTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData, string signature)
        where TDomain : IDomain
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data), "Data to sign cannot be null.");
        }

        if (typedData == null)
        {
            throw new ArgumentNullException(nameof(typedData), "Typed data cannot be null.");
        }

        if (signature == null)
        {
            throw new ArgumentNullException(nameof(signature), "Signature cannot be null.");
        }

        var signer = new Eip712TypedDataSigner();
        var address = signer.RecoverFromSignatureV4(data, typedData, signature);
        return Task.FromResult(address);
    }

    public Task<EIP7702Authorization> SignAuthorization(BigInteger chainId, string contractAddress, bool willSelfExecute)
    {
        throw new NotImplementedException();
    }

    public Task SwitchNetwork(BigInteger chainId)
    {
        return Task.CompletedTask;
    }

    public Task<List<LinkedAccount>> LinkAccount(
        IThirdwebWallet walletToLink,
        string otp = null,
        bool? isMobile = null,
        Action<string> browserOpenAction = null,
        string mobileRedirectScheme = "thirdweb://",
        IThirdwebBrowser browser = null,
        BigInteger? chainId = null,
        string jwt = null,
        string payload = null,
        string defaultSessionIdOverride = null,
        List<string> forceWalletIds = null
    )
    {
        throw new NotImplementedException();
    }

    public Task<List<LinkedAccount>> UnlinkAccount(LinkedAccount accountToUnlink)
    {
        throw new NotImplementedException();
    }

    public Task<List<LinkedAccount>> GetLinkedAccounts()
    {
        throw new NotImplementedException();
    }

    #endregion
}
