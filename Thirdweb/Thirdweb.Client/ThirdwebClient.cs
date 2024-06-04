[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Thirdweb.Tests")]

namespace Thirdweb
{
    public class ThirdwebClient
    {
        internal string SecretKey { get; }
        internal string ClientId { get; }
        internal string BundleId { get; }
        internal ITimeoutOptions FetchTimeoutOptions { get; }
        internal IThirdwebHttpClient HttpClient { get; }

        private ThirdwebClient(string clientId = null, string secretKey = null, string bundleId = null, ITimeoutOptions fetchTimeoutOptions = null, IThirdwebHttpClient httpClient = null)
        {
            if (string.IsNullOrEmpty(clientId) && string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("ClientId or SecretKey must be provided");
            }

            if (!string.IsNullOrEmpty(secretKey))
            {
                ClientId = Utils.ComputeClientIdFromSecretKey(secretKey);
                SecretKey = secretKey;
            }
            else
            {
                ClientId = clientId;
            }

            BundleId = bundleId;

            FetchTimeoutOptions = fetchTimeoutOptions ?? new TimeoutOptions();

            HttpClient =
                httpClient
                ?? ThirdwebHttpClientFactory.CreateThirdwebHttpClient(
                    new Dictionary<string, string>
                    {
                        { "x-sdk-name", "Thirdweb.NET" },
                        { "x-sdk-os", System.Runtime.InteropServices.RuntimeInformation.OSDescription },
                        { "x-sdk-platform", "dotnet" },
                        { "x-sdk-version", Constants.VERSION },
                        { "x-client-id", ClientId },
                        { "x-secret-key", SecretKey },
                        { "x-bundle-id", BundleId }
                    }
                );
        }

        public static ThirdwebClient Create(string clientId = null, string secretKey = null, string bundleId = null, ITimeoutOptions fetchTimeoutOptions = null, IThirdwebHttpClient httpClient = null)
        {
            return new ThirdwebClient(clientId, secretKey, bundleId, fetchTimeoutOptions, httpClient);
        }
    }
}
