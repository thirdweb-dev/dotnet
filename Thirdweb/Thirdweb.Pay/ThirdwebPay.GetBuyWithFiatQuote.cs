using Newtonsoft.Json;

namespace Thirdweb.Pay;

/// <summary>
/// Provides methods for processing payments with cryptocurrency and fiat.
/// </summary>
public partial class ThirdwebPay
{
    /// <summary>
    /// Retrieves a quote for buying with fiat using the provided parameters.
    /// </summary>
    /// <param name="client">The Thirdweb client.</param>
    /// <param name="buyWithFiatParams">The parameters for the fiat purchase.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the quote result.</returns>
    /// <exception cref="Exception">Thrown if the HTTP response is not successful.</exception>
    public static async Task<BuyWithFiatQuoteResult> GetBuyWithFiatQuote(ThirdwebClient client, BuyWithFiatQuoteParams buyWithFiatParams)
    {
        var queryString = new Dictionary<string, string>
        {
            { "fromCurrencySymbol", buyWithFiatParams.FromCurrencySymbol },
            { "fromAmount", buyWithFiatParams.FromAmount },
            { "fromAmountUnits", buyWithFiatParams.FromAmountUnits },
            { "toAddress", buyWithFiatParams.ToAddress },
            { "toChainId", buyWithFiatParams.ToChainId },
            { "toTokenAddress", buyWithFiatParams.ToTokenAddress },
            { "toAmount", buyWithFiatParams.ToAmount },
            { "toAmountWei", buyWithFiatParams.ToAmountWei },
            { "preferredProvider", buyWithFiatParams.PreferredProvider },
            { "maxSlippageBPS", buyWithFiatParams.MaxSlippageBPS?.ToString() }
        };

        var queryStringFormatted = string.Join("&", queryString.Where(kv => kv.Value != null).Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
        var url = $"{THIRDWEB_PAY_FIAT_QUOTE_ENDPOINT}?{queryStringFormatted}";
        url += buyWithFiatParams.IsTestMode ? "&isTestMode=true" : "&isTestMode=false";

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

        var data = JsonConvert.DeserializeObject<GetFiatQuoteResponse>(content);
        return data.Result;
    }
}
