using System.Numerics;
using Nethereum.Hex.HexTypes;

namespace Thirdweb.Tests.Transactions;

public class TransactionTests : BaseTests
{
    public TransactionTests(ITestOutputHelper output)
        : base(output) { }

    private async Task<ThirdwebTransaction> CreateSampleTransaction()
    {
        var wallet = await PrivateKeyWallet.Generate(this.Client);
        var transaction = await ThirdwebTransaction.Create(wallet, new ThirdwebTransactionInput(421614) { To = await wallet.GetAddress(), });
        return transaction;
    }

    [Fact(Timeout = 120000)]
    public void ConstructorArgs_TransactionInput()
    {
        var input = new ThirdwebTransactionInput(
            from: "0x123",
            to: "0x456",
            nonce: 123,
            gas: 123,
            gasPrice: 123,
            value: 123,
            data: "0x123",
            chainId: 123,
            maxFeePerGas: 123,
            maxPriorityFeePerGas: 123,
            zkSync: new ZkSyncOptions()
        );

        Assert.Equal("0x123", input.From);
        Assert.Equal("0x456", input.To);
        Assert.Equal(123, input.Nonce.Value);
        Assert.Equal(123, input.Gas.Value);
        Assert.Equal(123, input.GasPrice.Value);
        Assert.Equal(123, input.Value.Value);
        Assert.Equal("0x123", input.Data);
        Assert.Equal(123, input.ChainId.Value);
        Assert.Equal(123, input.MaxFeePerGas.Value);
        Assert.Equal(123, input.MaxPriorityFeePerGas.Value);
        Assert.NotNull(input.ZkSync);
    }

    [Fact(Timeout = 120000)]
    public async Task Create_ValidatesInputParameters()
    {
        var client = this.Client;
        var wallet = await PrivateKeyWallet.Generate(client);
        var txInput = new ThirdwebTransactionInput(421614) { To = Constants.ADDRESS_ZERO };
        var transaction = await ThirdwebTransaction.Create(wallet, txInput);
        Assert.NotNull(transaction);
    }

