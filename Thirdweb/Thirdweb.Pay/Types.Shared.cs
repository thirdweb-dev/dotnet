using System.Numerics;
using Newtonsoft.Json;

namespace Thirdweb.Pay
{
    /// <summary>
    /// Represents the error response.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Gets or sets the error details.
        /// </summary>
        [JsonProperty("error")]
        public ErrorDetails Error { get; set; }
    }

    /// <summary>
    /// Represents the details of an error.
    /// </summary>
    public class ErrorDetails
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the reason for the error.
        /// </summary>
        [JsonProperty("reason")]
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the error stack trace.
        /// </summary>
        [JsonProperty("stack")]
        public string Stack { get; set; }

        /// <summary>
        /// Gets or sets the status code of the error.
        /// </summary>
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }
    }

    /// <summary>
    /// Represents a token.
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Gets or sets the chain ID of the token.
        /// </summary>
        [JsonProperty("chainId")]
        public BigInteger ChainId { get; set; }

        /// <summary>
        /// Gets or sets the address of the token.
        /// </summary>
        [JsonProperty("tokenAddress")]
        public string TokenAddress { get; set; }

        /// <summary>
        /// Gets or sets the number of decimals of the token.
        /// </summary>
        [JsonProperty("decimals")]
        public int Decimals { get; set; }

        /// <summary>
        /// Gets or sets the price of the token in USD cents.
        /// </summary>
        [JsonProperty("priceUSDCents")]
        public int PriceUSDCents { get; set; }

        /// <summary>
        /// Gets or sets the name of the token.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the symbol of the token.
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }
    }

    /// <summary>
    /// Represents the estimated details for a transaction.
    /// </summary>
    public class Estimated
    {
        /// <summary>
        /// Gets or sets the amount in USD cents for the source token.
        /// </summary>
        [JsonProperty("fromAmountUSDCents")]
        public double FromAmountUSDCents { get; set; }

        /// <summary>
        /// Gets or sets the minimum amount in USD cents for the destination token.
        /// </summary>
        [JsonProperty("toAmountMinUSDCents")]
        public double ToAmountMinUSDCents { get; set; }

        /// <summary>
        /// Gets or sets the amount in USD cents for the destination token.
        /// </summary>
        [JsonProperty("toAmountUSDCents")]
        public double ToAmountUSDCents { get; set; }

        /// <summary>
        /// Gets or sets the slippage in basis points.
        /// </summary>
        [JsonProperty("slippageBPS")]
        public int SlippageBPS { get; set; }

        /// <summary>
        /// Gets or sets the fees in USD cents.
        /// </summary>
        [JsonProperty("feesUSDCents")]
        public double FeesUSDCents { get; set; }

        /// <summary>
        /// Gets or sets the gas cost in USD cents.
        /// </summary>
        [JsonProperty("gasCostUSDCents")]
        public double GasCostUSDCents { get; set; }

        /// <summary>
        /// Gets or sets the duration of the transaction in seconds.
        /// </summary>
        [JsonProperty("durationSeconds")]
        public int DurationSeconds { get; set; }
    }

    /// <summary>
    /// Represents the currency details for an on-ramp transaction.
    /// </summary>
    public class OnRampCurrency
    {
        /// <summary>
        /// Gets or sets the amount of the currency.
        /// </summary>
        [JsonProperty("amount")]
        public string Amount { get; set; }

        /// <summary>
        /// Gets or sets the units of the currency amount.
        /// </summary>
        [JsonProperty("amountUnits")]
        public string AmountUnits { get; set; }

        /// <summary>
        /// Gets or sets the number of decimals for the currency.
        /// </summary>
        [JsonProperty("decimals")]
        public int Decimals { get; set; }

        /// <summary>
        /// Gets or sets the symbol of the currency.
        /// </summary>
        [JsonProperty("currencySymbol")]
        public string CurrencySymbol { get; set; }
    }

    /// <summary>
    /// Represents the different types of swaps.
    /// </summary>
    public enum SwapType
    {
        /// <summary>
        /// Swap on the same chain.
        /// </summary>
        SAME_CHAIN,

        /// <summary>
        /// Swap across different chains.
        /// </summary>
        CROSS_CHAIN,

        /// <summary>
        /// On-ramp swap.
        /// </summary>
        ON_RAMP
    }
}
