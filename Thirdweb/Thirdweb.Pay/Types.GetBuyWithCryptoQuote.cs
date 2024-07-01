using System.Numerics;
using Newtonsoft.Json;

namespace Thirdweb.Pay
{
    /// <summary>
    /// Parameters for getting a quote for buying with cryptocurrency.
    /// </summary>
    public class BuyWithCryptoQuoteParams
    {
        /// <summary>
        /// The address from which the payment is made.
        /// </summary>
        [JsonProperty("fromAddress")]
        public string FromAddress { get; set; }

        /// <summary>
        /// The chain ID of the source token.
        /// </summary>
        [JsonProperty("fromChainId")]
        public BigInteger? FromChainId { get; set; }

        /// <summary>
        /// The address of the source token.
        /// </summary>
        [JsonProperty("fromTokenAddress")]
        public string FromTokenAddress { get; set; }

        /// <summary>
        /// The amount of the source token.
        /// </summary>
        [JsonProperty("fromAmount")]
        public string FromAmount { get; set; }

        /// <summary>
        /// The amount of the source token in wei.
        /// </summary>
        [JsonProperty("fromAmountWei")]
        public string FromAmountWei { get; set; }

        /// <summary>
        /// The chain ID of the destination token.
        /// </summary>
        [JsonProperty("toChainId")]
        public BigInteger? ToChainId { get; set; }

        /// <summary>
        /// The address of the destination token.
        /// </summary>
        [JsonProperty("toTokenAddress")]
        public string ToTokenAddress { get; set; }

        /// <summary>
        /// The amount of the destination token.
        /// </summary>
        [JsonProperty("toAmount")]
        public string ToAmount { get; set; }

        /// <summary>
        /// The amount of the destination token in wei.
        /// </summary>
        [JsonProperty("toAmountWei")]
        public string ToAmountWei { get; set; }

        /// <summary>
        /// The address of the recipient.
        /// </summary>
        [JsonProperty("toAddress")]
        public string ToAddress { get; set; }

        /// <summary>
        /// The maximum slippage in basis points.
        /// </summary>
        [JsonProperty("maxSlippageBPS")]
        public double? MaxSlippageBPS { get; set; }

        /// <summary>
        /// The intent ID for the transaction.
        /// </summary>
        [JsonProperty("intentId")]
        public string IntentId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyWithCryptoQuoteParams"/> class.
        /// </summary>
        public BuyWithCryptoQuoteParams(
            string fromAddress,
            BigInteger? fromChainId,
            string fromTokenAddress,
            string toTokenAddress,
            string fromAmount = null,
            string fromAmountWei = null,
            BigInteger? toChainId = null,
            string toAmount = null,
            string toAmountWei = null,
            string toAddress = null,
            double? maxSlippageBPS = null,
            string intentId = null
        )
        {
            FromAddress = fromAddress;
            FromChainId = fromChainId;
            FromTokenAddress = fromTokenAddress;
            FromAmount = fromAmount;
            FromAmountWei = fromAmountWei;
            ToChainId = toChainId;
            ToTokenAddress = toTokenAddress;
            ToAmount = toAmount;
            ToAmountWei = toAmountWei;
            ToAddress = toAddress;
            MaxSlippageBPS = maxSlippageBPS;
            IntentId = intentId;
        }
    }

    /// <summary>
    /// Represents a transaction request.
    /// </summary>
    public class TransactionRequest
    {
        /// <summary>
        /// Gets or sets the data of the transaction.
        /// </summary>
        [JsonProperty("data")]
        public string Data { get; set; }

        /// <summary>
        /// Gets or sets the recipient address of the transaction.
        /// </summary>
        [JsonProperty("to")]
        public string To { get; set; }

        /// <summary>
        /// Gets or sets the value of the transaction.
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the sender address of the transaction.
        /// </summary>
        [JsonProperty("from")]
        public string From { get; set; }

        /// <summary>
        /// Gets or sets the chain ID of the transaction.
        /// </summary>
        [JsonProperty("chainId")]
        public BigInteger ChainId { get; set; }

