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
    public async Task DownloadTest_Deserialization()
    {
        var client = new ThirdwebClient(secretKey: _secretKey);
        var res = await ThirdwebStorage.Download<List<string>>(client, "https://1.rpc.thirdweb.com/providers");
        Assert.NotNull(res);
        Assert.NotEmpty(res);
    }

    [Fact]
    public async Task DownloadTest_NullUri()
    {
        var client = new ThirdwebClient(secretKey: _secretKey);
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ThirdwebStorage.Download<string>(client, null));
        Assert.Equal("uri", exception.ParamName);
    }

    [Fact]
    public async Task DownloadTest_ThirdwebIPFS()
    {
        var client = new ThirdwebClient(secretKey: _secretKey);
        var res = await ThirdwebStorage.Download<string>(client, "ipfs://QmRHf3sBEAaSkaPdjrnYZS7VH1jVgvNBJNoUXmiUyvUpNM/8");
        Assert.NotNull(res);
    }

    [Fact]
    public async Task DownloadTest_400()
    {
        var client = new ThirdwebClient(secretKey: _secretKey);
        var exception = await Assert.ThrowsAsync<Exception>(() => ThirdwebStorage.Download<string>(client, "https://0.rpc.thirdweb.com/"));
        Assert.Contains("Failed to download", exception.Message);
        Assert.Contains("BadRequest", exception.Message);
    }

    [Fact]
    public async Task DownloadTest_Timeout()
    {
        var client = new ThirdwebClient(secretKey: _secretKey, fetchTimeoutOptions: new TimeoutOptions(storage: 0));
        var exception = await Assert.ThrowsAsync<TaskCanceledException>(() => ThirdwebStorage.Download<string>(client, "https://1.rpc.thirdweb.com/providers", 1));
        Assert.Contains("A task was canceled", exception.Message);
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

    [Fact]
    public async Task UploadTest_NullPath()
    {
        var client = new ThirdwebClient(secretKey: _secretKey);
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ThirdwebStorage.Upload(client, null));
        Assert.Equal("path", exception.ParamName);
    }

    [Fact]
    public async Task UploadTest_401()
    {
        var client = new ThirdwebClient(clientId: "invalid", bundleId: "hello");
        var path = Path.Combine(Path.GetTempPath(), "testJson.json");
        File.WriteAllText(path, "{\"test\": \"test\"}");
        var exception = await Assert.ThrowsAsync<Exception>(() => ThirdwebStorage.Upload(client, path));
        Assert.Contains("Failed to upload", exception.Message);
        Assert.Contains("Unauthorized", exception.Message);
    }
}
