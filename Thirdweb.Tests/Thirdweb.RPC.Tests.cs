using System.Numerics;

namespace Thirdweb.Tests;

public class RpcTests : BaseTests
{
    public RpcTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task GetBlockNumber()
    {
        var client = new ThirdwebClient(secretKey: _secretKey, fetchTimeoutOptions: new TimeoutOptions(rpc: 10000));
        var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
        var blockNumber = await rpc.SendRequestAsync<string>("eth_blockNumber");
        Assert.NotNull(blockNumber);
        Assert.StartsWith("0x", blockNumber);
    }

    [Fact]
    public async Task TestAuth()
    {
        var client = new ThirdwebClient(clientId: "hi", fetchTimeoutOptions: new TimeoutOptions(rpc: 60000));
        var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () => await rpc.SendRequestAsync<string>("eth_blockNumber"));
        _output.WriteLine($"TestAuth Exception Message: {exception.Message}");
    }

    [Fact]
    public async Task TestTimeout()
    {
        var client = new ThirdwebClient(secretKey: _secretKey, fetchTimeoutOptions: new TimeoutOptions(rpc: 0));
        var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
        var exception = await Assert.ThrowsAsync<TimeoutException>(async () => await rpc.SendRequestAsync<string>("eth_chainId"));
        _output.WriteLine($"TestTimeout Exception Message: {exception.Message}");
    }

    [Fact]
    public async Task TestBatch()
    {
        var client = new ThirdwebClient(secretKey: _secretKey);
        var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
        var req = rpc.SendRequestAsync<string>("eth_blockNumber");
        _ = await rpc.SendRequestAsync<string>("eth_chainId");
        var blockNumberTasks = new List<Task<string>>();
        for (var i = 0; i < 100; i++)
        {
            blockNumberTasks.Add(rpc.SendRequestAsync<string>("eth_blockNumber"));
        }
        var results = await Task.WhenAll(blockNumberTasks);
        Assert.Equal(100, results.Length);
        Assert.All(results, result => Assert.StartsWith("0x", result));
        Assert.All(results, result => Assert.Equal(results[0], result));
    }

    [Fact]
    public async Task TestDeserialization()
    {
        var client = new ThirdwebClient(secretKey: _secretKey);
        var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await rpc.SendRequestAsync<BigInteger>("eth_blockNumber"));
        Assert.Equal("Failed to deserialize RPC response.", exception.Message);
    }

    [Fact]
    public void TestBadInitialization()
    {
        var clientException = Assert.Throws<ArgumentNullException>(() => ThirdwebRPC.GetRpcInstance(null, 0));
        Assert.Equal("client", clientException.ParamName);
        var chainIdException = Assert.Throws<ArgumentException>(() => ThirdwebRPC.GetRpcInstance(new ThirdwebClient(secretKey: _secretKey), 0));
        Assert.Equal("Invalid Chain ID", chainIdException.Message);
    }

    [Fact]
    public async Task TestBundleIdRpc()
    {
        var client = new ThirdwebClient(clientId: _clientIdBundleIdOnly, bundleId: _bundleIdBundleIdOnly);
        var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
        var blockNumber = await rpc.SendRequestAsync<string>("eth_blockNumber");
        Assert.NotNull(blockNumber);
        Assert.StartsWith("0x", blockNumber);
    }

    [Fact]
    public async Task TestRpcError()
    {
        var client = new ThirdwebClient(secretKey: _secretKey);
        var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
        var exception = await Assert.ThrowsAsync<Exception>(async () => await rpc.SendRequestAsync<string>("eth_invalidMethod"));
        Assert.Contains("RPC Error for request", exception.Message);
    }
}
