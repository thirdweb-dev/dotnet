using Newtonsoft.Json;

namespace Thirdweb.Pay
{
    /// <summary>
    /// Represents the response for buy history.
    /// </summary>
    public class BuyHistoryResponse
    {
        /// <summary>
        /// Gets or sets the result of the buy history.
        /// </summary>
        [JsonProperty("result")]
        public BuyHistoryResult Result { get; set; }
    }

    /// <summary>
    /// Represents the result of the buy history.
    /// </summary>
    public class BuyHistoryResult
    {
        /// <summary>
        /// Gets or sets the wallet address.
        /// </summary>
        [JsonProperty("walletAddress")]
        public string WalletAddress { get; set; }

        /// <summary>
        /// Gets or sets the list of history pages.
        /// </summary>
        [JsonProperty("page")]
        public List<HistoryPage> Page { get; set; }

        /// <summary>
        /// Gets or sets the next cursor for pagination.
        /// </summary>
        [JsonProperty("nextCursor")]
        public string NextCursor { get; set; }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        [JsonProperty("pageSize")]
        public int PageSize { get; set; }
    }

    /// <summary>
    /// Represents a page in the buy history.
    /// </summary>
    public class HistoryPage
    {
        /// <summary>
        /// Gets or sets the status of the buy with crypto transaction.
        /// </summary>
        [JsonProperty("buyWithCryptoStatus")]
        public BuyWithCryptoStatusResult BuyWithCryptoStatus;

        /// <summary>
        /// Gets or sets the status of the buy with fiat transaction.
        /// </summary>
        [JsonProperty("buyWithFiatStatus")]
        public BuyWithFiatStatusResult BuyWithFiatStatus;
    }
}
