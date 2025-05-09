using System.Numerics;
using Newtonsoft.Json;

namespace Thirdweb.Bridge;

public class ThirdwebBridge
{
    private readonly IThirdwebHttpClient _httpClient;

    internal ThirdwebBridge(ThirdwebClient client)
    {
        this._httpClient = client.HttpClient;
    }

    /// <summary>
    /// Create a new instance of the ThirdwebBridge class.
    /// </summary>
    /// <param name="client">The ThirdwebClient instance.</param>
    /// <returns>A new instance of <see cref="ThirdwebBridge"/>.</returns>
    public static Task<ThirdwebBridge> Create(ThirdwebClient client)
    {
        return Task.FromResult(new ThirdwebBridge(client));
    }

    #region Buy

    /// <summary>
    /// Get a quote for buying a specific amount of tokens on any chain.
    /// </summary>
    /// <param name="originChainId">The chain ID of the origin chain.</param>
    /// <param name="originTokenAddress">The address of the token on the origin chain.</param>
    /// <param name="destinationChainId">The chain ID of the destination chain.</param>
    /// <param name="destinationTokenAddress">The address of the token on the destination chain.</param>
    /// <param name="buyAmountWei">The amount of tokens to buy in wei.</param>
    /// <param name="maxSteps">The maximum number of steps in the returned route.</param>
    /// <returns>A <see cref="BuyQuoteData"/> object representing the quote.</returns>
    /// <exception cref="ArgumentException">Thrown when one of the parameters is invalid.</exception>
    public async Task<BuyQuoteData> Buy_Quote(
        BigInteger originChainId,
        string originTokenAddress,
        BigInteger destinationChainId,
        string destinationTokenAddress,
        BigInteger buyAmountWei,
        int maxSteps = 3
    )
    {
        if (originChainId <= 0)
        {
            throw new ArgumentException("originChainId cannot be less than or equal to 0", nameof(originChainId));
        }

        if (destinationChainId <= 0)
        {
            throw new ArgumentException("destinationChainId cannot be less than or equal to 0", nameof(destinationChainId));
        }

        if (!Utils.IsValidAddress(originTokenAddress))
        {
            throw new ArgumentException("originTokenAddress is not a valid address", nameof(originTokenAddress));
        }

        if (buyAmountWei <= 0)
        {
            throw new ArgumentException("buyAmountWei cannot be less than or equal to 0", nameof(buyAmountWei));
        }

        var url = $"{Constants.BRIDGE_API_URL}/v1/buy/quote";
        var queryParams = new Dictionary<string, string>
        {
            { "originChainId", originChainId.ToString() },
            { "originTokenAddress", originTokenAddress },
            { "destinationChainId", destinationChainId.ToString() },
            { "destinationTokenAddress", destinationTokenAddress },
            { "buyAmountWei", buyAmountWei.ToString() },
            { "maxSteps", maxSteps.ToString() }
        };
        url = AppendQueryParams(url, queryParams);

        var response = await this._httpClient.GetAsync(url).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var result = JsonConvert.DeserializeObject<ResponseModel<BuyQuoteData>>(responseContent);
        return result.Data;
    }

