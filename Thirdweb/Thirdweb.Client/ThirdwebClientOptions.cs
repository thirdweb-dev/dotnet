namespace Thirdweb
{
    public class ThirdwebClientOptions
    {
        private string _secretKey;

        internal string SecretKey => _secretKey;
        internal string ClientId { get; private set; }
        internal string BundleId { get; private set; }
        internal ITimeoutOptions FetchTimeoutOptions { get; private set; }

        public ThirdwebClientOptions(string clientId = null, string secretKey = null, string bundleId = null, ITimeoutOptions fetchTimeoutOptions = null)
        {
            _secretKey = secretKey;
            ClientId = clientId;
            BundleId = bundleId;
            FetchTimeoutOptions = fetchTimeoutOptions;
        }
    }
}
