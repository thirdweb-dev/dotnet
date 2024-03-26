namespace Thirdweb.Tests;

public class RpcTests : BaseTests
{
    public RpcTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task GetBlockNumber()
    {
        var client = new ThirdwebClient(new ThirdwebClientOptions(secretKey: _secretKey, fetchTimeoutOptions: new TimeoutOptions(rpc: 10000)));
        var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
        var blockNumber = await rpc.SendRequestAsync<string>("eth_blockNumber");
        Assert.NotNull(blockNumber);
        Assert.StartsWith("0x", blockNumber);
    }

    [Fact]
    public async Task TestAuth()
    {
        var client = new ThirdwebClient(new ThirdwebClientOptions(clientId: "hi", fetchTimeoutOptions: new TimeoutOptions(rpc: 10000)));
        var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () => await rpc.SendRequestAsync<string>("eth_blockNumber"));
        _output.WriteLine($"TestAuth Exception Message: {exception.Message}");
    }

    [Fact]
    public async Task TestTimeout()
    {
        var client = new ThirdwebClient(new ThirdwebClientOptions(secretKey: _secretKey, fetchTimeoutOptions: new TimeoutOptions(rpc: 0)));
        var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
        var exception = await Assert.ThrowsAsync<TimeoutException>(async () => await rpc.SendRequestAsync<string>("eth_chainId"));
        _output.WriteLine($"TestTimeout Exception Message: {exception.Message}");
    }
}
