using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Newtonsoft.Json;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.Hex.HexConvertors.Extensions;
using Newtonsoft.Json.Linq;

namespace Thirdweb
{
    public struct TotalCosts
    {
        public string ether;
        public BigInteger wei;
    }

    public class ThirdwebTransaction
    {
        public ThirdwebTransactionInput Input { get; private set; }

        private readonly ThirdwebClient _client;
        private readonly IThirdwebWallet _wallet;

        private ThirdwebTransaction(ThirdwebClient client, IThirdwebWallet wallet, ThirdwebTransactionInput txInput, BigInteger chainId)
        {
            Input = txInput;
            _client = client;
            _wallet = wallet;
            Input.ChainId = chainId.ToHexBigInteger();
        }

        public static async Task<ThirdwebTransaction> Create(ThirdwebClient client, IThirdwebWallet wallet, ThirdwebTransactionInput txInput, BigInteger chainId)
        {
            if (client == null)
            {
                throw new ArgumentException("Client must be provided", nameof(client));
            }

            if (wallet == null)
            {
                throw new ArgumentException("Wallet must be provided", nameof(wallet));
            }

            if (chainId == 0)
            {
                throw new ArgumentException("Invalid Chain ID", nameof(chainId));
            }

            if (txInput.To == null)
            {
                throw new ArgumentException("Transaction recipient (to) must be provided", nameof(txInput));
            }

            var address = await wallet.GetAddress();
            txInput.From ??= address;
            txInput.Data ??= "0x";

            if (address != txInput.From)
            {
                throw new ArgumentException("Transaction sender (from) must match wallet address", nameof(txInput));
            }

            return new ThirdwebTransaction(client, wallet, txInput, chainId);
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

        public ThirdwebTransaction SetMaxFeePerGas(BigInteger maxFeePerGas)
        {
            Input.MaxFeePerGas = maxFeePerGas.ToHexBigInteger();
            return this;
        }

        public ThirdwebTransaction SetMaxPriorityFeePerGas(BigInteger maxPriorityFeePerGas)
        {
            Input.MaxPriorityFeePerGas = maxPriorityFeePerGas.ToHexBigInteger();
            return this;
        }

        public ThirdwebTransaction SetChainId(BigInteger chainId)
        {
            Input.ChainId = chainId.ToHexBigInteger();
            return this;
        }

        public ThirdwebTransaction SetZkSyncOptions(ZkSyncOptions zkSyncOptions)
        {
            Input.ZkSync = zkSyncOptions;
            return this;
        }

        public static async Task<TotalCosts> EstimateGasCosts(ThirdwebTransaction transaction)
        {
            var gasPrice = transaction.Input.GasPrice?.Value ?? await EstimateGasPrice(transaction);
            var gasLimit = transaction.Input.Gas?.Value ?? await EstimateGasLimit(transaction);
            var gasCost = BigInteger.Multiply(gasLimit, gasPrice);
            return new TotalCosts { ether = gasCost.ToString().ToEth(18, false), wei = gasCost };
        }

        public static async Task<TotalCosts> EstimateTotalCosts(ThirdwebTransaction transaction)
        {
            var gasCosts = await EstimateGasCosts(transaction);
            var value = transaction.Input.Value?.Value ?? 0;
            return new TotalCosts { ether = (value + gasCosts.wei).ToString().ToEth(18, false), wei = value + gasCosts.wei };
        }

        public static async Task<BigInteger> EstimateGasPrice(ThirdwebTransaction transaction, bool withBump = true)
        {
            var rpc = ThirdwebRPC.GetRpcInstance(transaction._client, transaction.Input.ChainId.Value);
            var hex = new HexBigInteger(await rpc.SendRequestAsync<string>("eth_gasPrice"));
            return withBump ? hex.Value * 10 / 9 : hex.Value;
        }

        public static async Task<(BigInteger, BigInteger)> EstimateGasFees(ThirdwebTransaction transaction, bool withBump = true)
        {
            var rpc = ThirdwebRPC.GetRpcInstance(transaction._client, transaction.Input.ChainId.Value);
            var chainId = transaction.Input.ChainId.Value;

            if (Utils.IsZkSync(transaction.Input.ChainId.Value))
            {
                var fees = await rpc.SendRequestAsync<JToken>("zks_estimateFee", transaction.Input, "latest");
                var maxFee = fees["max_fee_per_gas"].ToObject<HexBigInteger>().Value;
                var maxPriorityFee = fees["max_priority_fee_per_gas"].ToObject<HexBigInteger>().Value;
                return withBump ? (maxFee * 10 / 5, maxPriorityFee * 10 / 5) : (maxFee, maxPriorityFee);
            }

            var gasPrice = await EstimateGasPrice(transaction, withBump);

            try
            {
                var block = await rpc.SendRequestAsync<JObject>("eth_getBlockByNumber", "latest", true);
                var maxPriorityFeePerGas = await rpc.SendRequestAsync<BigInteger?>("eth_maxPriorityFeePerGas");
                var baseBlockFee = block["result"]?["baseFeePerGas"]?.ToObject<BigInteger>() ?? new BigInteger(100);

                if (chainId == 42220 || chainId == 44787 || chainId == 62320)
                {
                    return (gasPrice, gasPrice);
                }

                if (!maxPriorityFeePerGas.HasValue)
                {
                    maxPriorityFeePerGas = new BigInteger(2);
                }

                var preferredMaxPriorityFeePerGas = maxPriorityFeePerGas.Value * 10 / 9;
                var maxFeePerGas = (baseBlockFee * 2) + preferredMaxPriorityFeePerGas;

                if (withBump)
                {
                    maxFeePerGas *= 10 / 9;
                }

                return (maxFeePerGas, preferredMaxPriorityFeePerGas);
            }
            catch
            {
                return (gasPrice, gasPrice);
            }
        }

        public static async Task<string> Simulate(ThirdwebTransaction transaction)
        {
            var rpc = ThirdwebRPC.GetRpcInstance(transaction._client, transaction.Input.ChainId.Value);
            var data = await rpc.SendRequestAsync<string>("eth_call", transaction.Input, "latest");
            return data;
        }

        public static async Task<BigInteger> EstimateGasLimit(ThirdwebTransaction transaction)
        {
            var rpc = ThirdwebRPC.GetRpcInstance(transaction._client, transaction.Input.ChainId.Value);

            if (Utils.IsZkSync(transaction.Input.ChainId.Value))
            {
                var hex = (await rpc.SendRequestAsync<JToken>("zks_estimateFee", transaction.Input, "latest"))["gas_limit"].ToString();
                return new HexBigInteger(hex).Value * 10 / 5;
            }

            if (transaction._wallet.AccountType == ThirdwebAccountType.SmartAccount)
            {
                var smartAccount = transaction._wallet as SmartWallet;
                return await smartAccount.EstimateUserOperationGas(transaction.Input, transaction.Input.ChainId.Value);
            }
            else
            {
                var hex = await rpc.SendRequestAsync<string>("eth_estimateGas", transaction.Input, "latest");
                return new HexBigInteger(hex).Value;
            }
        }

        public static async Task<BigInteger> GetNonce(ThirdwebTransaction transaction)
        {
            var rpc = ThirdwebRPC.GetRpcInstance(transaction._client, transaction.Input.ChainId.Value);
            return new HexBigInteger(await rpc.SendRequestAsync<string>("eth_getTransactionCount", transaction.Input.From, "latest")).Value;
        }

        private static async Task<BigInteger> GetGasPerPubData(ThirdwebTransaction transaction)
        {
            var rpc = ThirdwebRPC.GetRpcInstance(transaction._client, transaction.Input.ChainId.Value);
            var hex = (await rpc.SendRequestAsync<JToken>("zks_estimateFee", transaction.Input, "latest"))["gas_per_pubdata_limit"].ToString();
            return new HexBigInteger(hex).Value * 3 / 2;
        }

        public static async Task<string> Sign(ThirdwebTransaction transaction)
        {
            return await transaction._wallet.SignTransaction(transaction.Input);
        }

        public static async Task<string> Send(ThirdwebTransaction transaction)
        {
            if (transaction.Input.To == null)
            {
                throw new InvalidOperationException("Transaction recipient (to) must be provided");
            }

            if (transaction.Input.GasPrice != null && (transaction.Input.MaxFeePerGas != null || transaction.Input.MaxPriorityFeePerGas != null))
            {
                throw new InvalidOperationException("Transaction GasPrice and MaxFeePerGas/MaxPriorityFeePerGas cannot be set at the same time");
            }

            transaction.Input.From ??= await transaction._wallet.GetAddress();
            transaction.Input.Value ??= new HexBigInteger(0);
            transaction.Input.Data ??= "0x";
            transaction.Input.Gas ??= new HexBigInteger(await EstimateGasLimit(transaction));
            if (transaction.Input.GasPrice == null)
            {
                var (maxFeePerGas, maxPriorityFeePerGas) = await EstimateGasFees(transaction);
                transaction.Input.MaxFeePerGas ??= maxFeePerGas.ToHexBigInteger();
                transaction.Input.MaxPriorityFeePerGas ??= maxPriorityFeePerGas.ToHexBigInteger();
            }
            else
            {
                transaction.Input.MaxFeePerGas = null;
                transaction.Input.MaxPriorityFeePerGas = null;
            }

            var rpc = ThirdwebRPC.GetRpcInstance(transaction._client, transaction.Input.ChainId.Value);
            string hash;
            if (
                Utils.IsZkSync(transaction.Input.ChainId.Value)
                && transaction.Input.ZkSync.HasValue
                && transaction.Input.ZkSync.Value.Paymaster != 0
                && transaction.Input.ZkSync.Value.PaymasterInput != null
            )
            {
                var zkTx = await ConvertToZkSyncTransaction(transaction);
                var zkTxSigned = await EIP712.GenerateSignature_ZkSyncTransaction("zkSync", "2", transaction.Input.ChainId.Value, zkTx, transaction._wallet);
                hash = await rpc.SendRequestAsync<string>("eth_sendRawTransaction", zkTxSigned);
            }
            else
            {
                switch (transaction._wallet.AccountType)
                {
                    case ThirdwebAccountType.PrivateKeyAccount:
                        transaction.Input.Nonce ??= new HexBigInteger(await GetNonce(transaction));
                        var signedTx = await Sign(transaction);
                        hash = await rpc.SendRequestAsync<string>("eth_sendRawTransaction", signedTx);
                        break;
                    case ThirdwebAccountType.SmartAccount:
                    case ThirdwebAccountType.ExternalAccount:
                        hash = await transaction._wallet.SendTransaction(transaction.Input);
                        break;
                    default:
                        throw new NotImplementedException("Account type not supported");
                }
            }
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

        public static async Task<AccountAbstraction.ZkSyncAATransaction> ConvertToZkSyncTransaction(ThirdwebTransaction transaction)
        {
            var rpc = ThirdwebRPC.GetRpcInstance(transaction._client, transaction.Input.ChainId.Value);
            Console.WriteLine("Current TX: " + JsonConvert.SerializeObject(transaction.Input));
            return new AccountAbstraction.ZkSyncAATransaction
            {
                TxType = 0x71,
                From = new HexBigInteger(transaction.Input.From).Value,
                To = new HexBigInteger(transaction.Input.To).Value,
                GasLimit = transaction.Input.Gas.Value,
                GasPerPubdataByteLimit = transaction.Input.ZkSync?.GasPerPubdataByteLimit ?? await GetGasPerPubData(transaction),
                MaxFeePerGas = transaction.Input.MaxFeePerGas?.Value ?? transaction.Input.GasPrice.Value,
                MaxPriorityFeePerGas = transaction.Input.MaxPriorityFeePerGas?.Value ?? 0,
                Paymaster = transaction.Input.ZkSync.Value.Paymaster,
                Nonce = transaction.Input.Nonce ?? new HexBigInteger(await GetNonce(transaction)),
                Value = transaction.Input.Value?.Value ?? 0,
                Data = transaction.Input.Data?.HexToByteArray() ?? new byte[0],
                FactoryDeps = transaction.Input.ZkSync.Value.FactoryDeps,
                PaymasterInput = transaction.Input.ZkSync.Value.PaymasterInput
            };
        }
    }
}
