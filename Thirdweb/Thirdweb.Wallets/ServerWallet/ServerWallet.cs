using System.Numerics;
using System.Text;
using Nethereum.ABI.EIP712;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Thirdweb;

/// <summary>
/// Interact with vault-secured server wallets created from the Thirdweb project dashboard's Transactions tab.
/// </summary>
public partial class ServerWallet : IThirdwebWallet
{
    public ThirdwebClient Client { get; }
    public ThirdwebAccountType AccountType => ThirdwebAccountType.ExternalAccount;
    public string WalletId => "server";

    private readonly string _walletAddress;
    private readonly IThirdwebHttpClient _engineClient;
    private readonly ExecutionOptions _executionOptions;

    private readonly JsonSerializerSettings _jsonSerializerSettings = new() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

    internal ServerWallet(ThirdwebClient client, IThirdwebHttpClient engineClient, string walletAddress, ExecutionOptions executionOptions)
    {
        this.Client = client;
        this._walletAddress = walletAddress;
        this._engineClient = engineClient;
        this._executionOptions = executionOptions;
    }

    #region Creation

    /// <summary>
    /// Creates an instance of the ServerWallet.
    /// </summary>
    /// <param name="client">The Thirdweb client.</param>
    /// <param name="label">The label of your created server wallet.</param>
    /// <param name="executionOptions">The execution options for the server wallet, defaults to auto if not passed.</param>
    /// <param name="vaultAccessToken">The vault access token for the server wallet if self-managed.</param>
    /// <returns>A new instance of the ServerWallet.</returns>
    /// <exception cref="ArgumentNullException">Thrown when client or label is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no server wallets are found or the specified label does not match any existing server wallet.</exception>
    public static async Task<ServerWallet> Create(ThirdwebClient client, string label, ExecutionOptions executionOptions = null, string vaultAccessToken = null)
    {
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client), "Client cannot be null.");
        }

        if (string.IsNullOrEmpty(label))
        {
            throw new ArgumentNullException(nameof(label), "Label cannot be null or empty.");
        }

        var engineClient = Utils.ReconstructHttpClient(client.HttpClient, new Dictionary<string, string> { { "X-Secret-Key", client.SecretKey } });
        if (!string.IsNullOrEmpty(vaultAccessToken))
        {
            engineClient.AddHeader("X-Vault-Access-Token", vaultAccessToken);
        }
        var serverWalletListResponse = await engineClient.GetAsync($"{Constants.ENGINE_API_URL}/v1/accounts").ConfigureAwait(false);
        _ = serverWalletListResponse.EnsureSuccessStatusCode();
        var content = await serverWalletListResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

        var responseObj = JObject.Parse(content);
        var accounts = responseObj["result"]?["accounts"]?.ToObject<JArray>(); // TODO: Support pagination

        if (accounts == null || accounts.Count == 0)
        {
            throw new InvalidOperationException("No server wallets found in the account.");
        }

        var matchingAccount =
            accounts.FirstOrDefault(account => account["label"]?.ToString() == label)
            ?? throw new InvalidOperationException(
                $"Server wallet with label '{label}' not found. Available labels: {string.Join(", ", accounts.Select(a => a["label"]?.ToString()).Where(l => !string.IsNullOrEmpty(l)))}"
            );

        var signerWalletAddress = matchingAccount["address"]?.ToString().ToChecksumAddress();
        var smartWalletAddress = executionOptions is ERC4337ExecutionOptions ? matchingAccount["smartAccountAddress"]?.ToString() : null;
        if (string.IsNullOrEmpty(signerWalletAddress))
        {
            throw new InvalidOperationException($"Server wallet with label '{label}' found but has no address.");
        }

        executionOptions ??= new AutoExecutionOptions { IdempotencyKey = Guid.NewGuid().ToString(), From = signerWalletAddress.ToChecksumAddress() };
        if (executionOptions is ERC4337ExecutionOptions erc4337ExecutionOptions)
        {
            erc4337ExecutionOptions.SmartAccountAddress = smartWalletAddress;
            erc4337ExecutionOptions.SignerAddress = signerWalletAddress;
        }
        else if (executionOptions is EIP7702ExecutionOptions eip7702ExecutionOptions)
        {
            eip7702ExecutionOptions.From = signerWalletAddress.ToChecksumAddress();
        }
        else if (executionOptions is EOAExecutionOptions eoaExecutionOptions)
        {
            eoaExecutionOptions.From = signerWalletAddress.ToChecksumAddress();
        }
        else if (executionOptions is AutoExecutionOptions autoExecutionOptions)
        {
            autoExecutionOptions.From ??= signerWalletAddress.ToChecksumAddress();
        }
        else
        {
            throw new InvalidOperationException(
                $"Unsupported execution options type: {executionOptions.GetType().Name}. Supported types are AutoExecutionOptions, EIP7702ExecutionOptions, EOAExecutionOptions, and ERC4337ExecutionOptions."
            );
        }

        var wallet = new ServerWallet(client, engineClient, smartWalletAddress ?? signerWalletAddress, executionOptions);
        Utils.TrackConnection(wallet);
        return wallet;
    }

    #endregion

    #region Wallet Specific

    public async Task<string> WaitForTransactionHash(string txid)
    {
        var cancellationToken = new CancellationTokenSource();
        cancellationToken.CancelAfter(this.Client.FetchTimeoutOptions.GetTimeout(TimeoutType.Other));
        var transactionHash = string.Empty;
        while (string.IsNullOrEmpty(transactionHash) && !cancellationToken.IsCancellationRequested)
        {
            await ThirdwebTask.Delay(100);

            var statusResponse = await this._engineClient.GetAsync($"{Constants.ENGINE_API_URL}/v1/transactions?id={txid}").ConfigureAwait(false);
            var content = await statusResponse.Content.ReadAsStringAsync();
            var response = JObject.Parse(content);
            var transaction = (response["result"]?["transactions"]?.FirstOrDefault()) ?? throw new Exception($"Failed to fetch transaction status for ID: {txid}");
            var errorMessage = transaction?["errorMessage"]?.ToString();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                throw new Exception($"Sending transaction errored: {errorMessage}");
            }

            transactionHash = transaction?["transactionHash"]?.ToString();
        }
        return transactionHash;
    }

    private object ToEngineTransaction(ThirdwebTransactionInput transaction)
    {
        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        this._executionOptions.ChainId = transaction.ChainId;

        return new
        {
            executionOptions = this._executionOptions,
            @params = new[]
            {
                new
                {
                    to = transaction.To,
                    data = transaction.Data ?? "0x",
                    value = transaction.Value?.HexValue ?? "0x00",
                    authorizationList = transaction.AuthorizationList != null && transaction.AuthorizationList.Count > 0
                        ? transaction
                            .AuthorizationList.Select(authorization => new
                            {
                                chainId = authorization.ChainId.HexToNumber(),
                                address = authorization.Address,
                                nonce = authorization.Nonce.HexToNumber(),
                                yParity = authorization.YParity.HexToNumber(),
                                r = authorization.R,
                                s = authorization.S,
                            })
                            .ToArray()
                        : null,
                },
            },
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

    public async Task<string> PersonalSign(byte[] rawMessage)
    {
        if (rawMessage == null)
        {
            throw new ArgumentNullException(nameof(rawMessage), "Message to sign cannot be null.");
        }

        var url = $"{Constants.ENGINE_API_URL}/v1/sign/message";

        var address = await this.GetAddress();

        var payload = new
        {
            signingOptions = new
            {
                type = "auto",
                from = address,
                chainId = this._executionOptions.ChainId,
            },
            @params = new[] { new { message = rawMessage.BytesToHex(), format = "hex" } },
        };

        var requestContent = new StringContent(JsonConvert.SerializeObject(payload, this._jsonSerializerSettings), Encoding.UTF8, "application/json");

        var response = await this._engineClient.PostAsync(url, requestContent).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JObject.Parse(content)["result"]?[0]?["result"]?["signature"].Value<string>();
    }

    public async Task<string> PersonalSign(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentNullException(nameof(message), "Message to sign cannot be null.");
        }

        var url = $"{Constants.ENGINE_API_URL}/v1/sign/message";

        var address = await this.GetAddress();

        var payload = new
        {
            signingOptions = new
            {
                type = "auto",
                from = address,
                chainId = this._executionOptions.ChainId,
            },
            @params = new[] { new { message, format = "text" } },
        };

        var requestContent = new StringContent(JsonConvert.SerializeObject(payload, this._jsonSerializerSettings), Encoding.UTF8, "application/json");

        var response = await this._engineClient.PostAsync(url, requestContent).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JObject.Parse(content)["result"]?[0]?["result"]?["signature"].Value<string>();
    }

    public async Task<string> SignTypedDataV4(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            throw new ArgumentNullException(nameof(json), "Json to sign cannot be null.");
        }

        var processedJson = Utils.PreprocessTypedDataJson(json);

        var url = $"{Constants.ENGINE_API_URL}/v1/sign/typed-data";

        var address = await this.GetAddress();

        var payload = new
        {
            signingOptions = new
            {
                type = "auto",
                from = address,
                chainId = BigInteger.Parse(JObject.Parse(processedJson)["domain"]?["chainId"]?.Value<string>()),
            },
            @params = new[] { processedJson },
        };
        var requestContent = new StringContent(JsonConvert.SerializeObject(payload, this._jsonSerializerSettings), Encoding.UTF8, "application/json");

        var response = await this._engineClient.PostAsync(url, requestContent).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JObject.Parse(content)["result"]?[0]?["result"]?["signature"].Value<string>();
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

    public Task<string> SignTransaction(ThirdwebTransactionInput transaction)
    {
        throw new NotImplementedException("SignTransaction is not implemented for ServerWallet. Use SendTransaction instead.");
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

        var url = $"{Constants.ENGINE_API_URL}/v1/write/transaction";

        var requestContent = new StringContent(JsonConvert.SerializeObject(payload, this._jsonSerializerSettings), Encoding.UTF8, "application/json");

        var response = await this._engineClient.PostAsync(url, requestContent).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var queuedTransactionResponse = JsonConvert.DeserializeObject<QueuedTransactionResponse>(content);
        var txid = queuedTransactionResponse.Result?.Transactions?.FirstOrDefault()?.Id;
        if (string.IsNullOrEmpty(txid))
        {
            throw new Exception("Failed to queue the transaction. No transaction ID returned.");
        }
        return await this.WaitForTransactionHash(txid).ConfigureAwait(false);
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
        string defaultSessionIdOverride = null
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
