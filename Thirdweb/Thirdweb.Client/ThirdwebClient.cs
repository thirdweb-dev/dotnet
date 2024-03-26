[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Thirdweb.Tests")]

namespace Thirdweb
{
    public class ThirdwebClient
    {
        private string _secretKey;

        internal string SecretKey => _secretKey;
        internal string ClientId { get; private set; }
        internal string BundleId { get; private set; }
        internal ITimeoutOptions FetchTimeoutOptions { get; private set; }

        public ThirdwebClient(ThirdwebClientOptions options)
        {
            if (string.IsNullOrEmpty(options.ClientId) && string.IsNullOrEmpty(options.SecretKey))
            {
                throw new ArgumentNullException(nameof(options), "ClientId or SecretKey must be provided");
            }

            if (!string.IsNullOrEmpty(options.SecretKey))
            {
                ClientId = Utils.ComputeClientIdFromSecretKey(options.SecretKey);
                _secretKey = options.SecretKey;
            }
            else
            {
                ClientId = options.ClientId;
            }

            BundleId = options.BundleId;

            FetchTimeoutOptions = options.FetchTimeoutOptions ?? new TimeoutOptions();
        }
    }
}
