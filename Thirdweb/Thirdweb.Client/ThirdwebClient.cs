using System.Numerics;
using Thirdweb.Api;

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

    /// <summary>
    /// Interactiton with https://api.thirdweb.com
    /// Used in some places to enhance the core SDK functionality, or even extend it
    /// </summary>
    internal ThirdwebApiClient Api { get; }

    internal string SecretKey { get; }
    internal string BundleId { get; }
    internal TimeoutOptions FetchTimeoutOptions { get; }
    internal Dictionary<BigInteger, string> RpcOverrides { get; }

    private ThirdwebClient(
        string clientId = null,
        string secretKey = null,
        string bundleId = null,
        TimeoutOptions fetchTimeoutOptions = null,
        IThirdwebHttpClient httpClient = null,
        string sdkName = null,
        string sdkOs = null,
        string sdkPlatform = null,
        string sdkVersion = null,
        Dictionary<BigInteger, string> rpcOverrides = null
    )
    {
        if (string.IsNullOrEmpty(clientId) && string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("ClientId or SecretKey must be provided");
        }

        // Respects provided clientId if any, otherwise computes it from secretKey
        this.ClientId = !string.IsNullOrEmpty(clientId) ? clientId : (string.IsNullOrEmpty(secretKey) ? null : Utils.ComputeClientIdFromSecretKey(secretKey));
        this.SecretKey = secretKey;
        this.BundleId = bundleId;

        this.FetchTimeoutOptions = fetchTimeoutOptions ?? new TimeoutOptions();

        var defaultHeaders = new Dictionary<string, string>
        {
            { "x-sdk-name", sdkName ?? "Thirdweb.NET" },
            { "x-sdk-os", sdkOs ?? System.Runtime.InteropServices.RuntimeInformation.OSDescription },
            { "x-sdk-platform", sdkPlatform ?? "dotnet" },
            { "x-sdk-version", sdkVersion ?? Constants.VERSION },
            { "x-client-id", this.ClientId },
        };
        if (!string.IsNullOrEmpty(this.BundleId))
        {
            defaultHeaders.Add("x-bundle-id", this.BundleId);
        }
        if (!string.IsNullOrEmpty(this.SecretKey))
        {
            defaultHeaders.Add("x-secret-key", this.SecretKey);
        }

        this.HttpClient = httpClient ?? new ThirdwebHttpClient();
        this.HttpClient.SetHeaders(defaultHeaders);

        this.RpcOverrides = rpcOverrides;

        // Initialize the API client with the wrapped HTTP client
        var wrappedHttpClient = new ThirdwebHttpClientWrapper(this.HttpClient);
        this.Api = new ThirdwebApiClient(wrappedHttpClient);
    }

    /// <summary>
    /// Creates a new instance of <see cref="ThirdwebClient"/>.
    /// </summary>
    /// <param name="clientId">The client ID (optional).</param>
    /// <param name="secretKey">The secret key (optional).</param>
    /// <param name="bundleId">The bundle ID (optional).</param>
    /// <param name="fetchTimeoutOptions">The fetch timeout options (optional).</param>
    /// <param name="httpClient">The HTTP client (optional).</param>
    /// <param name="sdkName">The SDK name (optional).</param>
    /// <param name="sdkOs">The SDK OS (optional).</param>
    /// <param name="sdkPlatform">The SDK platform (optional).</param>
    /// <param name="sdkVersion">The SDK version (optional).</param>
    /// <param name="rpcOverrides">Mapping of chain id to your custom rpc for that chain id (optional, defaults to thirdweb RPC).</param>
    /// <returns>A new instance of <see cref="ThirdwebClient"/>.</returns>
    public static ThirdwebClient Create(
        string clientId = null,
        string secretKey = null,
        string bundleId = null,
        TimeoutOptions fetchTimeoutOptions = null,
        IThirdwebHttpClient httpClient = null,
        string sdkName = null,
        string sdkOs = null,
        string sdkPlatform = null,
        string sdkVersion = null,
        Dictionary<BigInteger, string> rpcOverrides = null
    )
    {
        return new ThirdwebClient(clientId, secretKey, bundleId, fetchTimeoutOptions, httpClient, sdkName, sdkOs, sdkPlatform, sdkVersion, rpcOverrides);
    }
}
