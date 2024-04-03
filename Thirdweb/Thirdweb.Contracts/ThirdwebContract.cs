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

        public ThirdwebContract(ThirdwebClient client, string address, BigInteger chain, string abi)
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

            if (string.IsNullOrEmpty(abi))
            {
                throw new ArgumentException("Abi must be provided");
            }

            Client = client;
            Address = address;
            Chain = chain;
            Abi = abi;
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

        public static async Task<string> WriteContract(ThirdwebWallet wallet, ThirdwebContract contract, string method, BigInteger weiValue, params object[] parameters)
        {
            var rpc = ThirdwebRPC.GetRpcInstance(contract.Client, contract.Chain);

            var service = new Nethereum.Contracts.Contract(null, contract.Abi, contract.Address);
            var function = service.GetFunction(method);
            var data = function.GetData(parameters);

            var transaction = new TransactionInput
            {
                From = await wallet.GetAddress(),
                To = contract.Address,
                Data = data,
            };

            // TODO: Implement 1559
            transaction.Gas = new HexBigInteger(await rpc.SendRequestAsync<string>("eth_estimateGas", transaction));
            transaction.GasPrice = new HexBigInteger(await rpc.SendRequestAsync<string>("eth_gasPrice"));
            transaction.Value = new HexBigInteger(weiValue);

            string hash;
            if (wallet.ActiveAccount.AccountType is ThirdwebAccountType.PrivateKeyAccount)
            {
                transaction.Nonce = new HexBigInteger(await rpc.SendRequestAsync<string>("eth_getTransactionCount", wallet.GetAddress(), "latest"));
                var signedTx = wallet.SignTransaction(transaction, contract.Chain);
                Console.WriteLine($"Signed transaction: {signedTx}");
                hash = await rpc.SendRequestAsync<string>("eth_sendRawTransaction", signedTx);
            }
            else if (wallet.ActiveAccount.AccountType is ThirdwebAccountType.SmartAccount)
            {
                var smartAccount = wallet.ActiveAccount as SmartAccount;
                hash = await smartAccount.SendTransaction(transaction);
            }
            else
            {
                throw new NotImplementedException("Account type not supported");
            }
            Console.WriteLine($"Transaction hash: {hash}");
            return hash;
        }
    }
}
