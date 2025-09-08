using System.Numerics;
using Thirdweb.RPC;

namespace Thirdweb.Tests.RPC;

public class RpcTests : BaseTests
{
    public RpcTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void RpcOverride_None()
    {
        var client = ThirdwebClient.Create(secretKey: this.SecretKey);
        var thirdwebRpc = $"https://1.rpc.thirdweb.com/{client.ClientId}";
        var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
        Assert.Equal(thirdwebRpc, rpc.RpcUrl.AbsoluteUri);
    }

    [Fact]
    public void RpcOverride_Single()
    {
        var customRpc = "https://eth.llamarpc.com/";
        var client = ThirdwebClient.Create(secretKey: this.SecretKey, rpcOverrides: new Dictionary<BigInteger, string> { { 1, customRpc } });
        var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
        Assert.Equal(customRpc, client.RpcOverrides[1]);
        Assert.Equal(customRpc, rpc.RpcUrl.AbsoluteUri);
    }

    [Fact]
    public void RpcOverride_Multiple()
    {
        var customRpc1 = "https://eth.llamarpc.com/";
        var customRpc42161 = "https://arbitrum.llamarpc.com/";
        var client = ThirdwebClient.Create(secretKey: this.SecretKey, rpcOverrides: new Dictionary<BigInteger, string> { { 1, customRpc1 }, { 42161, customRpc42161 } });
        var rpc1 = ThirdwebRPC.GetRpcInstance(client, 1);
        var rpc42161 = ThirdwebRPC.GetRpcInstance(client, 42161);
        Assert.Equal(customRpc1, client.RpcOverrides[1]);
        Assert.Equal(customRpc1, rpc1.RpcUrl.AbsoluteUri);
        Assert.Equal(customRpc42161, client.RpcOverrides[42161]);
        Assert.Equal(customRpc42161, rpc42161.RpcUrl.AbsoluteUri);
    }

    [Fact]
    public void RpcOverride_Single_Default()
    {
        var customRpc = "https://eth.llamarpc.com/";
        var client = ThirdwebClient.Create(secretKey: this.SecretKey, rpcOverrides: new Dictionary<BigInteger, string> { { 1, customRpc } });

        var thirdwebRpc = $"https://42161.rpc.thirdweb.com/{client.ClientId}";

        var rpc1 = ThirdwebRPC.GetRpcInstance(client, 1);
        Assert.Equal(customRpc, rpc1.RpcUrl.AbsoluteUri);

        var rpc42161 = ThirdwebRPC.GetRpcInstance(client, 42161);
        Assert.Equal(thirdwebRpc, rpc42161.RpcUrl.AbsoluteUri);
    }

    [Fact]
    public void RpcOverride_Multiple_Default()
    {
        var customRpc1 = "https://eth.llamarpc.com/";
        var customRpc42161 = "https://arbitrum.llamarpc.com/";
        var client = ThirdwebClient.Create(secretKey: this.SecretKey, rpcOverrides: new Dictionary<BigInteger, string> { { 1, customRpc1 }, { 42161, customRpc42161 } });

        var thirdwebRpc = $"https://421614.rpc.thirdweb.com/{client.ClientId}";

        var rpc1 = ThirdwebRPC.GetRpcInstance(client, 1);
        Assert.Equal(customRpc1, rpc1.RpcUrl.AbsoluteUri);

        var rpc42161 = ThirdwebRPC.GetRpcInstance(client, 42161);
        Assert.Equal(customRpc42161, rpc42161.RpcUrl.AbsoluteUri);

        var rpc421614 = ThirdwebRPC.GetRpcInstance(client, 421614);
        Assert.Equal(thirdwebRpc, rpc421614.RpcUrl.AbsoluteUri);
    }

    [Fact(Timeout = 120000)]
    public async Task Request_WithRpcOverride()
    {
        var customRpc = "https://eth.llamarpc.com/";
        var client = ThirdwebClient.Create(secretKey: this.SecretKey, rpcOverrides: new Dictionary<BigInteger, string> { { 1, customRpc } });

        var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
        var blockNumber = await rpc.SendRequestAsync<string>("eth_blockNumber");
        Assert.NotNull(blockNumber);
        Assert.StartsWith("0x", blockNumber);

        var rpc2 = ThirdwebRPC.GetRpcInstance(client, 42161);
        var blockNumber2 = await rpc2.SendRequestAsync<string>("eth_blockNumber");
        Assert.NotNull(blockNumber2);
        Assert.StartsWith("0x", blockNumber2);
    }

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
        var ex = await Assert.ThrowsAsync<HttpRequestException>(async () => await rpc.SendRequestAsync<string>("eth_blockNumber"));
        Assert.Contains("401", ex.Message);
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
}
