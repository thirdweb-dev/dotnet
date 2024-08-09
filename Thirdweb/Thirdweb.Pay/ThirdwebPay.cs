namespace Thirdweb.Pay
{
    public partial class ThirdwebPay
    {
        private const string THIRDWEB_PAY_BASE_URL = "https://pay.thirdweb.com";

        private const string THIRDWEB_PAY_CRYPTO_QUOTE_ENDPOINT = THIRDWEB_PAY_BASE_URL + "/buy-with-crypto/quote/v1";
        private const string THIRDWEB_PAY_CRYPTO_STATUS_ENDPOINT = THIRDWEB_PAY_BASE_URL + "/buy-with-crypto/status/v1";

        private const string THIRDWEB_PAY_FIAT_QUOTE_ENDPOINT = THIRDWEB_PAY_BASE_URL + "/buy-with-fiat/quote/v1";
        private const string THIRDWEB_PAY_FIAT_STATUS_ENDPOINT = THIRDWEB_PAY_BASE_URL + "/buy-with-fiat/status/v1";

        private const string THIRDWEB_PAY_FIAT_CURRENCIES_ENDPOINT = THIRDWEB_PAY_BASE_URL + "/buy-with-fiat/currency/v1";

        private const string THIRDWEB_PAY_HISTORY_ENDPOINT = THIRDWEB_PAY_BASE_URL + "/wallet/history/v1";
    }
}
