using Newtonsoft.Json;

namespace Thirdweb.Pay
{
    /// <summary>
    /// Provides methods for processing payments with cryptocurrency and fiat.
    /// </summary>
    public partial class ThirdwebPay
    {
        /// <summary>
        /// Retrieves the status of a cryptocurrency purchase using the transaction hash.
        /// </summary>
        /// <param name="client">The Thirdweb client.</param>
        /// <param name="transactionHash">The transaction hash to check the status of.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the status result.</returns>
        /// <exception cref="ArgumentException">Thrown if the transaction hash is null or empty.</exception>
        /// <exception cref="Exception">Thrown if the HTTP response is not successful.</exception>
        public static async Task<BuyWithCryptoStatusResult> GetBuyWithCryptoStatus(ThirdwebClient client, string transactionHash)
        {
            if (string.IsNullOrEmpty(transactionHash))
            {
                throw new ArgumentException(nameof(transactionHash), "Transaction hash cannot be null or empty.");
            }

            var queryString = new Dictionary<string, string> { { "transactionHash", transactionHash } };

            var queryStringFormatted = string.Join("&", queryString.Where(kv => kv.Value != null).Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            var url = $"{THIRDWEB_PAY_CRYPTO_STATUS_ENDPOINT}?{queryStringFormatted}";

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

            var data = JsonConvert.DeserializeObject<SwapStatusResponse>(content);
            return data.Result;
        }
    }
}
