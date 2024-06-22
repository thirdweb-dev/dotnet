[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Thirdweb.Tests")]

namespace Thirdweb
{
    public class ThirdwebClient
    {
        public IThirdwebHttpClient HttpClient { get; }

        internal string SecretKey { get; }
        internal string ClientId { get; }
        internal string BundleId { get; }
        internal ITimeoutOptions FetchTimeoutOptions { get; }
        internal string RpcUrl { get; }

        private ThirdwebClient(
            string clientId = null,
            string secretKey = null,
            string bundleId = null,
            ITimeoutOptions fetchTimeoutOptions = null,
            IThirdwebHttpClient httpClient = null,
            Dictionary<string, string> headers = null,
            string rpcUrl = null
        )
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

            HttpClient = httpClient ?? new ThirdwebHttpClient();

            if (!string.IsNullOrEmpty(rpcUrl))
            {
                RpcUrl = rpcUrl;
                if (headers != null)
                {
                    HttpClient.SetHeaders(headers);
                }
            }
            else
            {
                RpcUrl = null;
                HttpClient.SetHeaders(
                    headers
                        ?? new Dictionary<string, string>
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
        }

        public static ThirdwebClient Create(
            string clientId = null,
            string secretKey = null,
            string bundleId = null,
            ITimeoutOptions fetchTimeoutOptions = null,
            IThirdwebHttpClient httpClient = null,
            Dictionary<string, string> headers = null,
            string rpcOverride = null
        )
        {
            return new ThirdwebClient(clientId, secretKey, bundleId, fetchTimeoutOptions, httpClient, headers, rpcOverride);
        }
    }
}