    /// <summary>
    /// Get the transactions required to buy a specific amount of tokens on any chain, alongside the quote.
    /// </summary>
    /// <param name="originChainId">The chain ID of the origin chain.</param>
    /// <param name="originTokenAddress">The address of the token on the origin chain.</param>
    /// <param name="destinationChainId">The chain ID of the destination chain.</param>
    /// <param name="destinationTokenAddress">The address of the token on the destination chain.</param>
    /// <param name="buyAmountWei">The amount of tokens to buy in wei.</param>
    /// <param name="sender">The address of the sender.</param>
    /// <param name="receiver">The address of the receiver.</param>
    /// <param name="maxSteps">The maximum number of steps in the returned route.</param>
    /// <param name="purchaseData">Arbitrary purchase data to be included with the payment and returned with all webhooks and status checks.</param>
    /// <returns>A <see cref="BuyPrepareData"/> object representing the prepare data.</returns>
    /// <exception cref="ArgumentException">Thrown when one of the parameters is invalid.</exception>
    public async Task<BuyPrepareData> Buy_Prepare(
        BigInteger originChainId,
        string originTokenAddress,
        BigInteger destinationChainId,
        string destinationTokenAddress,
        BigInteger buyAmountWei,
        string sender,
        string receiver,
        int maxSteps = 3,
        object purchaseData = null
    )
    {
        if (originChainId <= 0)
        {
            throw new ArgumentException("originChainId cannot be less than or equal to 0", nameof(originChainId));
        }

        if (destinationChainId <= 0)
        {
            throw new ArgumentException("destinationChainId cannot be less than or equal to 0", nameof(destinationChainId));
        }

        if (!Utils.IsValidAddress(originTokenAddress))
        {
            throw new ArgumentException("originTokenAddress is not a valid address", nameof(originTokenAddress));
        }

        if (buyAmountWei <= 0)
        {
            throw new ArgumentException("buyAmountWei cannot be less than or equal to 0", nameof(buyAmountWei));
        }

        if (!Utils.IsValidAddress(sender))
        {
            throw new ArgumentException("sender is not a valid address", nameof(sender));
        }

        if (!Utils.IsValidAddress(receiver))
        {
            throw new ArgumentException("receiver is not a valid address", nameof(receiver));
        }

        var url = $"{Constants.BRIDGE_API_URL}/v1/buy/prepare";
        var queryParams = new Dictionary<string, string>
        {
            { "originChainId", originChainId.ToString() },
            { "originTokenAddress", originTokenAddress },
            { "destinationChainId", destinationChainId.ToString() },
            { "destinationTokenAddress", destinationTokenAddress },
            { "buyAmountWei", buyAmountWei.ToString() },
            { "sender", sender },
            { "receiver", receiver },
            { "maxSteps", maxSteps.ToString() },
            { "purchaseData", purchaseData != null ? JsonConvert.SerializeObject(purchaseData) : null }
        };
        url = AppendQueryParams(url, queryParams);

        var response = await this._httpClient.GetAsync(url).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var result = JsonConvert.DeserializeObject<ResponseModel<BuyPrepareData>>(responseContent);
        return result.Data;
    }

    #endregion

    #region Sell

    /// <summary>
    /// Get a quote for selling a specific amount of tokens on any chain.
    /// </summary>
    /// <param name="originChainId">The chain ID of the origin chain.</param>
    /// <param name="originTokenAddress">The address of the token on the origin chain.</param>
    /// <param name="destinationChainId">The chain ID of the destination chain.</param>
    /// <param name="destinationTokenAddress">The address of the token on the destination chain.</param>
    /// <param name="sellAmountWei">The amount of tokens to sell in wei.</param>
    /// <param name="maxSteps">The maximum number of steps in the returned route.</param>
    /// <returns>A <see cref="SellQuoteData"/> object representing the quote.</returns>
    /// <exception cref="ArgumentException">Thrown when one of the parameters is invalid.</exception>
    public async Task<SellQuoteData> Sell_Quote(
        BigInteger originChainId,
        string originTokenAddress,
        BigInteger destinationChainId,
        string destinationTokenAddress,
        BigInteger sellAmountWei,
        int maxSteps = 3
    )
    {
        if (originChainId <= 0)
        {
            throw new ArgumentException("originChainId cannot be less than or equal to 0", nameof(originChainId));
        }

        if (destinationChainId <= 0)
        {
            throw new ArgumentException("destinationChainId cannot be less than or equal to 0", nameof(destinationChainId));
        }

        if (!Utils.IsValidAddress(originTokenAddress))
        {
            throw new ArgumentException("originTokenAddress is not a valid address", nameof(originTokenAddress));
        }

        if (sellAmountWei <= 0)
        {
            throw new ArgumentException("sellAmountWei cannot be less than or equal to 0", nameof(sellAmountWei));
        }

        var url = $"{Constants.BRIDGE_API_URL}/v1/sell/quote";
        var queryParams = new Dictionary<string, string>
        {
            { "originChainId", originChainId.ToString() },
            { "originTokenAddress", originTokenAddress },
            { "destinationChainId", destinationChainId.ToString() },
            { "destinationTokenAddress", destinationTokenAddress },
            { "sellAmountWei", sellAmountWei.ToString() },
            { "maxSteps", maxSteps.ToString() }
        };
        url = AppendQueryParams(url, queryParams);

        var response = await this._httpClient.GetAsync(url).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var result = JsonConvert.DeserializeObject<ResponseModel<SellQuoteData>>(responseContent);
        return result.Data;
    }

