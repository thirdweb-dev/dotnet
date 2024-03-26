using System.Numerics;

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
    }
}
