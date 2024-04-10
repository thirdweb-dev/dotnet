namespace Thirdweb.Tests;

public class UtilsTests : BaseTests
{
    public UtilsTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ComputeClientIdFromSecretKey()
    {
        Assert.True(Utils.ComputeClientIdFromSecretKey(_secretKey).Length == 32);
    }

    [Fact]
    public void HexConcat()
    {
        var hexStrings = new string[] { "0x1234", "0x5678", "0x90AB" };
        Assert.Equal("0x1234567890AB", Utils.HexConcat(hexStrings));
    }

    [Fact]
    public async Task GetTransactionReceipt()
    {
        var client = ThirdwebClient.Create(secretKey: _secretKey);
        var chainId = 421614;
        var normalTxHash = "0x5a0b6cdb01ecfb25b368d3de1ac844414980ee3c330ec8c1435117b75027b5d7";
        var failedTxHash = "0xd2840219ffe172377c8a455c13d95e4dca204d5c0dd72232093e092eef412488";
        var aaTxHash = "0xbf76bd85e1759cf5cf9f4c7c52e76a74d32687f0b516017ff28192d04df50782";
        var aaSilentRevertTxHash = "0x8ada86c63846da7a3f91b8c8332de03f134e7619886425df858ee5400a9d9958";

        var normalReceipt = await Utils.GetTransactionReceipt(client, chainId, normalTxHash);
        Assert.NotNull(normalReceipt);

        var failedReceipt = await Assert.ThrowsAsync<Exception>(async () => await Utils.GetTransactionReceipt(client, chainId, failedTxHash));
        Assert.Equal($"Transaction {failedTxHash} execution reverted.", failedReceipt.Message);

        var aaReceipt = await Utils.GetTransactionReceipt(client, chainId, aaTxHash);
        Assert.NotNull(aaReceipt);

        var aaFailedReceipt = await Assert.ThrowsAsync<Exception>(async () => await Utils.GetTransactionReceipt(client, chainId, aaSilentRevertTxHash));
        Assert.StartsWith($"Transaction {aaSilentRevertTxHash} execution silently reverted", aaFailedReceipt.Message);
    }

    [Fact]
    public async Task GetTransactionReceipt_AAReasonString()
    {
        var client = ThirdwebClient.Create(secretKey: _secretKey);
        var chainId = 84532;
        var aaSilentRevertTxHashWithReason = "0x5374743bbb749df47a279ac21e6ed472c30cd471923a7bc78db6a40e1b6924de";
        var aaFailedReceiptWithReason = await Assert.ThrowsAsync<Exception>(async () => await Utils.GetTransactionReceipt(client, chainId, aaSilentRevertTxHashWithReason));
        Assert.StartsWith($"Transaction {aaSilentRevertTxHashWithReason} execution silently reverted:", aaFailedReceiptWithReason.Message);
    }

