using System.Numerics;

namespace Thirdweb
{
    public class ThirdwebContractOptions
    {
        internal ThirdwebClient Client { get; private set; }
        internal string Address { get; private set; }
        internal BigInteger Chain { get; private set; }
        internal string Abi { get; private set; }

        public ThirdwebContractOptions(ThirdwebClient client, string address, BigInteger chain, string abi)
        {
            Client = client;
            Address = address;
            Chain = chain;
            Abi = abi;
        }
    }
}