    /// <summary>
    /// Get the transactions required to sell a specific amount of tokens on any chain, alongside the quote.
    /// </summary>
    /// <param name="originChainId">The chain ID of the origin chain.</param>
    /// <param name="originTokenAddress">The address of the token on the origin chain.</param>
    /// <param name="destinationChainId">The chain ID of the destination chain.</param>
    /// <param name="destinationTokenAddress">The address of the token on the destination chain.</param>
    /// <param name="sellAmountWei">The amount of tokens to sell in wei.</param>
    /// <param name="sender">The address of the sender.</param>
    /// <param name="receiver">The address of the receiver.</param>
    /// <param name="maxSteps">The maximum number of steps in the returned route.</param>
    /// <param name="purchaseData">Arbitrary purchase data to be included with the payment and returned with all webhooks and status checks.</param>
    /// <returns>A <see cref="SellPrepareData"/> object representing the prepare data.</returns>
    /// <exception cref="ArgumentException">Thrown when one of the parameters is invalid.</exception>
    public async Task<SellPrepareData> Sell_Prepare(
        BigInteger originChainId,
        string originTokenAddress,
        BigInteger destinationChainId,
        string destinationTokenAddress,
        BigInteger sellAmountWei,
        string sender,
        string receiver,
        int maxSteps = 3,
        object purchaseData = null
    )
    {
        if (originChainId <= 0)
        {
            throw new ArgumentException("originChainId cannot be less than or equal to 0", nameof(originChainId));
        }

        if (destinationChainId <= 0)
        {
            throw new ArgumentException("destinationChainId cannot be less than or equal to 0", nameof(destinationChainId));
        }

        if (!Utils.IsValidAddress(originTokenAddress))
        {
            throw new ArgumentException("originTokenAddress is not a valid address", nameof(originTokenAddress));
        }

        if (sellAmountWei <= 0)
        {
            throw new ArgumentException("sellAmountWei cannot be less than or equal to 0", nameof(sellAmountWei));
        }

        if (!Utils.IsValidAddress(sender))
        {
            throw new ArgumentException("sender is not a valid address", nameof(sender));
        }

        if (!Utils.IsValidAddress(receiver))
        {
            throw new ArgumentException("receiver is not a valid address", nameof(receiver));
        }

        var url = $"{Constants.BRIDGE_API_URL}/v1/sell/prepare";
        var queryParams = new Dictionary<string, string>
        {
            { "originChainId", originChainId.ToString() },
            { "originTokenAddress", originTokenAddress },
            { "destinationChainId", destinationChainId.ToString() },
            { "destinationTokenAddress", destinationTokenAddress },
            { "sellAmountWei", sellAmountWei.ToString() },
            { "sender", sender },
            { "receiver", receiver },
            { "maxSteps", maxSteps.ToString() },
            { "purchaseData", purchaseData != null ? JsonConvert.SerializeObject(purchaseData) : null }
        };
        url = AppendQueryParams(url, queryParams);

        var response = await this._httpClient.GetAsync(url).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var result = JsonConvert.DeserializeObject<ResponseModel<SellPrepareData>>(responseContent);
        return result.Data;
    }

    #endregion

    #region Transfer

    /// <summary>
    /// Get the transactions required to transfer a specific amount of tokens on any chain.
    /// </summary>
    /// <param name="chainId">The chain ID of the token.</param>
    /// <param name="tokenAddress">The address of the token.</param>
    /// <param name="transferAmountWei">The amount of tokens to transfer in wei.</param>
    /// <param name="sender">The address of the sender.</param>
    /// <param name="receiver">The address of the receiver.</param>
    /// <param name="feePayer">The fee payer (default is "sender").</param>
    /// <param name="purchaseData">Arbitrary purchase data to be included with the payment and returned with all webhooks and status checks.</param>
    /// <returns>A <see cref="TransferPrepareData"/> object representing the prepare data.</returns>
    /// <exception cref="ArgumentException">Thrown when one of the parameters is invalid.</exception>
    public async Task<TransferPrepareData> Transfer_Prepare(
        BigInteger chainId,
        string tokenAddress,
        BigInteger transferAmountWei,
        string sender,
        string receiver,
        string feePayer = "sender",
        object purchaseData = null
    )
    {
        if (chainId <= 0)
        {
            throw new ArgumentException("chainId cannot be less than or equal to 0", nameof(chainId));
        }

        if (!Utils.IsValidAddress(tokenAddress))
        {
            throw new ArgumentException("tokenAddress is not a valid address", nameof(tokenAddress));
        }

        if (transferAmountWei <= 0)
        {
            throw new ArgumentException("transferAmountWei cannot be less than or equal to 0", nameof(transferAmountWei));
        }

        if (!Utils.IsValidAddress(sender))
        {
            throw new ArgumentException("sender is not a valid address", nameof(sender));
        }

        if (!Utils.IsValidAddress(receiver))
        {
            throw new ArgumentException("receiver is not a valid address", nameof(receiver));
        }

        var url = $"{Constants.BRIDGE_API_URL}/v1/transfer/prepare";
        var queryParams = new Dictionary<string, string>
        {
            { "chainId", chainId.ToString() },
            { "tokenAddress", tokenAddress },
            { "transferAmountWei", transferAmountWei.ToString() },
            { "sender", sender },
            { "receiver", receiver },
            { "feePayer", feePayer },
            { "purchaseData", purchaseData != null ? JsonConvert.SerializeObject(purchaseData) : null }
        };
        url = AppendQueryParams(url, queryParams);

        var response = await this._httpClient.GetAsync(url).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var result = JsonConvert.DeserializeObject<ResponseModel<TransferPrepareData>>(responseContent);
        return result.Data;
    }

