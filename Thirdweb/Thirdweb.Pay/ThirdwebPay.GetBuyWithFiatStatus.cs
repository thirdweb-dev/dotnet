using Newtonsoft.Json;

namespace Thirdweb.Pay
{
    /// <summary>
    /// Provides methods for processing payments with cryptocurrency and fiat.
    /// </summary>
    public partial class ThirdwebPay
    {
        /// <summary>
        /// Retrieves the status of a fiat purchase using the intent ID.
        /// </summary>
        /// <param name="client">The Thirdweb client.</param>
        /// <param name="intentId">The intent ID to check the status of.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the status result.</returns>
        /// <exception cref="ArgumentException">Thrown if the intent ID is null or empty.</exception>
        /// <exception cref="Exception">Thrown if the HTTP response is not successful.</exception>
        public static async Task<BuyWithFiatStatusResult> GetBuyWithFiatStatus(ThirdwebClient client, string intentId)
        {
            if (string.IsNullOrEmpty(intentId))
            {
                throw new ArgumentException(nameof(intentId), "Intent ID cannot be null or empty.");
            }

            var queryString = new Dictionary<string, string> { { "intentId", intentId } };

            var queryStringFormatted = string.Join("&", queryString.Where(kv => kv.Value != null).Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            var url = $"{Constants.THIRDWEB_PAY_FIAT_STATUS_ENDPOINT}?{queryStringFormatted}";

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

            var data = JsonConvert.DeserializeObject<OnRampStatusResponse>(content);
            return data.Result;
        }
    }
}
