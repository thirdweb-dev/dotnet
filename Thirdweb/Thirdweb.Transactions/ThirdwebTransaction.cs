using System.Numerics;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.Hex.HexConvertors.Extensions;
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
    public ThirdwebTransactionInput Input { get; }

    private readonly IThirdwebWallet _wallet;

    private ThirdwebTransaction(IThirdwebWallet wallet, ThirdwebTransactionInput txInput)
    {
        this.Input = txInput;
        this._wallet = wallet;
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
        var rpc = ThirdwebRPC.GetRpcInstance(transaction._wallet.Client, transaction.Input.ChainId.Value);
        var hex = new HexBigInteger(await rpc.SendRequestAsync<string>("eth_gasPrice").ConfigureAwait(false));
        return withBump ? hex.Value * 10 / 9 : hex.Value;
    }

    /// <summary>
    /// Estimates the gas fees for the transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="withBump">Whether to include a bump in the gas fees.</param>
    /// <returns>The estimated maximum fee per gas and maximum priority fee per gas.</returns>
    public static async Task<(BigInteger maxFeePerGas, BigInteger maxPriorityFeePerGas)> EstimateGasFees(ThirdwebTransaction transaction, bool withBump = true)
    {
        var rpc = ThirdwebRPC.GetRpcInstance(transaction._wallet.Client, transaction.Input.ChainId.Value);
        var chainId = transaction.Input.ChainId.Value;

        if (Utils.IsZkSync(transaction.Input.ChainId.Value))
        {
            var fees = await rpc.SendRequestAsync<JToken>("zks_estimateFee", transaction.Input).ConfigureAwait(false);
            var maxFee = fees["max_fee_per_gas"].ToObject<HexBigInteger>().Value;
            var maxPriorityFee = fees["max_priority_fee_per_gas"].ToObject<HexBigInteger>().Value;
            return withBump ? (maxFee * 10 / 5, maxPriorityFee * 10 / 5) : (maxFee, maxPriorityFee);
        }

        var gasPrice = await EstimateGasPrice(transaction, withBump).ConfigureAwait(false);

        // Polygon Mainnet & Amoy
        if (chainId == (BigInteger)137 || chainId == (BigInteger)80002)
        {
            return (gasPrice * 3 / 2, gasPrice * 4 / 3);
        }

        // Celo Mainnet, Alfajores & Baklava
        if (chainId == (BigInteger)42220 || chainId == (BigInteger)44787 || chainId == (BigInteger)62320)
        {
            return (gasPrice, gasPrice);
        }

        try
        {
            var block = await rpc.SendRequestAsync<JObject>("eth_getBlockByNumber", "latest", true).ConfigureAwait(false);
            var baseBlockFee = block["baseFeePerGas"]?.ToObject<HexBigInteger>();
            var maxFeePerGas = baseBlockFee.Value * 2;
            var maxPriorityFeePerGas = ((await rpc.SendRequestAsync<HexBigInteger>("eth_maxPriorityFeePerGas").ConfigureAwait(false))?.Value) ?? maxFeePerGas / 2;

            if (maxPriorityFeePerGas > maxFeePerGas)
            {
                maxPriorityFeePerGas = maxFeePerGas / 2;
            }

            return (maxFeePerGas + (maxPriorityFeePerGas * 10 / 9), maxPriorityFeePerGas * 10 / 9);
        }
        catch
        {
            return (gasPrice, gasPrice);
        }
    }

    /// <summary>
    /// Simulates the transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <returns>The result of the simulation.</returns>
    public static async Task<string> Simulate(ThirdwebTransaction transaction)
    {
        var rpc = ThirdwebRPC.GetRpcInstance(transaction._wallet.Client, transaction.Input.ChainId.Value);
        return await rpc.SendRequestAsync<string>("eth_call", transaction.Input, "latest");
    }

    /// <summary>
    /// Estimates the gas limit for the transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <returns>The estimated gas limit.</returns>
    public static async Task<BigInteger> EstimateGasLimit(ThirdwebTransaction transaction)
    {
        var rpc = ThirdwebRPC.GetRpcInstance(transaction._wallet.Client, transaction.Input.ChainId.Value);

        if (Utils.IsZkSync(transaction.Input.ChainId.Value))
        {
            var hex = (await rpc.SendRequestAsync<JToken>("zks_estimateFee", transaction.Input).ConfigureAwait(false))["gas_limit"].ToString();
            return new HexBigInteger(hex).Value * 10 / 5;
        }

        if (transaction._wallet.AccountType == ThirdwebAccountType.SmartAccount)
        {
            var smartAccount = transaction._wallet as SmartWallet;
            return await smartAccount.EstimateUserOperationGas(transaction.Input).ConfigureAwait(false);
        }
        else if (transaction._wallet.AccountType == ThirdwebAccountType.ModularSmartAccount)
        {
            var smartAccount = transaction._wallet as ModularSmartWallet;
            return await smartAccount.EstimateUserOperationGas(transaction.Input).ConfigureAwait(false);
        }
        else
        {
            var hex = await rpc.SendRequestAsync<string>("eth_estimateGas", transaction.Input, "latest").ConfigureAwait(false);
            return new HexBigInteger(hex).Value * 10 / 7;
        }
    }

    /// <summary>
    /// Gets the nonce for the transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <returns>The nonce.</returns>
    public static async Task<BigInteger> GetNonce(ThirdwebTransaction transaction)
    {
        return await transaction._wallet.GetTransactionCount(chainId: transaction.Input.ChainId, blocktag: "pending").ConfigureAwait(false);
    }

    private static async Task<BigInteger> GetGasPerPubData(ThirdwebTransaction transaction)
    {
        var rpc = ThirdwebRPC.GetRpcInstance(transaction._wallet.Client, transaction.Input.ChainId.Value);
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
        return await transaction._wallet.SignTransaction(transaction.Input).ConfigureAwait(false);
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

        var rpc = ThirdwebRPC.GetRpcInstance(transaction._wallet.Client, transaction.Input.ChainId.Value);
        string hash;
        if (Utils.IsZkSync(transaction.Input.ChainId.Value) && transaction.Input.ZkSync.HasValue)
        {
            var zkTx = await ConvertToZkSyncTransaction(transaction).ConfigureAwait(false);
            var zkTxSigned = await EIP712.GenerateSignature_ZkSyncTransaction("zkSync", "2", transaction.Input.ChainId.Value, zkTx, transaction._wallet).ConfigureAwait(false);
            hash = await rpc.SendRequestAsync<string>("eth_sendRawTransaction", zkTxSigned).ConfigureAwait(false);
        }
        else
        {
            switch (transaction._wallet.AccountType)
            {
                case ThirdwebAccountType.PrivateKeyAccount:
                    var signedTx = await Sign(transaction);
                    hash = await rpc.SendRequestAsync<string>("eth_sendRawTransaction", signedTx).ConfigureAwait(false);
                    break;
                case ThirdwebAccountType.SmartAccount:
                case ThirdwebAccountType.ModularSmartAccount:
                case ThirdwebAccountType.ExternalAccount:
                    hash = await transaction._wallet.SendTransaction(transaction.Input).ConfigureAwait(false);
                    break;
                default:
                    throw new NotImplementedException("Account type not supported");
            }
        }
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
        return await WaitForTransactionReceipt(transaction._wallet.Client, transaction.Input.ChainId.Value, txHash).ConfigureAwait(false);
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
                    await ThirdwebTask.Delay(100, cancellationToken).ConfigureAwait(false);
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

            var userOpEvent = receipt.DecodeAllEvents<AccountAbstraction.UserOperationEventEventDTO>();
            if (userOpEvent != null && userOpEvent.Count > 0 && !userOpEvent[0].Event.Success)
            {
                var revertReasonEvent = receipt.DecodeAllEvents<AccountAbstraction.UserOperationRevertReasonEventDTO>();
                var postOpRevertReasonEvent = receipt.DecodeAllEvents<AccountAbstraction.PostOpRevertReasonEventDTO>();
                if (revertReasonEvent != null && revertReasonEvent.Count > 0)
                {
                    var revertReason = revertReasonEvent[0].Event.RevertReason;
                    var revertReasonString = new FunctionCallDecoder().DecodeFunctionErrorMessage(revertReason.ToHex(true));
                    throw new Exception($"Transaction {txHash} execution silently reverted: {revertReasonString}");
                }
                else if (postOpRevertReasonEvent != null && postOpRevertReasonEvent.Count > 0)
                {
                    var revertReason = postOpRevertReasonEvent[0].Event.RevertReason;
                    var revertReasonString = new FunctionCallDecoder().DecodeFunctionErrorMessage(revertReason.ToHex(true));
                    throw new Exception($"Transaction {txHash} execution silently reverted: {revertReasonString}");
                }
                else
                {
                    throw new Exception($"Transaction {txHash} execution silently reverted with no reason string");
                }
            }
        }
        catch (OperationCanceledException)
        {
            throw new Exception($"Transaction receipt polling for hash {txHash} was cancelled.");
        }

        return receipt;
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
            Data = transaction.Input.Data?.HexToByteArray() ?? Array.Empty<byte>(),
            FactoryDeps = transaction.Input.ZkSync.Value.FactoryDeps,
            PaymasterInput = transaction.Input.ZkSync.Value.PaymasterInput
        };
    }
}
