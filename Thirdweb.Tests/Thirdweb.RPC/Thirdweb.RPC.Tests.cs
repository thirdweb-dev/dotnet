using System.Numerics;
using System.Reflection;

namespace Thirdweb.Tests.RPC;

public class RpcTests(ITestOutputHelper output) : BaseTests(output)
{
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
    public async Task TestBatch()
    {
        var client = ThirdwebClient.Create(secretKey: this.SecretKey);
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

    [Fact(Timeout = 120000)]
    public async Task TestCache()
    {
        var client = ThirdwebClient.Create(secretKey: this.SecretKey);
        var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
        var blockNumber1 = await rpc.SendRequestAsync<string>("eth_blockNumber");
        await Task.Delay(100);
        var blockNumber2 = await rpc.SendRequestAsync<string>("eth_blockNumber");
        Assert.Equal(blockNumber1, blockNumber2);
    }

    [Fact(Timeout = 120000)]
    public async Task TestBatchSizeLimit()
    {
        var client = ThirdwebClient.Create(secretKey: this.SecretKey);
        var rpc = ThirdwebRPC.GetRpcInstance(client, 1);
        var blockNumberTasks = new List<Task<string>>();
        for (var i = 0; i < 101; i++)
        {
            blockNumberTasks.Add(rpc.SendRequestAsync<string>("eth_blockNumber"));
        }
        var results = await Task.WhenAll(blockNumberTasks);
        Assert.Equal(101, results.Length);
        Assert.All(results, result => Assert.StartsWith("0x", result));
    }

    [Fact(Timeout = 120000)]
    public void Timer_StartsAndStops()
    {
        var timer = new ThirdwebRPCTimer(TimeSpan.FromMilliseconds(100));
        timer.Start();
        Assert.True(IsTimerRunning(timer));

        timer.Stop();
        Assert.False(IsTimerRunning(timer));
    }

    [Fact(Timeout = 120000)]
    public async Task Timer_ElapsedEventFires()
    {
        var timer = new ThirdwebRPCTimer(TimeSpan.FromMilliseconds(100));
        var eventFired = false;

        timer.Elapsed += () => eventFired = true;
        timer.Start();

        await Task.Delay(200); // Wait for the timer to elapse at least once
        Assert.True(eventFired);

        timer.Stop();
    }

    [Fact(Timeout = 120000)]
    public void Timer_DisposeStopsTimer()
    {
        var timer = new ThirdwebRPCTimer(TimeSpan.FromMilliseconds(100));
        timer.Start();
        timer.Dispose();
        Assert.False(IsTimerRunning(timer));
    }

    private static bool IsTimerRunning(ThirdwebRPCTimer timer)
    {
        var fieldInfo = typeof(ThirdwebRPCTimer).GetField("_isRunning", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("The field '_isRunning' was not found.");
        var value = fieldInfo.GetValue(timer);
        return value == null ? throw new InvalidOperationException("The field '_isRunning' value is null.") : (bool)value;
    }
}
