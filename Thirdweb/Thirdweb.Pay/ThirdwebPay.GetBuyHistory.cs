using Newtonsoft.Json;

namespace Thirdweb.Pay;

/// <summary>
/// Provides methods for processing payments with cryptocurrency and fiat.
/// </summary>
public partial class ThirdwebPay
{
    /// <summary>
    /// Retrieves the buy history for a specified wallet address.
    /// </summary>
    /// <param name="client">The Thirdweb client.</param>
    /// <param name="walletAddress">The wallet address to retrieve history for.</param>
    /// <param name="start">The start index for the history.</param>
    /// <param name="count">The number of history records to retrieve.</param>
    /// <param name="cursor">The cursor for pagination (optional).</param>
    /// <param name="pageSize">The page size for pagination (optional).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the buy history result.</returns>
    /// <exception cref="Exception">Thrown if the HTTP response is not successful.</exception>
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
        var url = $"{THIRDWEB_PAY_HISTORY_ENDPOINT}?{queryStringFormatted}";

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
