using Newtonsoft.Json;

namespace Thirdweb.Pay;

/// <summary>
/// Represents the response for an on-ramp status.
/// </summary>
public class OnRampStatusResponse
{
    /// <summary>
    /// Gets or sets the result of the on-ramp status.
    /// </summary>
    [JsonProperty("result")]
    public BuyWithFiatStatusResult Result { get; set; }
}

/// <summary>
/// Represents the status result of buying with fiat.
/// </summary>
public class BuyWithFiatStatusResult
{
    /// <summary>
    /// Gets or sets the intent ID of the transaction.
    /// </summary>
    [JsonProperty("intentId")]
    public string IntentId { get; set; }

    /// <summary>
    /// Gets or sets the status of the transaction.
    /// </summary>
    [JsonProperty("status")]
    public string Status { get; set; }

    /// <summary>
    /// Gets or sets the recipient address.
    /// </summary>
    [JsonProperty("toAddress")]
    public string ToAddress { get; set; }

    /// <summary>
    /// Gets or sets the quote details for the on-ramp transaction.
    /// </summary>
    [JsonProperty("quote")]
    public OnRampQuote Quote { get; set; }

    /// <summary>
    /// Gets or sets the source transaction details.
    /// </summary>
    [JsonProperty("source")]
    public TransactionDetails Source { get; set; }

    /// <summary>
    /// Gets or sets the destination transaction details.
    /// </summary>
    [JsonProperty("destination")]
    public TransactionDetails Destination { get; set; }

    /// <summary>
    /// Gets or sets the failure message if the transaction fails.
    /// </summary>
    [JsonProperty("failureMessage")]
    public string FailureMessage { get; set; }
}

/// <summary>
/// Represents a quote for an on-ramp transaction.
/// </summary>
public class OnRampQuote
{
    /// <summary>
    /// Gets or sets the creation date of the quote.
    /// </summary>
    [JsonProperty("createdAt")]
    public string CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the estimated amount for the on-ramp transaction in wei.
    /// </summary>
    [JsonProperty("estimatedOnRampAmountWei")]
    public string EstimatedOnRampAmountWei { get; set; }

    /// <summary>
    /// Gets or sets the estimated amount for the on-ramp transaction.
    /// </summary>
    [JsonProperty("estimatedOnRampAmount")]
    public string EstimatedOnRampAmount { get; set; }

    /// <summary>
    /// Gets or sets the estimated amount of the destination token.
    /// </summary>
    [JsonProperty("estimatedToTokenAmount")]
    public string EstimatedToTokenAmount { get; set; }

    /// <summary>
    /// Gets or sets the estimated amount of the destination token in wei.
    /// </summary>
    [JsonProperty("estimatedToTokenAmountWei")]
    public string EstimatedToTokenAmountWei { get; set; }

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
    public Token OnRampToken { get; set; }

    /// <summary>
    /// Gets or sets the details of the destination token.
    /// </summary>
    [JsonProperty("toToken")]
    public Token ToToken { get; set; }

    /// <summary>
    /// Gets or sets the estimated duration of the transaction in seconds.
    /// </summary>
    [JsonProperty("estimatedDurationSeconds")]
    public long EstimatedDurationSeconds { get; set; }
}

/// <summary>
/// Represents the various statuses of an on-ramp transaction.
/// </summary>
public enum OnRampStatus
{
    /// <summary>
    /// No status.
    /// </summary>
    NONE,

    /// <summary>
    /// Payment is pending.
    /// </summary>
    PENDING_PAYMENT,

    /// <summary>
    /// Payment has failed.
    /// </summary>
    PAYMENT_FAILED,

    /// <summary>
    /// Pending on-ramp transfer.
    /// </summary>
    PENDING_ON_RAMP_TRANSFER,

    /// <summary>
    /// On-ramp transfer is in progress.
    /// </summary>
    ON_RAMP_TRANSFER_IN_PROGRESS,

    /// <summary>
    /// On-ramp transfer is completed.
    /// </summary>
    ON_RAMP_TRANSFER_COMPLETED,

    /// <summary>
    /// On-ramp transfer has failed.
    /// </summary>
    ON_RAMP_TRANSFER_FAILED,

    /// <summary>
    /// Crypto swap is required.
    /// </summary>
    CRYPTO_SWAP_REQUIRED,

    /// <summary>
    /// Crypto swap is completed.
    /// </summary>
    CRYPTO_SWAP_COMPLETED,

    /// <summary>
    /// Crypto swap fallback.
    /// </summary>
    CRYPTO_SWAP_FALLBACK,

    /// <summary>
    /// Crypto swap is in progress.
    /// </summary>
    CRYPTO_SWAP_IN_PROGRESS,

    /// <summary>
    /// Crypto swap has failed.
    /// </summary>
    CRYPTO_SWAP_FAILED,
}
