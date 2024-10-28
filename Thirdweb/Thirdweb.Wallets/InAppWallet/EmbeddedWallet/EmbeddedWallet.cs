namespace Thirdweb.EWS;

internal partial class EmbeddedWallet
{
    private readonly LocalStorage _localStorage;
    private readonly Server _server;
    private readonly IvGenerator _ivGenerator;

    private const int DEVICE_SHARE_ID = 1;
    private const int KEY_SIZE = 256 / 8;
    private const int TAG_SIZE = 16;
    private const int CURRENT_ITERATION_COUNT = 650_000;
    private const int DEPRECATED_ITERATION_COUNT = 5_000_000;
    private const string WALLET_PRIVATE_KEY_PREFIX = "thirdweb_";
    private const string ENCRYPTION_SEPARATOR = ":";

    public EmbeddedWallet(ThirdwebClient client, string storageDirectoryPath = null, string ecosystemId = null, string ecosystemPartnerId = null)
    {
        this._localStorage = new LocalStorage(client.ClientId, storageDirectoryPath);

        // Create a new client of same type with extra needed headers for EWS
        var headers = client.HttpClient.Headers.ToDictionary(entry => entry.Key, entry => entry.Value);
        var platform = client.HttpClient.Headers["x-sdk-platform"];
        var version = client.HttpClient.Headers["x-sdk-version"];
        if (!string.IsNullOrEmpty(client.ClientId))
        {
            headers.Add("x-thirdweb-client-id", client.ClientId);
        }
        if (!string.IsNullOrEmpty(client.SecretKey))
        {
            headers.Add("x-thirdweb-secret-key", client.SecretKey);
        }
        headers.Add("x-session-nonce", Guid.NewGuid().ToString());
        headers.Add("x-embedded-wallet-version", $"{platform}:{version}");
        if (!string.IsNullOrEmpty(ecosystemId))
        {
            headers.Add("x-ecosystem-id", ecosystemId);

            if (!string.IsNullOrEmpty(ecosystemPartnerId))
            {
                headers.Add("x-ecosystem-partner-id", ecosystemPartnerId);
            }
        }

        var ewsHttpClient = Utils.ReconstructHttpClient(client.HttpClient, headers);

        this._server = new Server(client, ewsHttpClient);

        this._ivGenerator = new IvGenerator(storageDirectoryPath);
    }
}
