using System.Numerics;

namespace Thirdweb.Tests.RPC;

public class RpcTests : BaseTests
{
    public RpcTests(ITestOutputHelper output)
        : base(output) { }

    [Fact(Timeout = 120000)]
    public async Task GetBlockNumber()
    {
        var client = ThirdwebClient.Create(secretKey: this.SecretKey, fetchTimeoutOptions: new TimeoutOptions(rpc: 10000));
        var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
        var blockNumber = await rpc.SendRequestAsync<string>("eth_blockNumber");
        Assert.NotNull(blockNumber);
        Assert.StartsWith("0x", blockNumber);
    }

    [Fact(Timeout = 120000)]
    public async Task TestAuth()
    {
        var client = ThirdwebClient.Create(clientId: "hi", fetchTimeoutOptions: new TimeoutOptions(rpc: 60000));
        var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
        _ = await Assert.ThrowsAsync<HttpRequestException>(async () => await rpc.SendRequestAsync<string>("eth_blockNumber"));
    }

    [Fact(Timeout = 120000)]
    public async Task TestTimeout()
    {
        var client = ThirdwebClient.Create(secretKey: this.SecretKey, fetchTimeoutOptions: new TimeoutOptions(rpc: 0));
        var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
        _ = await Assert.ThrowsAsync<TimeoutException>(async () => await rpc.SendRequestAsync<string>("eth_chainId"));
    }

    [Fact(Timeout = 120000)]
    public async Task TestDeserialization()
    {
        var client = ThirdwebClient.Create(secretKey: this.SecretKey);
        var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await rpc.SendRequestAsync<BigInteger>("eth_blockNumber"));
        Assert.Equal("Failed to deserialize RPC response.", exception.Message);
    }

    [Fact(Timeout = 120000)]
    public void TestBadInitialization()
    {
        var clientException = Assert.Throws<ArgumentNullException>(() => ThirdwebRPC.GetRpcInstance(null, 0));
        Assert.Equal("client", clientException.ParamName);
        var chainIdException = Assert.Throws<ArgumentException>(() => ThirdwebRPC.GetRpcInstance(ThirdwebClient.Create(secretKey: this.SecretKey), 0));
        Assert.Equal("Invalid Chain ID", chainIdException.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task TestBundleIdRpc()
    {
        var client = ThirdwebClient.Create(clientId: this.ClientIdBundleIdOnly, bundleId: this.BundleIdBundleIdOnly);
        var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
        var blockNumber = await rpc.SendRequestAsync<string>("eth_blockNumber");
        Assert.NotNull(blockNumber);
        Assert.StartsWith("0x", blockNumber);
    }

    [Fact(Timeout = 120000)]
    public async Task TestRpcError()
    {
        var client = ThirdwebClient.Create(secretKey: this.SecretKey);
        var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
        var exception = await Assert.ThrowsAsync<Exception>(async () => await rpc.SendRequestAsync<string>("eth_invalidMethod"));
        Assert.Contains("RPC Error for request", exception.Message);
    }

    // [Fact(Timeout = 120000)]
    // public async Task TestCache()
    // {
    //     var client = ThirdwebClient.Create(secretKey: this.SecretKey);
    //     var rpc = ThirdwebRPC.GetRpcInstance(client, 421614);
    //     var blockNumber1 = await rpc.SendRequestAsync<string>("eth_blockNumber");
    //     await ThirdwebTask.Delay(1);
    //     var blockNumber2 = await rpc.SendRequestAsync<string>("eth_blockNumber");
    //     Assert.Equal(blockNumber1, blockNumber2);
    //     await ThirdwebTask.Delay(1000);
    //     var blockNumber3 = await rpc.SendRequestAsync<string>("eth_blockNumber");
    //     Assert.NotEqual(blockNumber1, blockNumber3);
    // }
}
