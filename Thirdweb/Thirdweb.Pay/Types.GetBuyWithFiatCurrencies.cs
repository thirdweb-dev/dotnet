using Newtonsoft.Json;

namespace Thirdweb.Pay
{
    /// <summary>
    /// Represents the response for fiat currencies.
    /// </summary>
    public class FiatCurrenciesResponse
    {
        /// <summary>
        /// Gets or sets the result of the fiat currencies response.
        /// </summary>
        [JsonProperty("result")]
        public FiatCurrenciesResult Result { get; set; }
    }

    /// <summary>
    /// Represents the result containing the list of fiat currencies.
    /// </summary>
    public class FiatCurrenciesResult
    {
        /// <summary>
        /// Gets or sets the list of fiat currencies.
        /// </summary>
        [JsonProperty("fiatCurrencies")]
        public List<string> FiatCurrencies { get; set; }
    }
}