    [Fact(Timeout = 120000)]
    public async Task Create_ThrowsOnNoTo()
    {
        var client = this.Client;
        var wallet = await PrivateKeyWallet.Generate(client);
        var txInput = new ThirdwebTransactionInput(421614) { };
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => ThirdwebTransaction.Create(wallet, txInput));
        Assert.Contains("Transaction recipient (to) must be provided", ex.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task Create_ThrowsOnNoWallet()
    {
        var client = this.Client;
        var wallet = await PrivateKeyWallet.Generate(client);
        var txInput = new ThirdwebTransactionInput(421614) { To = Constants.ADDRESS_ZERO };
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => ThirdwebTransaction.Create(null, txInput));
        Assert.Contains("Wallet must be provided", ex.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task Create_ThrowsOnChainIdZero()
    {
        var client = this.Client;
        var wallet = await PrivateKeyWallet.Generate(client);
        var ex = Assert.Throws<ArgumentException>(() => new ThirdwebTransactionInput(0) { To = Constants.ADDRESS_ZERO });
        Assert.Contains("Invalid Chain ID", ex.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task ToString_OverridesCorrectly()
    {
        var transaction = await this.CreateSampleTransaction();
        Assert.NotNull(transaction.ToString());
        Assert.StartsWith("{", transaction.ToString());
    }

    [Fact(Timeout = 120000)]
    public async Task SetTo_UpdatesToAddress()
    {
        var transaction = await this.CreateSampleTransaction();
        _ = transaction.SetTo("0x456");
        Assert.Equal("0x456", transaction.Input.To);
    }

    [Fact(Timeout = 120000)]
    public async Task SetValue_SetsValue()
    {
        var transaction = await this.CreateSampleTransaction();
        var value = new BigInteger(1000);
        _ = transaction.SetValue(value);
        Assert.Equal(value.ToHexBigInteger(), transaction.Input.Value);
    }

    [Fact(Timeout = 120000)]
    public async Task SetData_SetsData()
    {
        var transaction = await this.CreateSampleTransaction();
        var data = "0x123456";
        _ = transaction.SetData(data);
        Assert.Equal(data, transaction.Input.Data);
    }

    [Fact(Timeout = 120000)]
    public async Task SetGasPrice_SetsGasPrice()
    {
        var transaction = await this.CreateSampleTransaction();
        var gas = new BigInteger(1000);
        _ = transaction.SetGasPrice(gas);
        Assert.Equal(gas.ToHexBigInteger(), transaction.Input.GasPrice);
    }

    [Fact(Timeout = 120000)]
    public async Task SetMaxFeePerGas_SetsMaxFeePerGas()
    {
        var transaction = await this.CreateSampleTransaction();
        var gas = new BigInteger(1000);
        _ = transaction.SetMaxFeePerGas(gas);
        Assert.Equal(gas.ToHexBigInteger(), transaction.Input.MaxFeePerGas);
    }

    [Fact(Timeout = 120000)]
    public async Task SetMaxPriorityFeePerGas_SetsMaxPriorityFeePerGas()
    {
        var transaction = await this.CreateSampleTransaction();
        var gas = new BigInteger(1000);
        _ = transaction.SetMaxPriorityFeePerGas(gas);
        Assert.Equal(gas.ToHexBigInteger(), transaction.Input.MaxPriorityFeePerGas);
    }

    [Fact(Timeout = 120000)]
    public async Task SetAllGasParams_ThrowsInvalid()
    {
        var transaction = await this.CreateSampleTransaction();
        var gas = new BigInteger(1000);
        _ = transaction.SetTo(Constants.ADDRESS_ZERO);
        _ = transaction.SetGasPrice(gas);
        _ = transaction.SetMaxFeePerGas(gas);
        _ = transaction.SetMaxPriorityFeePerGas(gas);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => ThirdwebTransaction.Send(transaction));
        Assert.Contains("Transaction GasPrice and MaxFeePerGas/MaxPriorityFeePerGas cannot be set at the same time", ex.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task Sign_SmartWallet_SignsTransaction()
    {
        var client = this.Client;
        var privateKeyAccount = await PrivateKeyWallet.Generate(client);
        var smartAccount = await SmartWallet.Create(personalWallet: privateKeyAccount, factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052", gasless: true, chainId: 421614);
        var transaction = await ThirdwebTransaction.Create(smartAccount, new ThirdwebTransactionInput(421614) { To = Constants.ADDRESS_ZERO, });
        var signed = await ThirdwebTransaction.Sign(transaction);
        Assert.NotNull(signed);
    }

    [Fact(Timeout = 120000)]
    public async Task Send_ThrowsIfToAddressNotProvided()
    {
        var transaction = await this.CreateSampleTransaction();
        _ = transaction.SetTo(null);

        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => ThirdwebTransaction.Send(transaction));
    }

    [Fact(Timeout = 120000)]
    public async Task Send_CorrectlyHandlesNonce()
    {
        var transaction = await this.CreateSampleTransaction();
        _ = transaction.SetNonce(123);

        Assert.Equal("0x7b", transaction.Input.Nonce.HexValue);
        Assert.Equal("123", transaction.Input.Nonce.Value.ToString());
    }

    [Fact(Timeout = 120000)]
    public async Task SetZkSyncOptions_DefaultsToZeroNull()
    {
        // Both null
        var transaction = await this.CreateSampleTransaction();
        _ = transaction.SetZkSyncOptions(new ZkSyncOptions());
        Assert.Equal(0, transaction.Input.ZkSync?.Paymaster);
        Assert.Null(transaction.Input.ZkSync?.PaymasterInput);
        Assert.Null(transaction.Input.ZkSync?.GasPerPubdataByteLimit);
        Assert.Null(transaction.Input.ZkSync?.FactoryDeps);

        // Paymaster null
        transaction = await this.CreateSampleTransaction();
        _ = transaction.SetZkSyncOptions(new ZkSyncOptions(paymaster: null, paymasterInput: "0x"));
        Assert.Equal(0, transaction.Input.ZkSync?.Paymaster);
        Assert.Equal(transaction.Input.ZkSync?.PaymasterInput, Array.Empty<byte>());
        Assert.Null(transaction.Input.ZkSync?.GasPerPubdataByteLimit);
        Assert.Equal(transaction.Input.ZkSync?.FactoryDeps, new List<byte[]>());

        // PaymasterInput null
        transaction = await this.CreateSampleTransaction();
        _ = transaction.SetZkSyncOptions(new ZkSyncOptions(paymaster: "0x", paymasterInput: null));
        Assert.Equal(0, transaction.Input.ZkSync?.Paymaster);
        Assert.Equal(transaction.Input.ZkSync?.PaymasterInput, Array.Empty<byte>());
        Assert.Null(transaction.Input.ZkSync?.GasPerPubdataByteLimit);
        Assert.Equal(transaction.Input.ZkSync?.FactoryDeps, new List<byte[]>());
    }

    [Fact(Timeout = 120000)]
    public async Task Send_ZkSync_TransfersGaslessly()
    {
        var transaction = await this.CreateSampleTransaction();
        _ = transaction.SetChainId(300);
        _ = transaction.SetZkSyncOptions(
            new ZkSyncOptions(
                paymaster: "0xbA226d47Cbb2731CBAA67C916c57d68484AA269F",
                paymasterInput: "0x8c5a344500000000000000000000000000000000000000000000000000000000000000200000000000000000000000000000000000000000000000000000000000000000",
                gasPerPubdataByteLimit: 50000,
                factoryDeps: new List<byte[]>()
            )
        );
        var receipt = await ThirdwebTransaction.SendAndWaitForTransactionReceipt(transaction);
        Assert.NotNull(receipt);
        Assert.StartsWith("0x", receipt.TransactionHash);
    }

    [Fact(Timeout = 120000)]
    public async Task Send_ZkSync_NoGasPerPubFactoryDepsTransfersGaslessly()
    {
        var transaction = await this.CreateSampleTransaction();
        _ = transaction.SetChainId(300);
        _ = transaction.SetZkSyncOptions(
            new ZkSyncOptions(
                paymaster: "0xbA226d47Cbb2731CBAA67C916c57d68484AA269F",
                paymasterInput: "0x8c5a344500000000000000000000000000000000000000000000000000000000000000200000000000000000000000000000000000000000000000000000000000000000"
            )
        );
        var receipt = await ThirdwebTransaction.SendAndWaitForTransactionReceipt(transaction);
        Assert.NotNull(receipt);
        Assert.StartsWith("0x", receipt.TransactionHash);
    }

    [Fact(Timeout = 120000)]
    public async Task EstimateTotalCosts_CalculatesCostsCorrectly()
    {
        var transaction = await this.CreateSampleTransaction();
        _ = transaction.SetValue(new BigInteger(1000));
        _ = transaction.SetGasLimit(21000);
        _ = transaction.SetGasPrice(new BigInteger(1000000000));

        var costs = await ThirdwebTransaction.EstimateTotalCosts(transaction);

        Assert.NotEqual(BigInteger.Zero, costs.Wei);
    }

    [Fact(Timeout = 120000)]
    public async Task EstimateTotalCosts_WithoutSetting_CalculatesCostsCorrectly()
    {
        var transaction = await this.CreateSampleTransaction();
        transaction.Input.From = Constants.ADDRESS_ZERO;
        _ = transaction.SetValue(new BigInteger(1000));

        var costs = await ThirdwebTransaction.EstimateTotalCosts(transaction);

        Assert.NotEqual(BigInteger.Zero, costs.Wei);
    }

    [Fact(Timeout = 120000)]
    public async Task EstimateTotalCosts_WithoutValue_CalculatesCostsCorrectly()
    {
        var transaction = await this.CreateSampleTransaction();

        var costs = await ThirdwebTransaction.EstimateTotalCosts(transaction);

        Assert.NotEqual(BigInteger.Zero, costs.Wei);
    }

    [Fact(Timeout = 120000)]
    public async Task EstimateGasCosts_CalculatesCostsCorrectly()
    {
        var transaction = await this.CreateSampleTransaction();
        _ = transaction.SetValue(new BigInteger(1000));
        _ = transaction.SetGasLimit(21000);
        _ = transaction.SetGasPrice(new BigInteger(1000000000));

        var costs = await ThirdwebTransaction.EstimateGasCosts(transaction);

        Assert.NotEqual(BigInteger.Zero, costs.Wei);
    }

    [Fact(Timeout = 120000)]
    public async Task EstimateGasCosts_WithoutSetting_CalculatesCostsCorrectly()
    {
        var transaction = await this.CreateSampleTransaction();
        transaction.Input.From = Constants.ADDRESS_ZERO;
        _ = transaction.SetValue(new BigInteger(1000));

        var costs = await ThirdwebTransaction.EstimateGasCosts(transaction);

        Assert.NotEqual(BigInteger.Zero, costs.Wei);
    }

    [Fact(Timeout = 120000)]
    public async Task EstimateTotalCosts_HigherThanGasCostsByValue()
    {
        var transaction = await this.CreateSampleTransaction();
        _ = transaction.SetValue(new BigInteger(1000000000000000000)); // 100 gwei accounting for fluctuations
        _ = transaction.SetGasLimit(21000);

        var totalCostsTask = ThirdwebTransaction.EstimateTotalCosts(transaction);
        var gasCostsTask = ThirdwebTransaction.EstimateGasCosts(transaction);

        var costs = await Task.WhenAll(totalCostsTask, gasCostsTask);

        Assert.True(costs[0].Wei > costs[1].Wei);
        Assert.True(costs[0].Wei - costs[1].Wei == transaction.Input.Value.Value);
    }

    [Fact(Timeout = 120000)]
    public async Task EstimateGasFees_ReturnsCorrectly()
    {
        var transaction = await ThirdwebTransaction.Create(await PrivateKeyWallet.Generate(this.Client), new ThirdwebTransactionInput(250) { To = Constants.ADDRESS_ZERO, });

        (var maxFee, var maxPrio) = await ThirdwebTransaction.EstimateGasFees(transaction);

        Assert.NotEqual(BigInteger.Zero, maxFee);
        Assert.NotEqual(BigInteger.Zero, maxPrio);
        Assert.NotEqual(maxFee, maxPrio);
    }

    [Fact(Timeout = 120000)]
    public async Task EstimateGasPrice_BumpsCorrectly()
    {
        var transaction = await this.CreateSampleTransaction();
        var gasPrice = await ThirdwebTransaction.EstimateGasPrice(transaction, withBump: false);
        var gasPriceWithBump = await ThirdwebTransaction.EstimateGasPrice(transaction, withBump: true);
        Assert.NotEqual(gasPrice, gasPriceWithBump);
        Assert.True(gasPriceWithBump > gasPrice);
    }

    [Fact(Timeout = 120000)]
    public async Task Simulate_ThrowsInsufficientFunds()
    {
        var transaction = await this.CreateSampleTransaction();
        _ = transaction.SetValue(new BigInteger(1000000000000000000));
        _ = transaction.SetGasLimit(21000);

        var exception = await Assert.ThrowsAsync<Exception>(() => ThirdwebTransaction.Simulate(transaction));
        Assert.Contains("insufficient funds", exception.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task Simulate_ReturnsDataOrThrowsIntrinsic()
    {
        var client = this.Client;
        var privateKeyAccount = await PrivateKeyWallet.Generate(client);
        var smartAccount = await SmartWallet.Create(personalWallet: privateKeyAccount, factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052", gasless: true, chainId: 421614);
        var transaction = await ThirdwebTransaction.Create(smartAccount, new ThirdwebTransactionInput(421614) { To = Constants.ADDRESS_ZERO, Gas = new HexBigInteger(250000), });

        try
        {
            var data = await ThirdwebTransaction.Simulate(transaction);
            Assert.NotNull(data);
        }
        catch (Exception ex)
        {
            Assert.Contains("intrinsic gas too low", ex.Message);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task WaitForTransactionReceipt()
    {
        var client = this.Client;
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

    [Fact(Timeout = 120000)]
    public async Task WaitForTransactionReceipt_AAReasonString()
    {
        var client = this.Client;
        var chainId = 84532;
        var aaSilentRevertTxHashWithReason = "0x5374743bbb749df47a279ac21e6ed472c30cd471923a7bc78db6a40e1b6924de";
        var aaFailedReceiptWithReason = await Assert.ThrowsAsync<Exception>(async () => await ThirdwebTransaction.WaitForTransactionReceipt(client, chainId, aaSilentRevertTxHashWithReason));
        Assert.StartsWith($"Transaction {aaSilentRevertTxHashWithReason} execution silently reverted:", aaFailedReceiptWithReason.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task WaitForTransactionReceipt_CancellationToken()
    {
        var client = this.Client;
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

        var aaReceipt2 = await ThirdwebTransaction.WaitForTransactionReceipt(client, chainId, aaTxHash, CancellationToken.None);
        Assert.NotNull(aaReceipt2);
    }

    [Fact(Timeout = 120000)]
    public async Task WaitForTransactionReceipt_ToStringReturnsJson()
    {
        var client = this.Client;
        var chainId = 421614;
        var normalTxHash = "0x5a0b6cdb01ecfb25b368d3de1ac844414980ee3c330ec8c1435117b75027b5d7";

        var normalReceipt = await ThirdwebTransaction.WaitForTransactionReceipt(client, chainId, normalTxHash);
        Assert.NotNull(normalReceipt);
        Assert.StartsWith("{", normalReceipt.ToString());
    }
}
