namespace Thirdweb.EWS;

internal partial class EmbeddedWallet
{
    private readonly LocalStorage _localStorage;
    private readonly Server _server;

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
    }
}
