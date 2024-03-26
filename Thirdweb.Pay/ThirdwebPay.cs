namespace Thirdweb.Pay
{
    public class ThirdwebPay
    {
        public ThirdwebPayOptions Options { get; private set; }

        private ThirdwebPayClient _client;

        public ThirdwebPay(ThirdwebPayOptions options)
        {
            Options = options;

            HttpClient payHttpClient = new HttpClient() { DefaultRequestHeaders = { { "x-secret-key", Options.SecretKey } } };
            _client = new ThirdwebPayClient("https://pay.thirdweb.com/", payHttpClient);
        }

        public async Task<Result5> GetQuoteAsync(string fromAddress, int fromChainId, string fromTokenAddress, string toTokenAddress, string toAmount)
        {
            var quote = await _client.GetSwapQuoteAsync(fromAddress: fromAddress, fromChainId: fromChainId, fromTokenAddress: fromTokenAddress, toTokenAddress: toTokenAddress, toAmount: toAmount);
            // TODO: Check errors
            // TODO: Parse into human readable type
            return quote.Result;
        }

        // TODO: Swap

        public async Task<Result6> GetSwapStatus(string transactionHash)
        {
            var swap = await _client.GetSwapStatusAsync(transactionHash);
            // TODO: Check errors
            // TODO: Parse into human readable type
            return swap.Result;
        }

        // TODO: History
    }
}
