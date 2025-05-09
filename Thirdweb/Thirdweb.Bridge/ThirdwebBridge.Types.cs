using System.Numerics;
using Newtonsoft.Json;

namespace Thirdweb.Bridge;

/// <summary>
/// Represents the response model wrapping the result of an API call.
/// </summary>
/// <typeparam name="T">The type of the result.</typeparam>
internal class ResponseModel<T>
{
    /// <summary>
    /// The result returned by the API.
    /// </summary>
    [JsonProperty("data")]
    internal T Data { get; set; }
}

#region Common Types

/// <summary>
/// Represents the base intent object for different types of transactions.
/// </summary>
public class Intent
{
    /// <summary>
    /// The origin chain ID.
    /// </summary>
    [JsonProperty("originChainId")]
    public BigInteger OriginChainId { get; set; }

    /// <summary>
    /// The origin token address.
    /// </summary>
    [JsonProperty("originTokenAddress")]
    public string OriginTokenAddress { get; set; }

    /// <summary>
    /// The destination chain ID.
    /// </summary>
    [JsonProperty("destinationChainId")]
    public BigInteger DestinationChainId { get; set; }

    /// <summary>
    /// The destination token address.
    /// </summary>
    [JsonProperty("destinationTokenAddress")]
    public string DestinationTokenAddress { get; set; }

    /// <summary>
    /// The desired amount in wei.
    /// </summary>
    [JsonProperty("amount")]
    public string Amount { get; set; }

    /// <summary>
    /// The maximum number of steps in the returned route (optional).
    /// </summary>
    [JsonProperty("maxSteps", NullValueHandling = NullValueHandling.Ignore)]
    public int? MaxSteps { get; set; } = 3;
}

/// <summary>
/// Represents the common fields for both Buy and Sell transactions.
/// </summary>
public class QuoteData
{
    /// <summary>
    /// The amount (in wei) of the input token that must be paid to receive the desired amount.
    /// </summary>
    [JsonProperty("originAmount")]
    public string OriginAmount { get; set; }

    /// <summary>
    /// The amount (in wei) of the output token to be received by the receiver address.
    /// </summary>
    [JsonProperty("destinationAmount")]
    public string DestinationAmount { get; set; }

    /// <summary>
    /// The timestamp when the quote was generated.
    /// </summary>
    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }

    /// <summary>
    /// The block number when the quote was generated.
    /// </summary>
    [JsonProperty("blockNumber")]
    public string BlockNumber { get; set; }

    /// <summary>
    /// The estimated execution time in milliseconds for filling the quote.
    /// </summary>
    [JsonProperty("estimatedExecutionTimeMs")]
    public long EstimatedExecutionTimeMs { get; set; }

    /// <summary>
    /// The intent object containing details about the transaction.
    /// </summary>
    [JsonProperty("intent")]
    public Intent Intent { get; set; }

    [JsonProperty("steps")]
    public List<Step> Steps { get; set; }

    [JsonProperty("purchaseData", NullValueHandling = NullValueHandling.Ignore)]
    public object PurchaseData { get; set; }
}

/// <summary>
/// Represents a single step in a transaction, including origin and destination tokens.
/// </summary>
public class Step
{
    [JsonProperty("originToken")]
    public TokenData OriginToken { get; set; }

    [JsonProperty("destinationToken")]
    public TokenData DestinationToken { get; set; }

    [JsonProperty("transactions")]
    public List<Transaction> Transactions { get; set; }

    [JsonProperty("originAmount")]
    public string OriginAmount { get; set; }

    [JsonProperty("destinationAmount")]
    public string DestinationAmount { get; set; }

    [JsonProperty("nativeFee")]
    public string NativeFee { get; set; }

    [JsonProperty("estimatedExecutionTimeMs")]
    public long EstimatedExecutionTimeMs { get; set; }
}

/// <summary>
/// Represents a token in a step, including metadata like chain ID, address, and pricing.
/// </summary>
public class TokenData
{
    [JsonProperty("chainId")]
    public BigInteger ChainId { get; set; }

