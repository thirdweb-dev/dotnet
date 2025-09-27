using System.Numerics;
using Nethereum.Hex.HexTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Thirdweb;

/// <summary>
/// Represents the total costs in ether and wei.
/// </summary>
public struct TotalCosts
{
    /// <summary>
    /// The cost in ether.
    /// </summary>
    public string Ether { get; set; }

    /// <summary>
    /// The cost in wei.
    /// </summary>
    public BigInteger Wei { get; set; }
}

/// <summary>
/// Represents a Thirdweb transaction.
/// </summary>
public class ThirdwebTransaction
{
    public ThirdwebTransactionInput Input { get; set; }

    internal readonly IThirdwebWallet Wallet;

    private ThirdwebTransaction(IThirdwebWallet wallet, ThirdwebTransactionInput txInput)
    {
        this.Input = txInput;
        this.Wallet = wallet;
    }

    /// <summary>
    /// Creates a new Thirdweb transaction.
    /// </summary>
    /// <param name="wallet">The wallet to use for the transaction.</param>
    /// <param name="txInput">The transaction input.</param>
    /// <returns>A new Thirdweb transaction.</returns>
    public static async Task<ThirdwebTransaction> Create(IThirdwebWallet wallet, ThirdwebTransactionInput txInput)
    {
        if (wallet == null)
        {
            throw new ArgumentException("Wallet must be provided", nameof(wallet));
        }

        if (txInput.To == null)
        {
            throw new ArgumentException("Transaction recipient (to) must be provided", nameof(txInput));
        }

        txInput.From = await wallet.GetAddress().ConfigureAwait(false);
        txInput.Data ??= "0x";
        txInput.Value ??= new HexBigInteger(0);

        return new ThirdwebTransaction(wallet, txInput);
    }

