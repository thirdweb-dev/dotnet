using Newtonsoft.Json;

namespace Thirdweb.Pay
{
    public partial class ThirdwebPay
    {
        public static async Task<List<string>> GetBuyWithFiatCurrencies(ThirdwebClient client)
        {
            var url = $"{Constants.THIRDWEB_PAY_FIAT_CURRENCIES_ENDPOINT}";

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

            var data = JsonConvert.DeserializeObject<FiatCurrenciesResponse>(content);
            return data.Result.FiatCurrencies;
        }
    }
}
