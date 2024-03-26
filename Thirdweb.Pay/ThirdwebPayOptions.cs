namespace Thirdweb.Pay
{
    public class ThirdwebPayOptions
    {
        private string _secretKey;

        internal string SecretKey => _secretKey;
        internal string ClientId { get; private set; }
        internal string BundleId { get; private set; }

        public ThirdwebPayOptions(string clientId = null, string secretKey = null, string bundleId = null)
        {
            _secretKey = secretKey;
            ClientId = clientId;
            BundleId = bundleId;
        }
    }
}
