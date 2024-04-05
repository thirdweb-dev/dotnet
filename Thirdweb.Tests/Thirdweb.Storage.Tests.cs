namespace Thirdweb.Tests;

public class StorageTests : BaseTests
{
    public StorageTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task DownloadTest_SecretKey()
    {
        var client = new ThirdwebClient(secretKey: _secretKey);
        var res = await ThirdwebStorage.Download<string>(client, "https://1.rpc.thirdweb.com/providers");
        Assert.NotNull(res);
    }

    [Fact]
    public async Task DownloadTest_Client_BundleId()
    {
        var client = new ThirdwebClient(clientId: _clientIdBundleIdOnly, bundleId: _bundleIdBundleIdOnly);
        var res = await ThirdwebStorage.Download<string>(client, "https://1.rpc.thirdweb.com/providers");
        Assert.NotNull(res);
    }

    [Fact]
    public async Task UploadTest_SecretKey()
    {
        var client = new ThirdwebClient(secretKey: _secretKey);
        var path = Path.Combine(Path.GetTempPath(), "testJson.json");
        File.WriteAllText(path, "{\"test\": \"test\"}");
        var res = await ThirdwebStorage.Upload(client, path);
        Assert.StartsWith($"https://{client.ClientId}.ipfscdn.io/ipfs/", res.PreviewUrl);
    }

    [Fact]
    public async Task UploadTest_Client_BundleId()
    {
        var client = new ThirdwebClient(clientId: _clientIdBundleIdOnly, bundleId: _bundleIdBundleIdOnly);
        var path = Path.Combine(Path.GetTempPath(), "testJson.json");
        File.WriteAllText(path, "{\"test\": \"test\"}");
        var res = await ThirdwebStorage.Upload(client, path);
        Assert.StartsWith($"https://{client.ClientId}.ipfscdn.io/ipfs/", res.PreviewUrl);
    }
}
