namespace Thirdweb.EWS
{
    internal partial class EmbeddedWallet
    {
        private readonly LocalStorageBase localStorage;
        private readonly ServerBase server;
        private readonly IvGeneratorBase ivGenerator;
        private User user;

        private const int DEVICE_SHARE_ID = 1;
        private const int KEY_SIZE = 256 / 8;
        private const int TAG_SIZE = 16;
        private const int CURRENT_ITERATION_COUNT = 650_000;
        private const string WALLET_PRIVATE_KEY_PREFIX = "thirdweb_";
        private const string ENCRYPTION_SEPARATOR = ":";

        public EmbeddedWallet(ThirdwebClient client, string storageDirectoryPath = null)
        {
            localStorage = new LocalStorage(client.ClientId, storageDirectoryPath);
            server = new Server(client.ClientId, client.BundleId, "dotnet", Constants.VERSION, client.SecretKey);
            ivGenerator = new IvGenerator();
        }
    }
}