    [JsonProperty("address")]
    public string Address { get; set; }

    [JsonProperty("symbol")]
    public string Symbol { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("decimals")]
    public int Decimals { get; set; }

    [JsonProperty("priceUsd")]
    public decimal PriceUsd { get; set; }

    [JsonProperty("iconUri")]
    public string IconUri { get; set; }
}

/// <summary>
/// Represents a transaction ready to be executed.
/// </summary>
public class Transaction
{
    /// <summary>
    /// The transaction ID, each step in a quoted payment will have a unique transaction ID.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// The chain ID where the transaction will take place.
    /// </summary>
    [JsonProperty("chainId")]
    public BigInteger ChainId { get; set; }

    /// <summary>
    /// The maximum priority fee per gas (EIP-1559).
    /// </summary>
    [JsonProperty("maxPriorityFeePerGas", NullValueHandling = NullValueHandling.Ignore)]
    public string MaxPriorityFeePerGas { get; set; }

    /// <summary>
    /// The maximum fee per gas (EIP-1559).
    /// </summary>
    [JsonProperty("maxFeePerGas", NullValueHandling = NullValueHandling.Ignore)]
    public string MaxFeePerGas { get; set; }

    /// <summary>
    /// The address to which the transaction is sent.
    /// </summary>
    [JsonProperty("to")]
    public string To { get; set; }

    /// <summary>
    /// The address from which the transaction is sent, or null if not applicable.
    /// </summary>
    [JsonProperty("from", NullValueHandling = NullValueHandling.Ignore)]
    public string From { get; set; }

    /// <summary>
    /// The value (amount) to be sent in the transaction.
    /// </summary>
    [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
    public string Value { get; set; }

    /// <summary>
    /// The gas limit for the transaction.
    /// </summary>
    [JsonProperty("gas", NullValueHandling = NullValueHandling.Ignore)]
    public string Gas { get; set; }

    /// <summary>
    /// The transaction data.
    /// </summary>
    [JsonProperty("data")]
    public string Data { get; set; }

    /// <summary>
    /// The type of the transaction (e.g., "eip1559").
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; }

    /// <summary>
    /// The action type for the transaction (e.g., "approval", "transfer", "buy", "sell").
    /// </summary>
    [JsonProperty("action")]
    public string Action { get; set; }
}

#endregion

#region Buy

/// <summary>
/// Represents the data returned in the buy quote response.
/// </summary>
public class BuyQuoteData : QuoteData { }

/// <summary>
/// Represents the data returned in the buy prepare response.
/// </summary>
public class BuyPrepareData : QuoteData
{
    /// <summary>
    /// A hex ID associated with the quoted payment.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// An array of transactions to be executed to fulfill this quote (in order).
    /// </summary>
    [Obsolete("Use Steps.Transactions instead.")]
    [JsonProperty("transactions")]
    public List<Transaction> Transactions { get; set; }

    /// <summary>
    /// The expiration timestamp for this prepared quote and its transactions (if applicable).
    /// </summary>
    [JsonProperty("expiration")]
    public long? Expiration { get; set; }
}

#endregion

#region Sell

/// <summary>
/// Represents the data returned in the sell quote response.
/// </summary>
public class SellQuoteData : QuoteData { }

/// <summary>
/// Represents the data returned in the sell prepare response.
/// </summary>
public class SellPrepareData : QuoteData
{
    /// <summary>
    /// A hex ID associated with the quoted payment.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// An array of transactions to be executed to fulfill this quote (in order).
    /// </summary>
    [Obsolete("Use Steps.Transactions instead.")]
    [JsonProperty("transactions")]
    public List<Transaction> Transactions { get; set; }

    /// <summary>
    /// The expiration timestamp for this prepared quote and its transactions (if applicable).
    /// </summary>
    [JsonProperty("expiration")]
    public long? Expiration { get; set; }
}

#endregion

#region Transfer

