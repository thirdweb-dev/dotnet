﻿using System.Numerics;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;

namespace Thirdweb
{
    public class ThirdwebContract
    {
        internal ThirdwebClient Client { get; private set; }
        internal string Address { get; private set; }
        internal BigInteger Chain { get; private set; }
        internal string Abi { get; private set; }

        private static Dictionary<string, string> _contractAbiCache = new();

        private ThirdwebContract(ThirdwebClient client, string address, BigInteger chain, string abi)
        {
            Client = client;
            Address = address;
            Chain = chain;
            Abi = abi;
        }

        public static async Task<ThirdwebContract> Create(ThirdwebClient client, string address, BigInteger chain, string abi = null)
        {
            if (client == null)
            {
                throw new ArgumentException("Client must be provided");
            }

            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentException("Address must be provided");
            }

            if (chain == 0)
            {
                throw new ArgumentException("Chain must be provided");
            }

            abi ??= await FetchAbi(client, address, chain).ConfigureAwait(false);
            return new ThirdwebContract(client, address, chain, abi);
        }

        public static async Task<string> FetchAbi(ThirdwebClient client, string address, BigInteger chainId)
        {
            if (_contractAbiCache.TryGetValue($"{address}:{chainId}", out var cachedAbi))
            {
                return cachedAbi;
            }

            var url = $"https://contract.thirdweb.com/abi/{chainId}/{address}";
            var httpClient = client.HttpClient;
            var response = await httpClient.GetAsync(url).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            var abi = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            _contractAbiCache[$"{address}:{chainId}"] = abi;
            return abi;
        }

        public static async Task<T> Read<T>(ThirdwebContract contract, string method, params object[] parameters)
        {
            var rpc = ThirdwebRPC.GetRpcInstance(contract.Client, contract.Chain);

            var contractRaw = new Contract(null, contract.Abi, contract.Address);
            var function = GetFunctionMatchSignature(contractRaw, method, parameters);
            var data = function.GetData(parameters);

            var resultData = await rpc.SendRequestAsync<string>("eth_call", new { to = contract.Address, data = data, }, "latest").ConfigureAwait(false);

            return function.DecodeTypeOutput<T>(resultData);
        }

        public static async Task<ThirdwebTransaction> Prepare(IThirdwebWallet wallet, ThirdwebContract contract, string method, BigInteger weiValue, params object[] parameters)
        {
            var contractRaw = new Contract(null, contract.Abi, contract.Address);
            var function = GetFunctionMatchSignature(contractRaw, method, parameters);
            var data = function.GetData(parameters);
            var transaction = new ThirdwebTransactionInput
            {
                From = await wallet.GetAddress().ConfigureAwait(false),
                To = contract.Address,
                Data = data,
                Value = new HexBigInteger(weiValue),
            };

            return await ThirdwebTransaction.Create(contract.Client, wallet, transaction, contract.Chain).ConfigureAwait(false);
        }

        public static async Task<ThirdwebTransactionReceipt> Write(IThirdwebWallet wallet, ThirdwebContract contract, string method, BigInteger weiValue, params object[] parameters)
        {
            var thirdwebTx = await Prepare(wallet, contract, method, weiValue, parameters).ConfigureAwait(false);
            return await ThirdwebTransaction.SendAndWaitForTransactionReceipt(thirdwebTx).ConfigureAwait(false);
        }

        private static Function GetFunctionMatchSignature(Contract contract, string functionName, params object[] args)
        {
            var abi = contract.ContractBuilder.ContractABI;
            var functions = abi.Functions;
            var paramsCount = args?.Length ?? 0;
            foreach (var function in functions)
            {
                if (function.Name == functionName && function.InputParameters.Length == paramsCount)
                {
                    var sha = function.Sha3Signature;
                    return contract.GetFunctionBySignature(sha);
                }
            }
            return null;
        }
    }
}
