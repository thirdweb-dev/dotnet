using Newtonsoft.Json;

namespace Thirdweb.Pay
{
    /// <summary>
    /// Represents the response for a swap status.
    /// </summary>
    public class SwapStatusResponse
    {
        /// <summary>
        /// Gets or sets the result of the swap status.
        /// </summary>
        [JsonProperty("result")]
        public BuyWithCryptoStatusResult Result { get; set; }
    }

    /// <summary>
    /// Represents the status result of buying with cryptocurrency.
    /// </summary>
    public class BuyWithCryptoStatusResult
    {
        /// <summary>
        /// Gets or sets the swap quote details.
        /// </summary>
        [JsonProperty("quote")]
        public SwapQuote Quote { get; set; }

        /// <summary>
        /// Gets or sets the type of swap.
        /// </summary>
        [JsonProperty("swapType")]
        public string SwapType { get; set; }

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
        /// Gets or sets the status of the swap.
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the sub-status of the swap.
        /// </summary>
        [JsonProperty("subStatus")]
        public string SubStatus { get; set; }

        /// <summary>
        /// Gets or sets the address from which the swap is initiated.
        /// </summary>
        [JsonProperty("fromAddress")]
        public string FromAddress { get; set; }

        /// <summary>
        /// Gets or sets the recipient address.
        /// </summary>
        [JsonProperty("toAddress")]
        public string ToAddress { get; set; }

        /// <summary>
        /// Gets or sets the failure message if the swap fails.
        /// </summary>
        [JsonProperty("failureMessage")]
        public string FailureMessage { get; set; }

        /// <summary>
        /// Gets or sets the bridge details.
        /// </summary>
        [JsonProperty("bridge")]
        public string Bridge { get; set; }
    }

    /// <summary>
    /// Represents the transaction details.
    /// </summary>
    public class TransactionDetails
    {
        /// <summary>
        /// Gets or sets the transaction hash.
        /// </summary>
        [JsonProperty("transactionHash")]
        public string TransactionHash { get; set; }

        /// <summary>
        /// Gets or sets the token details.
        /// </summary>
        [JsonProperty("token")]
        public Token Token { get; set; }

        /// <summary>
        /// Gets or sets the amount of the token.
        /// </summary>
        [JsonProperty("amount")]
        public string Amount { get; set; }

        /// <summary>
        /// Gets or sets the amount of the token in wei.
        /// </summary>
        [JsonProperty("amountWei")]
        public string AmountWei { get; set; }

        /// <summary>
        /// Gets or sets the amount in USD cents.
        /// </summary>
        [JsonProperty("amountUSDCents")]
        public double AmountUSDCents { get; set; }

        /// <summary>
        /// Gets or sets the completion date of the transaction.
        /// </summary>
        [JsonProperty("completedAt")]
        public DateTime CompletedAt { get; set; }

        /// <summary>
        /// Gets or sets the explorer link for the transaction.
        /// </summary>
        [JsonProperty("explorerLink")]
        public string ExplorerLink { get; set; }
    }

    /// <summary>
    /// Represents a swap quote.
    /// </summary>
    public class SwapQuote
    {
        /// <summary>
        /// Gets or sets the source token details.
        /// </summary>
        [JsonProperty("fromToken")]
        public Token FromToken { get; set; }

        /// <summary>
        /// Gets or sets the destination token details.
        /// </summary>
        [JsonProperty("toToken")]
        public Token ToToken { get; set; }

        /// <summary>
        /// Gets or sets the amount of the source token in wei.
        /// </summary>
        [JsonProperty("fromAmountWei")]
        public string FromAmountWei { get; set; }

        /// <summary>
        /// Gets or sets the amount of the source token.
        /// </summary>
        [JsonProperty("fromAmount")]
        public string FromAmount { get; set; }

        /// <summary>
        /// Gets or sets the amount of the destination token in wei.
        /// </summary>
        [JsonProperty("toAmountWei")]
        public string ToAmountWei { get; set; }

        /// <summary>
        /// Gets or sets the amount of the destination token.
        /// </summary>
        [JsonProperty("toAmount")]
        public string ToAmount { get; set; }

        /// <summary>
        /// Gets or sets the minimum amount of the destination token.
        /// </summary>
        [JsonProperty("toAmountMin")]
        public string ToAmountMin { get; set; }

        /// <summary>
        /// Gets or sets the minimum amount of the destination token in wei.
        /// </summary>
        [JsonProperty("toAmountMinWei")]
        public string ToAmountMinWei { get; set; }

        /// <summary>
        /// Gets or sets the estimated details.
        /// </summary>
        [JsonProperty("estimated")]
        public Estimated Estimated { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the swap quote.
        /// </summary>
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Represents the swap status.
    /// </summary>
    public enum SwapStatus
    {
        /// <summary>
        /// Status when the swap is not found.
        /// </summary>
        NOT_FOUND,

        /// <summary>
        /// Status when there is no swap.
        /// </summary>
        NONE,

        /// <summary>
        /// Status when the swap is pending.
        /// </summary>
        PENDING,

        /// <summary>
        /// Status when the swap has failed.
        /// </summary>
        FAILED,

        /// <summary>
        /// Status when the swap is completed.
        /// </summary>
        COMPLETED
    }

    /// <summary>
    /// Represents the swap sub-status.
    /// </summary>
    public enum SwapSubStatus
    {
        /// <summary>
        /// Sub-status when there is no specific sub-status.
        /// </summary>
        NONE,

        /// <summary>
        /// Sub-status when waiting for the bridge.
        /// </summary>
        WAITING_BRIDGE,

        /// <summary>
        /// Sub-status when the swap is reverted on chain.
        /// </summary>
        REVERTED_ON_CHAIN,

        /// <summary>
        /// Sub-status when the swap is successful.
        /// </summary>
        SUCCESS,

        /// <summary>
        /// Sub-status when the swap is partially successful.
        /// </summary>
        PARTIAL_SUCCESS,

        /// <summary>
        /// Sub-status when there is an unknown error.
        /// </summary>
        UNKNOWN_ERROR
    }
}
