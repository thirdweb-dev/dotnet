using System.Numerics;

namespace Thirdweb.Tests;

public class TransactionTests : BaseTests
{
    public TransactionTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task WaitForTransactionReceipt()
    {
        var client = ThirdwebClient.Create(secretKey: _secretKey);
        var chainId = 421614;
        var normalTxHash = "0x5a0b6cdb01ecfb25b368d3de1ac844414980ee3c330ec8c1435117b75027b5d7";
        var failedTxHash = "0xd2840219ffe172377c8a455c13d95e4dca204d5c0dd72232093e092eef412488";
        var aaTxHash = "0xbf76bd85e1759cf5cf9f4c7c52e76a74d32687f0b516017ff28192d04df50782";
        var aaSilentRevertTxHash = "0x8ada86c63846da7a3f91b8c8332de03f134e7619886425df858ee5400a9d9958";

        var normalReceipt = await ThirdwebTransaction.WaitForTransactionReceipt(client, chainId, normalTxHash);
        Assert.NotNull(normalReceipt);

        var failedReceipt = await Assert.ThrowsAsync<Exception>(async () => await ThirdwebTransaction.WaitForTransactionReceipt(client, chainId, failedTxHash));
        Assert.Equal($"Transaction {failedTxHash} execution reverted.", failedReceipt.Message);

        var aaReceipt = await ThirdwebTransaction.WaitForTransactionReceipt(client, chainId, aaTxHash);
        Assert.NotNull(aaReceipt);

        var aaFailedReceipt = await Assert.ThrowsAsync<Exception>(async () => await ThirdwebTransaction.WaitForTransactionReceipt(client, chainId, aaSilentRevertTxHash));
        Assert.StartsWith($"Transaction {aaSilentRevertTxHash} execution silently reverted", aaFailedReceipt.Message);
    }

    [Fact]
    public async Task WaitForTransactionReceipt_AAReasonString()
    {
        var client = ThirdwebClient.Create(secretKey: _secretKey);
        var chainId = 84532;
        var aaSilentRevertTxHashWithReason = "0x5374743bbb749df47a279ac21e6ed472c30cd471923a7bc78db6a40e1b6924de";
        var aaFailedReceiptWithReason = await Assert.ThrowsAsync<Exception>(async () => await ThirdwebTransaction.WaitForTransactionReceipt(client, chainId, aaSilentRevertTxHashWithReason));
        Assert.StartsWith($"Transaction {aaSilentRevertTxHashWithReason} execution silently reverted:", aaFailedReceiptWithReason.Message);
    }

    [Fact]
    public async Task WaitForTransactionReceipt_CancellationToken()
    {
        var client = ThirdwebClient.Create(secretKey: _secretKey);
        var chainId = 421614;
        var normalTxHash = "0x5a0b6cdb01ecfb25b368d3de1ac844414980ee3c330ec8c1435117b75027b5d7";
        var failedTxHash = "0xd2840219ffe172377c8a455c13d95e4dca204d5c0dd72232093e092eef412488";
        var aaTxHash = "0xbf76bd85e1759cf5cf9f4c7c52e76a74d32687f0b516017ff28192d04df50782";
        var aaSilentRevertTxHash = "0x8ada86c63846da7a3f91b8c8332de03f134e7619886425df858ee5400a9d9958";

        var cts = new CancellationTokenSource();
        cts.CancelAfter(10000);
        var normalReceipt = await ThirdwebTransaction.WaitForTransactionReceipt(client, chainId, normalTxHash, cts.Token);
        Assert.NotNull(normalReceipt);

        cts = new CancellationTokenSource();
        cts.CancelAfter(10000);
        var failedReceipt = await Assert.ThrowsAsync<Exception>(async () => await ThirdwebTransaction.WaitForTransactionReceipt(client, chainId, failedTxHash, cts.Token));
        Assert.Equal($"Transaction {failedTxHash} execution reverted.", failedReceipt.Message);

        cts = new CancellationTokenSource();
        cts.CancelAfter(10000);
        var aaReceipt = await ThirdwebTransaction.WaitForTransactionReceipt(client, chainId, aaTxHash, cts.Token);
        Assert.NotNull(aaReceipt);

        cts = new CancellationTokenSource();
        cts.CancelAfter(10000);
        var aaFailedReceipt = await Assert.ThrowsAsync<Exception>(async () => await ThirdwebTransaction.WaitForTransactionReceipt(client, chainId, aaSilentRevertTxHash, cts.Token));
        Assert.StartsWith($"Transaction {aaSilentRevertTxHash} execution silently reverted", aaFailedReceipt.Message);

        var infiniteTxHash = "0x55181384a4b908ddf6311cf0eb55ea0aa2b1ef4d9e0cc047eab9051fec284958";
        cts = new CancellationTokenSource();
        cts.CancelAfter(1);
        var infiniteReceipt = await Assert.ThrowsAsync<TaskCanceledException>(async () => await ThirdwebTransaction.WaitForTransactionReceipt(client, chainId, infiniteTxHash, cts.Token));
        Assert.Equal("A task was canceled.", infiniteReceipt.Message);

        cts = new CancellationTokenSource();
        var infiniteReceipt2 = Assert.ThrowsAsync<TaskCanceledException>(() => ThirdwebTransaction.WaitForTransactionReceipt(client, chainId, infiniteTxHash, cts.Token));
        await Task.Delay(2000);
        cts.Cancel();
        Assert.Equal("A task was canceled.", (await infiniteReceipt2).Message);

        var aaReceipt2 = await ThirdwebTransaction.WaitForTransactionReceipt(client, chainId, aaTxHash, CancellationToken.None);
        Assert.NotNull(aaReceipt2);
    }
}