    /// <summary>
    /// Converts the transaction input to a JSON string.
    /// </summary>
    /// <returns>A JSON string representation of the transaction input.</returns>
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this.Input);
    }

    /// <summary>
    /// Sets the recipient address of the transaction.
    /// </summary>
    /// <param name="to">The recipient address.</param>
    /// <returns>The updated transaction.</returns>
    public ThirdwebTransaction SetTo(string to)
    {
        this.Input.To = to;
        return this;
    }

    /// <summary>
    /// Sets the data for the transaction.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <returns>The updated transaction.</returns>
    public ThirdwebTransaction SetData(string data)
    {
        this.Input.Data = data;
        return this;
    }

    /// <summary>
    /// Sets the value to be transferred in the transaction.
    /// </summary>
    /// <param name="weiValue">The value in wei.</param>
    /// <returns>The updated transaction.</returns>
    public ThirdwebTransaction SetValue(BigInteger weiValue)
    {
        this.Input.Value = weiValue.ToHexBigInteger();
        return this;
    }

    /// <summary>
    /// Sets the gas limit for the transaction.
    /// </summary>
    /// <param name="gas">The gas limit.</param>
    /// <returns>The updated transaction.</returns>
    public ThirdwebTransaction SetGasLimit(BigInteger gas)
    {
        this.Input.Gas = gas.ToHexBigInteger();
        return this;
    }

    /// <summary>
    /// Sets the gas price for the transaction.
    /// </summary>
    /// <param name="gasPrice">The gas price.</param>
    /// <returns>The updated transaction.</returns>
    public ThirdwebTransaction SetGasPrice(BigInteger gasPrice)
    {
        this.Input.GasPrice = gasPrice.ToHexBigInteger();
        return this;
    }

    /// <summary>
    /// Sets the nonce for the transaction.
    /// </summary>
    /// <param name="nonce">The nonce.</param>
    /// <returns>The updated transaction.</returns>
    public ThirdwebTransaction SetNonce(BigInteger nonce)
    {
        this.Input.Nonce = nonce.ToHexBigInteger();
        return this;
    }

    /// <summary>
    /// Sets the maximum fee per gas for the transaction.
    /// </summary>
    /// <param name="maxFeePerGas">The maximum fee per gas.</param>
    /// <returns>The updated transaction.</returns>
    public ThirdwebTransaction SetMaxFeePerGas(BigInteger maxFeePerGas)
    {
        this.Input.MaxFeePerGas = maxFeePerGas.ToHexBigInteger();
        return this;
    }

    /// <summary>
    /// Sets the maximum priority fee per gas for the transaction.
    /// </summary>
    /// <param name="maxPriorityFeePerGas">The maximum priority fee per gas.</param>
    /// <returns>The updated transaction.</returns>
    public ThirdwebTransaction SetMaxPriorityFeePerGas(BigInteger maxPriorityFeePerGas)
    {
        this.Input.MaxPriorityFeePerGas = maxPriorityFeePerGas.ToHexBigInteger();
        return this;
    }

    /// <summary>
    /// Sets the chain ID for the transaction.
    /// </summary>
    /// <param name="chainId">The chain ID.</param>
    /// <returns>The updated transaction.</returns>
    public ThirdwebTransaction SetChainId(BigInteger chainId)
    {
        this.Input.ChainId = chainId.ToHexBigInteger();
        return this;
    }

    /// <summary>
    /// Sets the zkSync options for the transaction.
    /// </summary>
    /// <param name="zkSyncOptions">The zkSync options.</param>
    /// <returns>The updated transaction.</returns>
    public ThirdwebTransaction SetZkSyncOptions(ZkSyncOptions zkSyncOptions)
    {
        this.Input.ZkSync = zkSyncOptions;
        return this;
    }

    /// <summary>
    /// Estimates the gas costs for the transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <returns>The estimated gas costs.</returns>
    public static async Task<TotalCosts> EstimateGasCosts(ThirdwebTransaction transaction)
    {
        var gasPrice = transaction.Input.GasPrice?.Value ?? await EstimateGasPrice(transaction).ConfigureAwait(false);
        var gasLimit = transaction.Input.Gas?.Value ?? await EstimateGasLimit(transaction).ConfigureAwait(false);
        var gasCost = BigInteger.Multiply(gasLimit, gasPrice);
        return new TotalCosts { Ether = gasCost.ToString().ToEth(18, false), Wei = gasCost };
    }

    /// <summary>
    /// Estimates the total costs for the transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <returns>The estimated total costs.</returns>
    public static async Task<TotalCosts> EstimateTotalCosts(ThirdwebTransaction transaction)
    {
        var gasCosts = await EstimateGasCosts(transaction).ConfigureAwait(false);
        var value = transaction.Input.Value?.Value ?? 0;
        return new TotalCosts { Ether = (value + gasCosts.Wei).ToString().ToEth(18, false), Wei = value + gasCosts.Wei };
    }

    /// <summary>
    /// Estimates the gas price for the transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="withBump">Whether to include a bump in the gas price.</param>
    /// <returns>The estimated gas price.</returns>
    public static async Task<BigInteger> EstimateGasPrice(ThirdwebTransaction transaction, bool withBump = true)
    {
        return await Utils.FetchGasPrice(transaction.Wallet.Client, transaction.Input.ChainId.Value, withBump).ConfigureAwait(false);
    }

    /// <summary>
    /// Estimates the gas fees for the transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="withBump">Whether to include a bump in the gas fees.</param>
    /// <returns>The estimated maximum fee per gas and maximum priority fee per gas.</returns>
    public static async Task<(BigInteger maxFeePerGas, BigInteger maxPriorityFeePerGas)> EstimateGasFees(ThirdwebTransaction transaction, bool withBump = true)
    {
        var rpc = ThirdwebRPC.GetRpcInstance(transaction.Wallet.Client, transaction.Input.ChainId.Value);
        var chainId = transaction.Input.ChainId.Value;

        if (await Utils.IsZkSync(transaction.Wallet.Client, transaction.Input.ChainId.Value).ConfigureAwait(false))
        {
            var fees = await rpc.SendRequestAsync<JToken>("zks_estimateFee", transaction.Input).ConfigureAwait(false);
            var maxFee = fees["max_fee_per_gas"].ToObject<HexBigInteger>().Value;
            var maxPriorityFee = fees["max_priority_fee_per_gas"].ToObject<HexBigInteger>().Value;
            return withBump ? (maxFee * 10 / 5, maxPriorityFee * 10 / 5) : (maxFee, maxPriorityFee);
        }
        else
        {
            return await Utils.FetchGasFees(transaction.Wallet.Client, chainId, withBump).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Simulates the transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <returns>The result of the simulation.</returns>
    public static async Task<string> Simulate(ThirdwebTransaction transaction)
    {
        var rpc = ThirdwebRPC.GetRpcInstance(transaction.Wallet.Client, transaction.Input.ChainId.Value);
        return await rpc.SendRequestAsync<string>("eth_call", transaction.Input, "latest");
    }

    /// <summary>
    /// Estimates the gas limit for the transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <returns>The estimated gas limit.</returns>
    public static async Task<BigInteger> EstimateGasLimit(ThirdwebTransaction transaction)
    {
        var rpc = ThirdwebRPC.GetRpcInstance(transaction.Wallet.Client, transaction.Input.ChainId.Value);
        var isZkSync = await Utils.IsZkSync(transaction.Wallet.Client, transaction.Input.ChainId.Value).ConfigureAwait(false);
        BigInteger divider =
            isZkSync ? 7
            : transaction.Input.AuthorizationList == null ? 5
            : 3;
        BigInteger baseGas;
        if (isZkSync)
        {
            var hex = (await rpc.SendRequestAsync<JToken>("zks_estimateFee", transaction.Input).ConfigureAwait(false))["gas_limit"].ToString();
            baseGas = hex.HexToNumber();
        }
        else
        {
            var hex = await rpc.SendRequestAsync<string>("eth_estimateGas", transaction.Input).ConfigureAwait(false);
            baseGas = hex.HexToNumber();
        }
        return baseGas * 10 / divider;
    }

    /// <summary>
    /// Gets the nonce for the transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <returns>The nonce.</returns>
    public static async Task<BigInteger> GetNonce(ThirdwebTransaction transaction)
    {
        return await transaction.Wallet.GetTransactionCount(chainId: transaction.Input.ChainId, blocktag: "pending").ConfigureAwait(false);
    }

    private static async Task<BigInteger> GetGasPerPubData(ThirdwebTransaction transaction)
    {
        var rpc = ThirdwebRPC.GetRpcInstance(transaction.Wallet.Client, transaction.Input.ChainId.Value);
        var hex = (await rpc.SendRequestAsync<JToken>("zks_estimateFee", transaction.Input).ConfigureAwait(false))["gas_per_pubdata_limit"].ToString();
        var finalGasPerPubData = new HexBigInteger(hex).Value * 10 / 5;
        return finalGasPerPubData < 10000 ? 10000 : finalGasPerPubData;
    }

    /// <summary>
    /// Signs the transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <returns>The signed transaction.</returns>
    public static async Task<string> Sign(ThirdwebTransaction transaction)
    {
        return await transaction.Wallet.SignTransaction(transaction.Input).ConfigureAwait(false);
    }

    /// <summary>
    /// Populates the transaction and prepares it for sending.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <returns>The populated transaction.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static async Task<ThirdwebTransaction> Prepare(ThirdwebTransaction transaction)
    {
        if (transaction.Input.To == null)
        {
            throw new InvalidOperationException("Transaction recipient (to) must be provided");
        }

        if (transaction.Input.GasPrice != null && (transaction.Input.MaxFeePerGas != null || transaction.Input.MaxPriorityFeePerGas != null))
        {
            throw new InvalidOperationException("Transaction GasPrice and MaxFeePerGas/MaxPriorityFeePerGas cannot be set at the same time");
        }

        transaction.Input.Nonce ??= new HexBigInteger(await GetNonce(transaction).ConfigureAwait(false));
        transaction.Input.Value ??= new HexBigInteger(0);
        transaction.Input.Data ??= "0x";
        transaction.Input.Gas ??= new HexBigInteger(await EstimateGasLimit(transaction).ConfigureAwait(false));

        var supports1559 = Utils.IsEip1559Supported(transaction.Input.ChainId.Value.ToString());
        if (supports1559)
        {
            if (transaction.Input.GasPrice == null)
            {
                var (maxFeePerGas, maxPriorityFeePerGas) = await EstimateGasFees(transaction).ConfigureAwait(false);
                transaction.Input.MaxFeePerGas ??= new HexBigInteger(maxFeePerGas);
                transaction.Input.MaxPriorityFeePerGas ??= new HexBigInteger(maxPriorityFeePerGas);
            }
        }
        else
        {
            if (transaction.Input.MaxFeePerGas == null && transaction.Input.MaxPriorityFeePerGas == null)
            {
                transaction.Input.GasPrice ??= new HexBigInteger(await EstimateGasPrice(transaction).ConfigureAwait(false));
            }
        }

        return transaction;
    }

    /// <summary>
    /// Sends the transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <returns>The transaction hash.</returns>
    public static async Task<string> Send(ThirdwebTransaction transaction)
    {
        transaction = await Prepare(transaction).ConfigureAwait(false);

        var rpc = ThirdwebRPC.GetRpcInstance(transaction.Wallet.Client, transaction.Input.ChainId.Value);
        string hash;

        if (await Utils.IsZkSync(transaction.Wallet.Client, transaction.Input.ChainId.Value).ConfigureAwait(false) && transaction.Input.ZkSync.HasValue)
        {
            var zkTx = await ConvertToZkSyncTransaction(transaction).ConfigureAwait(false);
            var zkTxSigned = await EIP712.GenerateSignature_ZkSyncTransaction("zkSync", "2", transaction.Input.ChainId.Value, zkTx, transaction.Wallet).ConfigureAwait(false);
            hash = await rpc.SendRequestAsync<string>("eth_sendRawTransaction", zkTxSigned).ConfigureAwait(false);
        }
        else
        {
            switch (transaction.Wallet.AccountType)
            {
                case ThirdwebAccountType.PrivateKeyAccount:
                    var signedTx = await Sign(transaction);
                    hash = await rpc.SendRequestAsync<string>("eth_sendRawTransaction", signedTx).ConfigureAwait(false);
                    break;
                case ThirdwebAccountType.SmartAccount:
                case ThirdwebAccountType.ExternalAccount:
                    hash = await transaction.Wallet.SendTransaction(transaction.Input).ConfigureAwait(false);
                    break;
                default:
                    throw new NotImplementedException("Account type not supported");
            }
        }
        Utils.TrackTransaction(transaction, hash);
        return hash;
    }

    /// <summary>
    /// Sends the transaction and waits for the transaction receipt.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <returns>The transaction receipt.</returns>
    public static async Task<ThirdwebTransactionReceipt> SendAndWaitForTransactionReceipt(ThirdwebTransaction transaction)
    {
        var txHash = await Send(transaction).ConfigureAwait(false);
        return await WaitForTransactionReceipt(transaction.Wallet.Client, transaction.Input.ChainId.Value, txHash).ConfigureAwait(false);
    }

    /// <summary>
    /// Waits for the transaction receipt.
    /// </summary>
    /// <param name="client">The Thirdweb client.</param>
    /// <param name="chainId">The chain ID.</param>
    /// <param name="txHash">The transaction hash.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The transaction receipt.</returns>
    public static async Task<ThirdwebTransactionReceipt> WaitForTransactionReceipt(ThirdwebClient client, BigInteger chainId, string txHash, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(client.FetchTimeoutOptions.GetTimeout(TimeoutType.Other));

        var rpc = ThirdwebRPC.GetRpcInstance(client, chainId);
        ThirdwebTransactionReceipt receipt = null;

        try
        {
            do
            {
                receipt = await rpc.SendRequestAsync<ThirdwebTransactionReceipt>("eth_getTransactionReceipt", txHash).ConfigureAwait(false);
                if (receipt == null)
                {
                    await ThirdwebTask.Delay(100, cts.Token).ConfigureAwait(false);
                }
            } while (receipt == null && !cts.Token.IsCancellationRequested);

            if (receipt == null)
            {
                throw new Exception($"Transaction {txHash} not found within the timeout period.");
            }

            if (receipt.Status != null && receipt.Status.Value == 0)
            {
                throw new Exception($"Transaction {txHash} execution reverted.");
            }
        }
        catch (OperationCanceledException)
        {
            throw new Exception($"Transaction receipt polling for hash {txHash} was cancelled.");
        }

        return receipt;
    }

    /// <summary>
    /// Waits for the transaction hash given a thirdweb transaction id. Use WaitForTransactionReceipt if you have a transaction hash.
    /// </summary>
    /// <param name="client">The Thirdweb client.</param>
    /// <param name="txId">The thirdweb transaction id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The transaction hash.</returns>
    public static async Task<string> WaitForTransactionHash(ThirdwebClient client, string txId, CancellationToken cancellationToken = default)
    {
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client));
        }

        if (string.IsNullOrEmpty(txId))
        {
            throw new ArgumentException("Transaction id cannot be null or empty.", nameof(txId));
        }
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(client.FetchTimeoutOptions.GetTimeout(TimeoutType.Other));

        var api = client.Api;
        string hash = null;

        try
        {
            do
            {
                var resp = await api.GetTransactionByIdAsync(txId, cts.Token).ConfigureAwait(false);
                hash = resp?.Result?.TransactionHash;
                if (hash == null)
                {
                    await ThirdwebTask.Delay(100, cts.Token).ConfigureAwait(false);
                }
            } while (hash == null && !cts.Token.IsCancellationRequested);

            if (hash == null)
            {
                throw new Exception($"Transaction {txId} not found within the timeout period.");
            }
        }
        catch (OperationCanceledException)
        {
            throw new Exception($"Transaction hash polling for id {txId} was cancelled.");
        }

        return hash;
    }

    /// <summary>
    /// Converts the transaction to a zkSync transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <returns>The zkSync transaction.</returns>
    public static async Task<AccountAbstraction.ZkSyncAATransaction> ConvertToZkSyncTransaction(ThirdwebTransaction transaction)
    {
        return new AccountAbstraction.ZkSyncAATransaction
        {
            TxType = 0x71,
            From = new HexBigInteger(transaction.Input.From).Value,
            To = new HexBigInteger(transaction.Input.To).Value,
            GasLimit = transaction.Input.Gas.Value,
            GasPerPubdataByteLimit = transaction.Input.ZkSync?.GasPerPubdataByteLimit ?? await GetGasPerPubData(transaction).ConfigureAwait(false),
            MaxFeePerGas = transaction.Input.MaxFeePerGas?.Value ?? transaction.Input.GasPrice.Value,
            MaxPriorityFeePerGas = transaction.Input.MaxPriorityFeePerGas?.Value ?? 0,
            Paymaster = transaction.Input.ZkSync.Value.Paymaster,
            Nonce = transaction.Input.Nonce ?? new HexBigInteger(await GetNonce(transaction).ConfigureAwait(false)),
            Value = transaction.Input.Value?.Value ?? 0,
            Data = transaction.Input.Data?.HexToBytes() ?? Array.Empty<byte>(),
            FactoryDeps = transaction.Input.ZkSync.Value.FactoryDeps,
            PaymasterInput = transaction.Input.ZkSync.Value.PaymasterInput,
        };
    }
}
