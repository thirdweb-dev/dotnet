namespace Thirdweb.EWS;

internal partial class EmbeddedWallet
{
    private readonly LocalStorage _localStorage;
    private readonly Server _server;
    private readonly IvGenerator _ivGenerator;
    private User _user;

    private const int DEVICE_SHARE_ID = 1;
    private const int KEY_SIZE = 256 / 8;
    private const int TAG_SIZE = 16;
    private const int CURRENT_ITERATION_COUNT = 650_000;
    private const int DEPRECATED_ITERATION_COUNT = 5_000_000;
    private const string WALLET_PRIVATE_KEY_PREFIX = "thirdweb_";
    private const string ENCRYPTION_SEPARATOR = ":";

    public EmbeddedWallet(ThirdwebClient client, string storageDirectoryPath = null)
    {
        this._localStorage = new LocalStorage(client.ClientId, storageDirectoryPath);

        // Create a new client of same type with extra needed headers for EWS
        var thirdwebHttpClientType = client.HttpClient.GetType();
        var ewsHttpClient = thirdwebHttpClientType.GetConstructor(Type.EmptyTypes).Invoke(null) as IThirdwebHttpClient;
        var headers = client.HttpClient.Headers.ToDictionary(entry => entry.Key, entry => entry.Value);
        var platform = client.HttpClient.Headers["x-sdk-platform"];
        var version = client.HttpClient.Headers["x-sdk-version"];
        if (client.ClientId != null)
        {
            headers.Add("x-thirdweb-client-id", client.ClientId);
        }
        if (client.SecretKey != null)
        {
            headers.Add("x-thirdweb-secret-key", client.SecretKey);
        }
        headers.Add("x-session-nonce", Guid.NewGuid().ToString());
        headers.Add("x-embedded-wallet-version", $"{platform}:{version}");
        ewsHttpClient.SetHeaders(headers);

        this._server = new Server(client, ewsHttpClient);

        this._ivGenerator = new IvGenerator(storageDirectoryPath);
    }
}
