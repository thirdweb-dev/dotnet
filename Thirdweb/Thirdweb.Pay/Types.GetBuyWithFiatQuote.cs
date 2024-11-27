using Newtonsoft.Json;

namespace Thirdweb.Pay;

/// <summary>
/// Parameters for getting a quote for buying with fiat.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BuyWithFiatQuoteParams"/> class.
/// </remarks>
public class BuyWithFiatQuoteParams(
    string fromCurrencySymbol,
    string toAddress,
    string toChainId,
    string toTokenAddress,
    string fromAmount = null,
    string fromAmountUnits = null,
    string toAmount = null,
    string toAmountWei = null,
    double? maxSlippageBPS = null,
    bool isTestMode = false,
    string preferredProvider = null
    )
{
    /// <summary>
    /// The symbol of the currency to be used for the purchase.
    /// </summary>
    [JsonProperty("fromCurrencySymbol")]
    public string FromCurrencySymbol { get; set; } = fromCurrencySymbol;

    /// <summary>
    /// The amount of the currency to be used for the purchase.
    /// </summary>
    [JsonProperty("fromAmount")]
    public string FromAmount { get; set; } = fromAmount;

    /// <summary>
    /// The units of the currency amount.
    /// </summary>
    [JsonProperty("fromAmountUnits")]
    public string FromAmountUnits { get; set; } = fromAmountUnits;

    /// <summary>
    /// The provider to use on the application for thirdweb pay
    /// </summary>
    [JsonProperty("preferredProvider")]
    public string PreferredProvider { get; set; } = preferredProvider;
    /// <summary>
    /// The address to receive the purchased tokens.
    /// </summary>
    [JsonProperty("toAddress")]
    public string ToAddress { get; set; } = toAddress;

    /// <summary>
    /// The chain ID of the destination token.
    /// </summary>
    [JsonProperty("toChainId")]
    public string ToChainId { get; set; } = toChainId;

    /// <summary>
    /// The address of the destination token.
    /// </summary>
    [JsonProperty("toTokenAddress")]
    public string ToTokenAddress { get; set; } = toTokenAddress;

    /// <summary>
    /// The amount of the destination token.
    /// </summary>
    [JsonProperty("toAmount")]
    public string ToAmount { get; set; } = toAmount;

    /// <summary>
    /// The amount of the destination token in wei.
    /// </summary>
    [JsonProperty("toAmountWei")]
    public string ToAmountWei { get; set; } = toAmountWei;

    /// <summary>
    /// The maximum slippage in basis points.
    /// </summary>
    [JsonProperty("maxSlippageBPS")]
    public double? MaxSlippageBPS { get; set; } = maxSlippageBPS;

    /// <summary>
    /// Indicates whether the transaction is in test mode.
    /// </summary>
    [JsonProperty("isTestMode")]
    public bool IsTestMode { get; set; } = isTestMode;
}

/// <summary>
/// Represents the result of a quote for buying with fiat.
/// </summary>
public class BuyWithFiatQuoteResult
{
    /// <summary>
    /// Gets or sets the intent ID of the quote.
    /// </summary>
    [JsonProperty("intentId")]
    public string IntentId { get; set; }

    /// <summary>
    /// Gets or sets the recipient address.
    /// </summary>
    [JsonProperty("toAddress")]
    public string ToAddress { get; set; }

    /// <summary>
    /// Gets or sets the details of the source currency.
    /// </summary>
    [JsonProperty("fromCurrency")]
    public OnRampCurrency FromCurrency { get; set; }

    /// <summary>
    /// Gets or sets the details of the source currency including fees.
    /// </summary>
    [JsonProperty("fromCurrencyWithFees")]
    public OnRampCurrency FromCurrencyWithFees { get; set; }

    /// <summary>
    /// Gets or sets the on-ramp token details.
    /// </summary>
    [JsonProperty("onRampToken")]
    public OnRampToken OnRampToken { get; set; }

    /// <summary>
    /// Gets or sets the details of the destination token.
    /// </summary>
    [JsonProperty("toToken")]
    public Token ToToken { get; set; }

    /// <summary>
    /// Gets or sets the estimated minimum amount of the destination token in wei.
    /// </summary>
    [JsonProperty("estimatedToAmountMinWei")]
    public string EstimatedToAmountMinWei { get; set; }

    /// <summary>
    /// Gets or sets the estimated minimum amount of the destination token.
    /// </summary>
    [JsonProperty("estimatedToAmountMin")]
    public string EstimatedToAmountMin { get; set; }

    /// <summary>
    /// Gets or sets the list of processing fees.
    /// </summary>
    [JsonProperty("processingFees")]
    public List<OnRampFees> ProcessingFees { get; set; }

    /// <summary>
    /// Gets or sets the estimated duration of the transaction in seconds.
    /// </summary>
    [JsonProperty("estimatedDurationSeconds")]
    public string EstimatedDurationSeconds { get; set; }

    /// <summary>
    /// Gets or sets the maximum slippage in basis points.
    /// </summary>
    [JsonProperty("maxSlippageBPS")]
    public double MaxSlippageBPS { get; set; }

    /// <summary>
    /// Gets or sets the on-ramp link for the transaction.
    /// </summary>
    [JsonProperty("onRampLink")]
    public string OnRampLink { get; set; }
}

/// <summary>
/// Represents an on-ramp token.
/// </summary>
public class OnRampToken
{
    /// <summary>
    /// Gets or sets the token details.
    /// </summary>
    [JsonProperty("token")]
    public Token Token { get; set; }

    /// <summary>
    /// Gets or sets the amount in wei.
    /// </summary>
    [JsonProperty("amountWei")]
    public string AmountWei { get; set; }

    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    [JsonProperty("amount")]
    public string Amount { get; set; }

    /// <summary>
    /// Gets or sets the amount in USD cents.
    /// </summary>
    [JsonProperty("amountUSDCents")]
    public double AmountUSDCents { get; set; }
}

/// <summary>
/// Represents on-ramp fees.
/// </summary>
public class OnRampFees
{
    /// <summary>
    /// Gets or sets the fee amount.
    /// </summary>
    [JsonProperty("amount")]
    public string Amount { get; set; }

    /// <summary>
    /// Gets or sets the units of the fee amount.
    /// </summary>
    [JsonProperty("amountUnits")]
    public string AmountUnits { get; set; }

    /// <summary>
    /// Gets or sets the number of decimals for the fee amount.
    /// </summary>
    [JsonProperty("decimals")]
    public int Decimals { get; set; }

    /// <summary>
    /// Gets or sets the currency symbol for the fee.
    /// </summary>
    [JsonProperty("currencySymbol")]
    public string CurrencySymbol { get; set; }

    /// <summary>
    /// Gets or sets the type of the fee.
    /// </summary>
    [JsonProperty("feeType")]
    public string FeeType { get; set; }
}

/// <summary>
/// Represents the response for getting a fiat quote.
/// </summary>
public class GetFiatQuoteResponse
{
    /// <summary>
    /// Gets or sets the result of the fiat quote.
    /// </summary>
    [JsonProperty("result")]
    public BuyWithFiatQuoteResult Result { get; set; }
}
