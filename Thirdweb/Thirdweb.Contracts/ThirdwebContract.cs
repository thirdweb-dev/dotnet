using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;

namespace Thirdweb
{
    public class ThirdwebContract
    {
        internal ThirdwebClient Client { get; private set; }
        internal string Address { get; private set; }
        internal BigInteger Chain { get; private set; }
        internal string Abi { get; private set; }

        public ThirdwebContract(ThirdwebContractOptions options)
        {
            if (options.Client == null)
                throw new ArgumentException("Client must be provided");

            if (string.IsNullOrEmpty(options.Address))
                throw new ArgumentException("Address must be provided");

            if (options.Chain == 0)
                throw new ArgumentException("Chain must be provided");

            if (string.IsNullOrEmpty(options.Abi))
                throw new ArgumentException("Abi must be provided");

            Client = options.Client;
            Address = options.Address;
            Chain = options.Chain;
            Abi = options.Abi;
        }

        public static async Task<T> ReadContract<T>(ThirdwebContract contract, string method, params object[] parameters)
        {
            var rpc = ThirdwebRPC.GetRpcInstance(contract.Client, contract.Chain);

            var service = new Nethereum.Contracts.Contract(null, contract.Abi, contract.Address);
            var function = service.GetFunction(method);
            var data = function.GetData(parameters);

            var resultData = await rpc.SendRequestAsync<string>("eth_call", new { to = contract.Address, data = data, }, "latest");
            return function.DecodeTypeOutput<T>(resultData);
        }

        public static async Task<string> WriteContract(ThirdwebAccount account, ThirdwebContract contract, string method, params object[] parameters)
        {
            var rpc = ThirdwebRPC.GetRpcInstance(contract.Client, contract.Chain);

            var service = new Nethereum.Contracts.Contract(null, contract.Abi, contract.Address);
            var function = service.GetFunction(method);
            var data = function.GetData(parameters);

            var transaction = new TransactionInput
            {
                From = account.GetAddress(),
                To = contract.Address,
                Data = data,
            };

            // TODO: Implement 1559
            transaction.Gas = new HexBigInteger(await rpc.SendRequestAsync<string>("eth_estimateGas", transaction));
            transaction.GasPrice = new HexBigInteger(await rpc.SendRequestAsync<string>("eth_gasPrice"));
            transaction.Nonce = new HexBigInteger(await rpc.SendRequestAsync<string>("eth_getTransactionCount", account.GetAddress(), "latest"));

            string hash;
            if (account.Options.Type == WalletType.PrivateKey)
            {
                var signedTx = account.SignTransaction(transaction, contract.Chain);
                Console.WriteLine($"Signed transaction: {signedTx}");
                hash = await rpc.SendRequestAsync<string>("eth_sendRawTransaction", signedTx);
            }
            else
            {
                hash = await rpc.SendRequestAsync<string>("eth_sendTransaction", transaction);
            }
            return hash;
        }
    }
}
