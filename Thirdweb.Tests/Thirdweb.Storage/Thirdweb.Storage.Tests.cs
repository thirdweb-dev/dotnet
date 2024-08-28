namespace Thirdweb.Tests.Storage;

public class StorageTests : BaseTests
{
    public StorageTests(ITestOutputHelper output)
        : base(output) { }

    [Fact(Timeout = 120000)]
    public async Task DownloadTest_SecretKey()
    {
        var client = ThirdwebClient.Create(secretKey: this.SecretKey);
        var res = await ThirdwebStorage.Download<string>(client, "https://1.rpc.thirdweb.com/providers");
        Assert.NotNull(res);
    }

    [Fact(Timeout = 120000)]
    public async Task DownloadTest_Client_BundleId()
    {
        var client = ThirdwebClient.Create(clientId: this.ClientIdBundleIdOnly, bundleId: this.BundleIdBundleIdOnly);
        var res = await ThirdwebStorage.Download<string>(client, "https://1.rpc.thirdweb.com/providers");
        Assert.NotNull(res);
    }

    [Fact(Timeout = 120000)]
    public async Task DownloadTest_Deserialization()
    {
        var client = ThirdwebClient.Create(secretKey: this.SecretKey);
        var res = await ThirdwebStorage.Download<List<string>>(client, "https://1.rpc.thirdweb.com/providers");
        Assert.NotNull(res);
        Assert.NotEmpty(res);
    }

    [Fact(Timeout = 120000)]
    public async Task DownloadTest_NullUri()
    {
        var client = ThirdwebClient.Create(secretKey: this.SecretKey);
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ThirdwebStorage.Download<string>(client, null));
        Assert.Equal("uri", exception.ParamName);
    }

    [Fact(Timeout = 120000)]
    public async Task DownloadTest_ThirdwebIPFS()
    {
        var client = ThirdwebClient.Create(secretKey: this.SecretKey);
        var res = await ThirdwebStorage.Download<string>(client, "ipfs://QmRHf3sBEAaSkaPdjrnYZS7VH1jVgvNBJNoUXmiUyvUpNM/8");
        Assert.NotNull(res);
    }

    [Fact(Timeout = 120000)]
    public async Task DownloadTest_Bytes()
    {
        var client = ThirdwebClient.Create(secretKey: this.SecretKey);
        var res = await ThirdwebStorage.Download<byte[]>(client, "https://1.rpc.thirdweb.com/providers");
        Assert.NotNull(res);
        Assert.NotEmpty(res);
    }

    [Fact(Timeout = 120000)]
    public async Task DownloadTest_400()
    {
        var client = ThirdwebClient.Create(secretKey: this.SecretKey);
        var exception = await Assert.ThrowsAsync<Exception>(() => ThirdwebStorage.Download<string>(client, "https://0.rpc.thirdweb.com/"));
        Assert.Contains("Failed to download", exception.Message);
        Assert.Contains("400", exception.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task DownloadTest_Timeout()
    {
        var client = ThirdwebClient.Create(secretKey: this.SecretKey, fetchTimeoutOptions: new TimeoutOptions(storage: 0));
        var exception = await Assert.ThrowsAsync<TaskCanceledException>(() => ThirdwebStorage.Download<string>(client, "https://1.rpc.thirdweb.com/providers", 1));
        Assert.Contains("A task was canceled", exception.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task UploadTest_SecretKey()
    {
        var client = ThirdwebClient.Create(secretKey: this.SecretKey);
        var path = Path.Combine(Path.GetTempPath(), "testJson.json");
        File.WriteAllText(
            path, /*lang=json,strict*/
            "{\"test\": \"test\"}"
        );
        var res = await ThirdwebStorage.Upload(client, path);
        Assert.StartsWith($"https://{client.ClientId}.ipfscdn.io/ipfs/", res.PreviewUrl);
    }

    [Fact(Timeout = 120000)]
    public async Task UploadTest_Client_BundleId()
    {
        var client = ThirdwebClient.Create(clientId: this.ClientIdBundleIdOnly, bundleId: this.BundleIdBundleIdOnly);
        var path = Path.Combine(Path.GetTempPath(), "testJson.json");
        File.WriteAllText(
            path, /*lang=json,strict*/
            "{\"test\": \"test\"}"
        );
        var res = await ThirdwebStorage.Upload(client, path);
        Assert.StartsWith($"https://{client.ClientId}.ipfscdn.io/ipfs/", res.PreviewUrl);
    }

    [Fact(Timeout = 120000)]
    public async Task UploadTest_NullPath()
    {
        var client = ThirdwebClient.Create(secretKey: this.SecretKey);
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ThirdwebStorage.Upload(client, null));
        Assert.Equal("path", exception.ParamName);
    }

    [Fact(Timeout = 120000)]
    public async Task UploadTest_401()
    {
        var client = ThirdwebClient.Create(clientId: "invalid", bundleId: "hello");
        var path = Path.Combine(Path.GetTempPath(), "testJson.json");
        File.WriteAllText(
            path, /*lang=json,strict*/
            "{\"test\": \"test\"}"
        );
        var exception = await Assert.ThrowsAsync<Exception>(() => ThirdwebStorage.Upload(client, path));
        Assert.Contains("Failed to upload", exception.Message);
        Assert.Contains("401", exception.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task UploadTest_RawBytes_Null()
    {
        var client = ThirdwebClient.Create(secretKey: this.SecretKey);
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ThirdwebStorage.UploadRaw(client, null));
        Assert.Equal("rawBytes", exception.ParamName);
    }

    [Fact(Timeout = 120000)]
    public async Task UploadTest_RawBytes_Empty()
    {
        var client = ThirdwebClient.Create(secretKey: this.SecretKey);
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => ThirdwebStorage.UploadRaw(client, Array.Empty<byte>()));
        Assert.Equal("rawBytes", exception.ParamName);
    }
}