    [Fact]
    public async Task GetTransactionReceipt_CancellationToken()
    {
        var client = ThirdwebClient.Create(secretKey: _secretKey);
        var chainId = 421614;
        var normalTxHash = "0x5a0b6cdb01ecfb25b368d3de1ac844414980ee3c330ec8c1435117b75027b5d7";
        var failedTxHash = "0xd2840219ffe172377c8a455c13d95e4dca204d5c0dd72232093e092eef412488";
        var aaTxHash = "0xbf76bd85e1759cf5cf9f4c7c52e76a74d32687f0b516017ff28192d04df50782";
        var aaSilentRevertTxHash = "0x8ada86c63846da7a3f91b8c8332de03f134e7619886425df858ee5400a9d9958";

        var cts = new CancellationTokenSource();
        cts.CancelAfter(10000);
        var normalReceipt = await Utils.GetTransactionReceipt(client, chainId, normalTxHash, cts.Token);
        Assert.NotNull(normalReceipt);

        cts = new CancellationTokenSource();
        cts.CancelAfter(10000);
        var failedReceipt = await Assert.ThrowsAsync<Exception>(async () => await Utils.GetTransactionReceipt(client, chainId, failedTxHash, cts.Token));
        Assert.Equal($"Transaction {failedTxHash} execution reverted.", failedReceipt.Message);

        cts = new CancellationTokenSource();
        cts.CancelAfter(10000);
        var aaReceipt = await Utils.GetTransactionReceipt(client, chainId, aaTxHash, cts.Token);
        Assert.NotNull(aaReceipt);

        cts = new CancellationTokenSource();
        cts.CancelAfter(10000);
        var aaFailedReceipt = await Assert.ThrowsAsync<Exception>(async () => await Utils.GetTransactionReceipt(client, chainId, aaSilentRevertTxHash, cts.Token));
        Assert.StartsWith($"Transaction {aaSilentRevertTxHash} execution silently reverted", aaFailedReceipt.Message);

        var infiniteTxHash = "0x55181384a4b908ddf6311cf0eb55ea0aa2b1ef4d9e0cc047eab9051fec284958";
        cts = new CancellationTokenSource();
        cts.CancelAfter(1);
        var infiniteReceipt = await Assert.ThrowsAsync<TaskCanceledException>(async () => await Utils.GetTransactionReceipt(client, chainId, infiniteTxHash, cts.Token));
        Assert.Equal("A task was canceled.", infiniteReceipt.Message);

        cts = new CancellationTokenSource();
        var infiniteReceipt2 = Assert.ThrowsAsync<TaskCanceledException>(() => Utils.GetTransactionReceipt(client, chainId, infiniteTxHash, cts.Token));
        await Task.Delay(2000);
        cts.Cancel();
        Assert.Equal("A task was canceled.", (await infiniteReceipt2).Message);

        var aaReceipt2 = await Utils.GetTransactionReceipt(client, chainId, aaTxHash, CancellationToken.None);
        Assert.NotNull(aaReceipt2);
    }

    [Fact]
    public void HashPrefixedMessage()
    {
        var messageStr = "Hello, World!";
        var message = System.Text.Encoding.UTF8.GetBytes(messageStr);
        var hashStr = Utils.HashPrefixedMessage(messageStr);
        var hash = Utils.HashPrefixedMessage(message);
        Assert.Equal(hashStr, Utils.BytesToHex(hash));
        Assert.Equal("0xc8ee0d506e864589b799a645ddb88b08f5d39e8049f9f702b3b61fa15e55fc73", hashStr);
    }

    [Fact]
    public void BytesToHex()
    {
        var bytes = new byte[] { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        Assert.Equal("0x1234567890abcdef", Utils.BytesToHex(bytes));
    }

    [Fact]
    public void HexToBytes()
    {
        var hex = "0x1234567890abcdef";
        var bytes = Utils.HexToBytes(hex);
        Assert.Equal(new byte[] { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF }, bytes);
    }

    [Fact]
    public void StringToHex()
    {
        var str = "Hello, World!";
        var hex = Utils.StringToHex(str);
        Assert.Equal("0x48656c6c6f2c20576f726c6421", hex);
    }

    [Fact]
    public void HexToString()
    {
        var hex = "0x48656c6c6f2c20576f726c6421";
        var str = Utils.HexToString(hex);
        Assert.Equal("Hello, World!", str);
    }

    [Fact]
    public void GetUnixTimeStampNow()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var now2 = Utils.GetUnixTimeStampNow();
        Assert.Equal(now, now2);
    }

    [Fact]
    public void GetUnixTimeStampIn10Years()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var tenYears = 60 * 60 * 24 * 365 * 10;
        var tenYearsFromNow = now + tenYears;
        var tenYearsFromNow2 = Utils.GetUnixTimeStampIn10Years();
        Assert.Equal(tenYearsFromNow, tenYearsFromNow2);
    }

    [Fact]
    public void ReplaceIPFS()
    {
        var uri = "ipfs://QmXn1b6Q7";
        var gateway = "https://myawesomegateway.io/ipfs/";
        var replaced = Utils.ReplaceIPFS(uri, gateway);
        Assert.Equal("https://myawesomegateway.io/ipfs/QmXn1b6Q7", replaced);

        uri = "https://myawesomegateway.io/ipfs/QmXn1b6Q7";
        replaced = Utils.ReplaceIPFS(uri, gateway);
        Assert.Equal("https://myawesomegateway.io/ipfs/QmXn1b6Q7", replaced);

        uri = "ipfs://QmXn1b6Q7";
        gateway = null;
        replaced = Utils.ReplaceIPFS(uri, gateway);
        Assert.Equal("https://ipfs.io/ipfs/QmXn1b6Q7", replaced);
    }
}
