using Newtonsoft.Json;

namespace Thirdweb.Pay
{
    /// <summary>
    /// Provides methods for processing payments with cryptocurrency and fiat.
    /// </summary>
    public partial class ThirdwebPay
    {
        /// <summary>
        /// Retrieves a quote for buying with cryptocurrency using the provided parameters.
        /// </summary>
        /// <param name="client">The Thirdweb client.</param>
        /// <param name="buyWithCryptoParams">The parameters for the crypto purchase.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the quote result.</returns>
        /// <exception cref="Exception">Thrown if the HTTP response is not successful.</exception>
        public static async Task<BuyWithCryptoQuoteResult> GetBuyWithCryptoQuote(ThirdwebClient client, BuyWithCryptoQuoteParams buyWithCryptoParams)
        {
            var queryString = new Dictionary<string, string>
            {
                { "fromAddress", buyWithCryptoParams.FromAddress },
                { "fromChainId", buyWithCryptoParams.FromChainId?.ToString() },
                { "fromTokenAddress", buyWithCryptoParams.FromTokenAddress },
                { "fromAmount", buyWithCryptoParams.FromAmount },
                { "fromAmountWei", buyWithCryptoParams.FromAmountWei },
                { "toChainId", buyWithCryptoParams.ToChainId?.ToString() },
                { "toTokenAddress", buyWithCryptoParams.ToTokenAddress },
                { "toAmount", buyWithCryptoParams.ToAmount },
                { "toAmountWei", buyWithCryptoParams.ToAmountWei },
                { "maxSlippageBPS", buyWithCryptoParams.MaxSlippageBPS?.ToString() },
                { "intentId", buyWithCryptoParams.IntentId }
            };

            var queryStringFormatted = string.Join("&", queryString.Where(kv => kv.Value != null).Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            var url = $"{Constants.THIRDWEB_PAY_CRYPTO_QUOTE_ENDPOINT}?{queryStringFormatted}";

            var getResponse = await client.HttpClient.GetAsync(url);

            var content = await getResponse.Content.ReadAsStringAsync();

            if (!getResponse.IsSuccessStatusCode)
            {
                ErrorResponse error;
                try
                {
                    error = JsonConvert.DeserializeObject<ErrorResponse>(content);
                }
                catch
                {
                    error = new ErrorResponse
                    {
                        Error = new ErrorDetails
                        {
                            Message = "Unknown error",
                            Reason = "Unknown",
                            Code = "Unknown",
                            Stack = "Unknown",
                            StatusCode = (int)getResponse.StatusCode
                        }
                    };
                }

                throw new Exception(
                    $"HTTP error! Code: {error.Error.Code} Message: {error.Error.Message} Reason: {error.Error.Reason} StatusCode: {error.Error.StatusCode} Stack: {error.Error.Stack}"
                );
            }

            var data = JsonConvert.DeserializeObject<GetSwapQuoteResponse>(content);
            return data.Result;
        }
    }
}