    #endregion

    #region Onramp

    public async Task<OnrampPrepareData> Onramp_Prepare(
        OnrampProvider onramp,
        BigInteger chainId,
        string tokenAddress,
        string amount,
        string receiver,
        string onrampTokenAddress = null,
        BigInteger? onrampChainId = null,
        string currency = "USD",
        int? maxSteps = 3,
        List<BigInteger> excludeChainIds = null,
        object purchaseData = null
    )
    {
        if (chainId <= 0)
        {
            throw new ArgumentException("chainId cannot be less than or equal to 0", nameof(chainId));
        }

        if (!Utils.IsValidAddress(tokenAddress))
        {
            throw new ArgumentException("tokenAddress is not a valid address", nameof(tokenAddress));
        }

        if (string.IsNullOrWhiteSpace(amount))
        {
            throw new ArgumentException("amount cannot be null or empty", nameof(amount));
        }

        if (!Utils.IsValidAddress(receiver))
        {
            throw new ArgumentException("receiver is not a valid address", nameof(receiver));
        }

        var url = $"{Constants.BRIDGE_API_URL}/v1/onramp/prepare";
        var queryParams = new Dictionary<string, string>
        {
            { "onramp", onramp.ToString().ToLower() },
            { "chainId", chainId.ToString() },
            { "tokenAddress", tokenAddress },
            { "amount", amount },
            { "receiver", receiver },
            { "purchaseData", purchaseData != null ? JsonConvert.SerializeObject(purchaseData) : null },
            { "onrampTokenAddress", onrampTokenAddress },
            { "onrampChainId", onrampChainId?.ToString() },
            { "currency", currency },
            { "maxSteps", maxSteps?.ToString() }
        };

        if (excludeChainIds != null && excludeChainIds.Count > 0)
        {
            queryParams.Add("excludeChainIds", string.Join(",", excludeChainIds));
        }

        url = AppendQueryParams(url, queryParams);

        var response = await this._httpClient.GetAsync(url).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var result = JsonConvert.DeserializeObject<ResponseModel<OnrampPrepareData>>(responseContent);
        return result.Data;
    }

    public async Task<OnrampStatusData> Onramp_Status(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("id cannot be null or empty", nameof(id));
        }

        var url = $"{Constants.BRIDGE_API_URL}/v1/onramp/status";
        var queryParams = new Dictionary<string, string> { { "id", id } };
        url = AppendQueryParams(url, queryParams);

        var response = await this._httpClient.GetAsync(url).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var result = JsonConvert.DeserializeObject<ResponseModel<OnrampStatusData>>(responseContent);
        return result.Data;
    }

    #endregion

    #region Status

    /// <summary>
    /// Get the status of any bridge-initiated transaction.
    /// </summary>
    /// <param name="transactionHash">The hash of the transaction.</param>
    /// <param name="chainId">The chain ID of the transaction.</param>
    /// <returns>A <see cref="StatusData"/> object representing the status.</returns>
    /// <exception cref="ArgumentException">Thrown when one of the parameters is invalid.</exception>
    public async Task<StatusData> Status(string transactionHash, BigInteger chainId)
    {
        if (string.IsNullOrWhiteSpace(transactionHash))
        {
            throw new ArgumentException("transactionHash cannot be null or empty", nameof(transactionHash));
        }

        if (chainId <= 0)
        {
            throw new ArgumentException("chainId cannot be less than or equal to 0", nameof(chainId));
        }

        var url = $"{Constants.BRIDGE_API_URL}/v1/status";
        var queryParams = new Dictionary<string, string> { { "transactionHash", transactionHash }, { "chainId", chainId.ToString() } };
        url = AppendQueryParams(url, queryParams);

        var response = await this._httpClient.GetAsync(url).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var result = JsonConvert.DeserializeObject<ResponseModel<StatusData>>(responseContent);
        return result.Data;
    }

    #endregion

    private static string AppendQueryParams(string url, Dictionary<string, string> queryParams)
    {
        var query = new List<string>();
        foreach (var param in queryParams)
        {
            if (string.IsNullOrEmpty(param.Value))
            {
                continue;
            }
            query.Add($"{param.Key}={param.Value}");
        }

        return url + "?" + string.Join("&", query);
    }
}