        /// <summary>
        /// Gets or sets the gas price of the transaction.
        /// </summary>
        [JsonProperty("gasPrice")]
        public string GasPrice { get; set; }

        /// <summary>
        /// Gets or sets the gas limit of the transaction.
        /// </summary>
        [JsonProperty("gasLimit")]
        public string GasLimit { get; set; }
    }

    /// <summary>
    /// Represents an approval request.
    /// </summary>
    public class Approval
    {
        /// <summary>
        /// Gets or sets the chain ID of the approval request.
        /// </summary>
        [JsonProperty("chainId")]
        public BigInteger ChainId { get; set; }

        /// <summary>
        /// Gets or sets the token address for the approval request.
        /// </summary>
        [JsonProperty("tokenAddress")]
        public string TokenAddress { get; set; }

        /// <summary>
        /// Gets or sets the spender address for the approval request.
        /// </summary>
        [JsonProperty("spenderAddress")]
        public string SpenderAddress { get; set; }

        /// <summary>
        /// Gets or sets the amount in wei for the approval request.
        /// </summary>
        [JsonProperty("amountWei")]
        public string AmountWei { get; set; }
    }

    /// <summary>
    /// Represents a payment token.
    /// </summary>
    public class PaymentToken
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
    /// Represents a processing fee.
    /// </summary>
    public class ProcessingFee
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
    /// Represents the result of a quote for buying with cryptocurrency.
    /// </summary>
    public class BuyWithCryptoQuoteResult
    {
        /// <summary>
        /// Gets or sets the quote ID.
        /// </summary>
        [JsonProperty("quoteId")]
        public string QuoteId { get; set; }

        /// <summary>
        /// Gets or sets the transaction request.
        /// </summary>
        [JsonProperty("transactionRequest")]
        public TransactionRequest TransactionRequest { get; set; }

        /// <summary>
        /// Gets or sets the approval details.
        /// </summary>
        [JsonProperty("approval")]
        public Approval Approval { get; set; }

        /// <summary>
        /// Gets or sets the address from which the payment is made.
        /// </summary>
        [JsonProperty("fromAddress")]
        public string FromAddress { get; set; }

        /// <summary>
        /// Gets or sets the recipient address.
        /// </summary>
        [JsonProperty("toAddress")]
        public string ToAddress { get; set; }

        /// <summary>
        /// Gets or sets the details of the source token.
        /// </summary>
        [JsonProperty("fromToken")]
        public Token FromToken { get; set; }

        /// <summary>
        /// Gets or sets the details of the destination token.
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
        /// Gets or sets the minimum amount of the destination token in wei.
        /// </summary>
        [JsonProperty("toAmountMinWei")]
        public string ToAmountMinWei { get; set; }

        /// <summary>
        /// Gets or sets the minimum amount of the destination token.
        /// </summary>
        [JsonProperty("toAmountMin")]
        public string ToAmountMin { get; set; }

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
        /// Gets or sets the list of payment tokens.
        /// </summary>
        [JsonProperty("paymentTokens")]
        public List<PaymentToken> PaymentTokens { get; set; }

        /// <summary>
        /// Gets or sets the list of processing fees.
        /// </summary>
        [JsonProperty("processingFees")]
        public List<ProcessingFee> ProcessingFees { get; set; }

        /// <summary>
        /// Gets or sets the estimated details.
        /// </summary>
        [JsonProperty("estimated")]
        public Estimated Estimated { get; set; }

        /// <summary>
        /// Gets or sets the maximum slippage in basis points.
        /// </summary>
        [JsonProperty("maxSlippageBPS")]
        public double MaxSlippageBPS { get; set; }

        /// <summary>
        /// Gets or sets the bridge details.
        /// </summary>
        [JsonProperty("bridge")]
        public string Bridge { get; set; }
    }

    /// <summary>
    /// Represents the response for getting a swap quote.
    /// </summary>
    public class GetSwapQuoteResponse
    {
        /// <summary>
        /// Gets or sets the result of the swap quote.
        /// </summary>
        [JsonProperty("result")]
        public BuyWithCryptoQuoteResult Result { get; set; }
    }
}