/// <summary>
/// Represents the data returned in the transfer prepare response.
/// </summary>
public class TransferPrepareData
{
    [JsonProperty("originAmount")]
    public string OriginAmount { get; set; }

    [JsonProperty("destinationAmount")]
    public string DestinationAmount { get; set; }

    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }

    [JsonProperty("blockNumber")]
    public string BlockNumber { get; set; }

    [JsonProperty("estimatedExecutionTimeMs")]
    public long EstimatedExecutionTimeMs { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("transactions")]
    public List<Transaction> Transactions { get; set; }

    [JsonProperty("expiration")]
    public long? Expiration { get; set; }

    [JsonProperty("intent")]
    public TransferIntent Intent { get; set; }
}

/// <summary>
/// Represents the intent object for the transfer prepare response.
/// </summary>
public class TransferIntent
{
    [JsonProperty("chainId")]
    public BigInteger ChainId { get; set; }

    [JsonProperty("tokenAddress")]
    public string TokenAddress { get; set; }

    [JsonProperty("transferAmountWei")]
    public string TransferAmountWei { get; set; }

    [JsonProperty("sender")]
    public string Sender { get; set; }

    [JsonProperty("receiver")]
    public string Receiver { get; set; }

    [JsonProperty("feePayer")]
    public string FeePayer { get; set; } = "sender";

    [JsonProperty("purchaseData", NullValueHandling = NullValueHandling.Ignore)]
    public object PurchaseData { get; set; }
}

#endregion

#region Status

/// <summary>
/// Represents the possible statuses for a transaction.
/// </summary>
public enum StatusType
{
    FAILED,
    PENDING,
    COMPLETED,
    NOT_FOUND,
    PROCESSING,
    CREATED,
    UNKNOWN
}

/// <summary>
/// Represents the data returned in the status response.
/// </summary>
public class StatusData
{
    /// <summary>
    /// The status of the transaction (as StatusType enum).
    /// </summary>
    [JsonIgnore]
    public StatusType StatusType =>
        this.Status switch
        {
            "FAILED" => StatusType.FAILED,
            "PENDING" => StatusType.PENDING,
            "COMPLETED" => StatusType.COMPLETED,
            "NOT_FOUND" => StatusType.NOT_FOUND,
            _ => StatusType.UNKNOWN
        };

    /// <summary>
    /// The status of the transaction.
    /// </summary>
    [JsonProperty("status")]
    public string Status { get; set; }

    /// <summary>
    /// A list of transactions involved in this status.
    /// </summary>
    [JsonProperty("transactions")]
    public List<TransactionStatus> Transactions { get; set; }

    /// <summary>
    /// The unique payment ID for the transaction.
    /// </summary>
    [JsonProperty("paymentId", NullValueHandling = NullValueHandling.Ignore)]
    public string PaymentId { get; set; }

    /// <summary>
    /// The unique transaction ID for the transaction.
    /// </summary>
    [JsonProperty("transactionId", NullValueHandling = NullValueHandling.Ignore)]
    public string TransactionId { get; set; }

    /// <summary>
    /// The origin chain ID (for PENDING and COMPLETED statuses).
    /// </summary>
    [JsonProperty("originChainId", NullValueHandling = NullValueHandling.Ignore)]
    public BigInteger? OriginChainId { get; set; }

    /// <summary>
    /// The origin token address (for PENDING and COMPLETED statuses).
    /// </summary>
    [JsonProperty("originTokenAddress", NullValueHandling = NullValueHandling.Ignore)]
    public string OriginTokenAddress { get; set; }

    /// <summary>
    /// The destination chain ID (for PENDING and COMPLETED statuses).
    /// </summary>
    [JsonProperty("destinationChainId", NullValueHandling = NullValueHandling.Ignore)]
    public BigInteger? DestinationChainId { get; set; }

    /// <summary>
    /// The destination token address (for PENDING and COMPLETED statuses).
    /// </summary>
    [JsonProperty("destinationTokenAddress", NullValueHandling = NullValueHandling.Ignore)]
    public string DestinationTokenAddress { get; set; }

