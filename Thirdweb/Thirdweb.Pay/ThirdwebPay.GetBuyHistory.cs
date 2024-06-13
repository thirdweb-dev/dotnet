using Newtonsoft.Json;

namespace Thirdweb.Pay
{
    public partial class ThirdwebPay
    {
        public static async Task<BuyHistoryResult> GetBuyHistory(ThirdwebClient client, string walletAddress, int start, int count, string cursor = null, int? pageSize = null)
        {
            var queryString = new Dictionary<string, string>
            {
                { "walletAddress", walletAddress },
                { "start", start.ToString() },
                { "count", count.ToString() },
                { "cursor", cursor },
                { "pageSize", pageSize?.ToString() }
            };

            var queryStringFormatted = string.Join("&", queryString.Where(kv => kv.Value != null).Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            var url = $"{Constants.THIRDWEB_PAY_HISTORY_ENDPOINT}?{queryStringFormatted}";

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

            var data = JsonConvert.DeserializeObject<BuyHistoryResponse>(content);
            return data.Result;
        }
    }
}
