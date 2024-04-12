using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Eth.Transactions;
using Newtonsoft.Json;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Thirdweb
{
    public struct TotalCosts
    {
        public string ether;
        public BigInteger wei;
    }

    public class ThirdwebTransaction
    {
        public TransactionInput Input { get; private set; }

        private readonly ThirdwebClient _client;
        private readonly IThirdwebWallet _wallet;

        private ThirdwebTransaction(ThirdwebClient client, IThirdwebWallet wallet, TransactionInput txInput, BigInteger chainId)
        {
            Input = txInput;
            _client = client;
            _wallet = wallet;
            Input.ChainId = chainId.ToHexBigInteger();
        }

        public static async Task<ThirdwebTransaction> Create(ThirdwebClient client, IThirdwebWallet wallet, TransactionInput txInput, BigInteger chainId)
        {
            txInput.From ??= await wallet.GetAddress();
            return await wallet.GetAddress() != txInput.From
                ? throw new ArgumentException("Transaction sender (from) must match wallet address")
                : client == null
                    ? throw new ArgumentNullException(nameof(client))
                    : wallet == null
                        ? throw new ArgumentNullException(nameof(wallet))
                        : chainId == 0
                            ? throw new ArgumentException("Invalid Chain ID")
                            : new ThirdwebTransaction(client, wallet, txInput, chainId);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(Input);
        }

        public ThirdwebTransaction SetTo(string to)
        {
            Input.To = to;
            return this;
        }

        public ThirdwebTransaction SetData(string data)
        {
            Input.Data = data;
            return this;
        }

        public ThirdwebTransaction SetValue(BigInteger weiValue)
        {
            Input.Value = weiValue.ToHexBigInteger();
            return this;
        }

        public ThirdwebTransaction SetGasLimit(BigInteger gas)
        {
            Input.Gas = gas.ToHexBigInteger();
            return this;
        }

        public ThirdwebTransaction SetGasPrice(BigInteger gasPrice)
        {
            Input.GasPrice = gasPrice.ToHexBigInteger();
            return this;
        }

        public ThirdwebTransaction SetNonce(BigInteger nonce)
        {
            Input.Nonce = nonce.ToHexBigInteger();
            return this;
        }

        public static async Task<TotalCosts> EstimateTotalCosts(ThirdwebTransaction transaction)
        {
            var gasPrice = transaction.Input.GasPrice?.Value ?? await EstimateGasPrice(transaction);
            var gasLimit = transaction.Input.Gas?.Value ?? await EstimateGasLimit(transaction, true);
            var gasCost = BigInteger.Multiply(gasLimit, gasPrice);
            var gasCostWithValue = BigInteger.Add(gasCost, transaction.Input.Value?.Value ?? 0);
            return new TotalCosts { ether = gasCostWithValue.ToString().ToEth(18, false), wei = gasCostWithValue };
        }

        public static async Task<BigInteger> EstimateGasPrice(ThirdwebTransaction transaction, bool withBump = true)
        {
            {
                var rpc = ThirdwebRPC.GetRpcInstance(transaction._client, transaction.Input.ChainId.Value);
                var hex = new HexBigInteger(await rpc.SendRequestAsync<string>("eth_gasPrice"));
                return withBump ? hex.Value * 10 / 9 : hex.Value;
            }
        }

        public static async Task<BigInteger> Simulate(ThirdwebTransaction transaction)
        {
            return await EstimateGasLimit(transaction, false);
        }

        public static async Task<BigInteger> EstimateGasLimit(ThirdwebTransaction transaction, bool overrideBalance = true)
        {
            var rpc = ThirdwebRPC.GetRpcInstance(transaction._client, transaction.Input.ChainId.Value);
            var from = transaction.Input.From;
            var hex = overrideBalance
                ? await rpc.SendRequestAsync<string>(
                    "eth_estimateGas",
                    transaction.Input,
                    "latest",
                    new Dictionary<string, Dictionary<string, string>>()
                    {
                        {
                            from,
                            new() { { "balance", "0xFFFFFFFFFFFFFFFFFFFF" } }
                        }
                    }
                )
                : await rpc.SendRequestAsync<string>("eth_estimateGas", transaction.Input, "latest");

            return new HexBigInteger(hex).Value;
        }

        public static async Task<string> Send(ThirdwebTransaction transaction)
        {
            if (transaction.Input.To == null)
            {
                throw new ArgumentException("To address must be provided");
            }

            transaction.Input.From ??= await transaction._wallet.GetAddress();
            transaction.Input.Value ??= new HexBigInteger(0);
            transaction.Input.Data ??= "0x";
            transaction.Input.GasPrice ??= new HexBigInteger(await EstimateGasPrice(transaction));
            transaction.Input.MaxFeePerGas = null;
            transaction.Input.MaxPriorityFeePerGas = null;
            transaction.Input.Gas ??= new HexBigInteger(await EstimateGasLimit(transaction));

            var rpc = ThirdwebRPC.GetRpcInstance(transaction._client, transaction.Input.ChainId.Value);
            string hash;
            switch (transaction._wallet.AccountType)
            {
                case ThirdwebAccountType.PrivateKeyAccount:
                    transaction.Input.Nonce ??= new HexBigInteger(await rpc.SendRequestAsync<string>("eth_getTransactionCount", await transaction._wallet.GetAddress(), "latest"));
                    var signedTx = await transaction._wallet.SignTransaction(transaction.Input, transaction.Input.ChainId.Value);
                    hash = await rpc.SendRequestAsync<string>("eth_sendRawTransaction", signedTx);
                    break;
                case ThirdwebAccountType.SmartAccount:
                    var smartAccount = transaction._wallet as SmartWallet;
                    hash = await smartAccount.SendTransaction(transaction.Input);
                    break;
                default:
                    throw new NotImplementedException("Account type not supported");
            }
            Console.WriteLine($"Transaction hash: {hash}");
            return hash;
        }

        public static async Task<TransactionReceipt> SendAndWaitForTransactionReceipt(ThirdwebTransaction transaction)
        {
            var txHash = await Send(transaction);
            return await WaitForTransactionReceipt(transaction._client, transaction.Input.ChainId.Value, txHash);
        }

        public static async Task<TransactionReceipt> WaitForTransactionReceipt(ThirdwebClient client, BigInteger chainId, string txHash, CancellationToken cancellationToken = default)
        {
            var rpc = ThirdwebRPC.GetRpcInstance(client, chainId);
            var receipt = await rpc.SendRequestAsync<TransactionReceipt>("eth_getTransactionReceipt", txHash).ConfigureAwait(false);
            while (receipt == null)
            {
                if (cancellationToken != CancellationToken.None)
                {
                    await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                }
                else
                {
                    await Task.Delay(1000, CancellationToken.None).ConfigureAwait(false);
                }

                receipt = await rpc.SendRequestAsync<TransactionReceipt>("eth_getTransactionReceipt", txHash).ConfigureAwait(false);
            }

            if (receipt.Failed())
            {
                throw new Exception($"Transaction {txHash} execution reverted.");
            }

            var userOpEvent = receipt.DecodeAllEvents<AccountAbstraction.UserOperationEventEventDTO>();
            if (userOpEvent != null && userOpEvent.Count > 0 && userOpEvent[0].Event.Success == false)
            {
                var revertReasonEvent = receipt.DecodeAllEvents<AccountAbstraction.UserOperationRevertReasonEventDTO>();
                if (revertReasonEvent != null && revertReasonEvent.Count > 0)
                {
                    var revertReason = revertReasonEvent[0].Event.RevertReason;
                    var revertReasonString = new FunctionCallDecoder().DecodeFunctionErrorMessage(revertReason.ToHex(true));
                    throw new Exception($"Transaction {txHash} execution silently reverted: {revertReasonString}");
                }
                else
                {
                    throw new Exception($"Transaction {txHash} execution silently reverted with no reason string");
                }
            }

            return receipt;
        }
    }
}