    /// <summary>
    /// The origin token amount in wei (for PENDING and COMPLETED statuses).
    /// </summary>
    [JsonProperty("originAmount", NullValueHandling = NullValueHandling.Ignore)]
    public string OriginAmount { get; set; }

    /// <summary>
    /// The destination token amount in wei (for COMPLETED status).
    /// </summary>
    [JsonProperty("destinationAmount", NullValueHandling = NullValueHandling.Ignore)]
    public string DestinationAmount { get; set; }

    /// <summary>
    /// The purchase data, which can be null.
    /// </summary>
    [JsonProperty("purchaseData", NullValueHandling = NullValueHandling.Ignore)]
    public object PurchaseData { get; set; }
}

/// <summary>
/// Represents the transaction details for a specific status.
/// </summary>
public class TransactionStatus
{
    /// <summary>
    /// The chain ID where the transaction took place.
    /// </summary>
    [JsonProperty("chainId")]
    public BigInteger ChainId { get; set; }

    /// <summary>
    /// The transaction hash of the transaction.
    /// </summary>
    [JsonProperty("transactionHash")]
    public string TransactionHash { get; set; }
}

#endregion

#region Onramp

public enum OnrampProvider
{
    Stripe,
    Coinbase,
    Transak
}

/// <summary>
/// Represents the core data of an onramp response.
/// </summary>
public class OnrampPrepareData
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("link")]
    public string Link { get; set; }

    [JsonProperty("currency")]
    public string Currency { get; set; }

    [JsonProperty("currencyAmount")]
    public decimal CurrencyAmount { get; set; }

    [JsonProperty("destinationAmount")]
    public string DestinationAmount { get; set; }

    [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
    public long? Timestamp { get; set; }

    [JsonProperty("expiration", NullValueHandling = NullValueHandling.Ignore)]
    public long? Expiration { get; set; }

    [JsonProperty("steps")]
    public List<Step> Steps { get; set; }

    [JsonProperty("intent")]
    public OnrampIntent Intent { get; set; }
}

/// <summary>
/// Represents the intent used to prepare the onramp.
/// </summary>
public class OnrampIntent
{
    [JsonProperty("onramp")]
    public OnrampProvider Onramp { get; set; }

    [JsonProperty("chainId")]
    public BigInteger ChainId { get; set; }

    [JsonProperty("tokenAddress")]
    public string TokenAddress { get; set; }

    [JsonProperty("amount")]
    public string Amount { get; set; }

    [JsonProperty("receiver")]
    public string Receiver { get; set; }

    [JsonProperty("purchaseData", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, object> PurchaseData { get; set; }

    [JsonProperty("onrampTokenAddress", NullValueHandling = NullValueHandling.Ignore)]
    public string OnrampTokenAddress { get; set; }

    [JsonProperty("onrampChainId", NullValueHandling = NullValueHandling.Ignore)]
    public BigInteger? OnrampChainId { get; set; }

    [JsonProperty("currency", NullValueHandling = NullValueHandling.Ignore)]
    public string Currency { get; set; } = "USD";

    [JsonProperty("maxSteps", NullValueHandling = NullValueHandling.Ignore)]
    public int? MaxSteps { get; set; } = 3;

    [JsonProperty("excludeChainIds", NullValueHandling = NullValueHandling.Ignore)]
    public List<BigInteger> ExcludeChainIds { get; set; }
}

/// <summary>
/// Represents the status of an onramp transaction.
/// </summary>
public class OnrampStatusData
{
    [JsonIgnore]
    public StatusType StatusType =>
        this.Status switch
        {
            "FAILED" => StatusType.FAILED,
            "PENDING" => StatusType.PENDING,
            "COMPLETED" => StatusType.COMPLETED,
            "PROCESSING" => StatusType.PROCESSING,
            "CREATED" => StatusType.CREATED,
            _ => StatusType.UNKNOWN
        };

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("transactionHash")]
    public string TransactionHash { get; set; }
}

#endregion
