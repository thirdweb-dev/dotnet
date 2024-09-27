[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Thirdweb.Tests")]

namespace Thirdweb;

/// <summary>
/// Represents a client for interacting with the Thirdweb API.
/// </summary>
public class ThirdwebClient
{
    /// <summary>
    /// Gets the HTTP client used by the Thirdweb client.
    /// </summary>
    public IThirdwebHttpClient HttpClient { get; }

    /// <summary>
    /// Gets the client ID.
    /// </summary>
    public string ClientId { get; }

    internal string SecretKey { get; }
    internal string BundleId { get; }
    internal ITimeoutOptions FetchTimeoutOptions { get; }

    private ThirdwebClient(string clientId = null, string secretKey = null, string bundleId = null, ITimeoutOptions fetchTimeoutOptions = null, IThirdwebHttpClient httpClient = null)
    {
        if (string.IsNullOrEmpty(clientId) && string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("ClientId or SecretKey must be provided");
        }

        if (!string.IsNullOrEmpty(secretKey))
        {
            this.ClientId = Utils.ComputeClientIdFromSecretKey(secretKey);
            this.SecretKey = secretKey;
        }
        else
        {
            this.ClientId = clientId;
        }

        this.BundleId = bundleId;

        this.FetchTimeoutOptions = fetchTimeoutOptions ?? new TimeoutOptions();

        if (httpClient != null)
        {
            this.HttpClient = httpClient;
        }
        else
        {
            var defaultHeaders = new Dictionary<string, string>
            {
                { "x-sdk-name", "Thirdweb.NET" },
                { "x-sdk-os", System.Runtime.InteropServices.RuntimeInformation.OSDescription },
                { "x-sdk-platform", "dotnet" },
                { "x-sdk-version", Constants.VERSION },
                { "x-client-id", this.ClientId },
            };
            if (!string.IsNullOrEmpty(this.SecretKey))
            {
                defaultHeaders.Add("x-secret-key", this.SecretKey);
            }
            if (!string.IsNullOrEmpty(this.BundleId))
            {
                defaultHeaders.Add("x-bundle-id", this.BundleId);
            }
            this.HttpClient = new ThirdwebHttpClient(defaultHeaders);
        }
    }

    /// <summary>
    /// Creates a new instance of <see cref="ThirdwebClient"/>.
    /// </summary>
    /// <param name="clientId">The client ID (optional).</param>
    /// <param name="secretKey">The secret key (optional).</param>
    /// <param name="bundleId">The bundle ID (optional).</param>
    /// <param name="fetchTimeoutOptions">The fetch timeout options (optional).</param>
    /// <param name="httpClient">The HTTP client (optional).</param>
    /// <returns>A new instance of <see cref="ThirdwebClient"/>.</returns>
    public static ThirdwebClient Create(string clientId = null, string secretKey = null, string bundleId = null, ITimeoutOptions fetchTimeoutOptions = null, IThirdwebHttpClient httpClient = null)
    {
        return new ThirdwebClient(clientId, secretKey, bundleId, fetchTimeoutOptions, httpClient);
    }
}
