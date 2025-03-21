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
    /// The chain ID where the transaction originates.
    /// </summary>
    [JsonProperty("originChainId")]
    public BigInteger OriginChainId { get; set; }

    /// <summary>
    /// The token address in the origin chain.
    /// </summary>
    [JsonProperty("originTokenAddress")]
    public string OriginTokenAddress { get; set; }

    /// <summary>
    /// The chain ID where the transaction is executed.
    /// </summary>
    [JsonProperty("destinationChainId")]
    public BigInteger DestinationChainId { get; set; }

    /// <summary>
    /// The token address in the destination chain.
    /// </summary>
    [JsonProperty("destinationTokenAddress")]
    public string DestinationTokenAddress { get; set; }

    /// <summary>
    /// The amount involved in the transaction (buy, sell, or transfer) in wei.
    /// </summary>
    public virtual string AmountWei { get; set; }
}

/// <summary>
/// Represents the common fields for both Buy and Sell transactions.
/// </summary>
public class QuoteData<TIntent>
    where TIntent : Intent
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
    public TIntent Intent { get; set; }
}

/// <summary>
/// Represents a transaction to be executed.
/// </summary>
public class Transaction
{
    /// <summary>
    /// The chain ID where the transaction will take place.
    /// </summary>
    [JsonProperty("chainId")]
    public BigInteger ChainId { get; set; }

    /// <summary>
    /// The address to which the transaction is sent, or null if not applicable.
    /// </summary>
    [JsonProperty("to", NullValueHandling = NullValueHandling.Ignore)]
    public string To { get; set; }

    /// <summary>
    /// The value (amount) to be sent in the transaction.
    /// </summary>
    [JsonProperty("value")]
    public string Value { get; set; }

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
}

#endregion

#region Buy

/// <summary>
/// Represents the data returned in the buy quote response.
/// </summary>
public class BuyQuoteData : QuoteData<BuyIntent> { }

/// <summary>
/// Represents the data returned in the buy prepare response.
/// </summary>
public class BuyPrepareData : QuoteData<BuyIntent>
{
    /// <summary>
    /// An array of transactions to be executed to fulfill this quote (in order).
    /// </summary>
    [JsonProperty("transactions")]
    public List<Transaction> Transactions { get; set; }

    /// <summary>
    /// The expiration timestamp for this prepared quote and its transactions (if applicable).
    /// </summary>
    [JsonProperty("expiration")]
    public long? Expiration { get; set; }
}

/// <summary>
/// Represents the intent object for a buy quote.
/// </summary>
public class BuyIntent : Intent
{
    /// <summary>
    /// The desired output amount in wei for buying.
    /// </summary>
    [JsonProperty("buyAmountWei")]
    public override string AmountWei { get; set; }
}

#endregion

#region Sell

/// <summary>
/// Represents the data returned in the sell quote response.
/// </summary>
public class SellQuoteData : QuoteData<SellIntent> { }

/// <summary>
/// Represents the data returned in the sell prepare response.
/// </summary>
public class SellPrepareData : QuoteData<SellIntent>
{
    /// <summary>
    /// An array of transactions to be executed to fulfill this quote (in order).
    /// </summary>
    [JsonProperty("transactions")]
    public List<Transaction> Transactions { get; set; }

    /// <summary>
    /// The expiration timestamp for this prepared quote and its transactions (if applicable).
    /// </summary>
    [JsonProperty("expiration")]
    public long? Expiration { get; set; }
}

/// <summary>
/// Represents the intent object for a sell quote.
/// </summary>
public class SellIntent : Intent
{
    /// <summary>
    /// The amount to sell in wei.
    /// </summary>
    [JsonProperty("sellAmountWei")]
    public override string AmountWei { get; set; }
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
    public int ChainId { get; set; }

    [JsonProperty("tokenAddress")]
    public string TokenAddress { get; set; }

    [JsonProperty("transferAmountWei")]
    public string TransferAmountWei { get; set; }

    [JsonProperty("sender")]
    public string Sender { get; set; }

    [JsonProperty("receiver")]
    public string Receiver { get; set; }
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
    NOT_FOUND
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
            _ => throw new InvalidOperationException($"Unknown status: {this.Status}")
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
