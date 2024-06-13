using System.Numerics;
using Nethereum.Hex.HexTypes;

namespace Thirdweb
{
    public static class ThirdwebWalletExtensions
    {
        public static async Task<BigInteger> GetBalance(this IThirdwebWallet wallet, ThirdwebClient client, BigInteger chainId)
        {
            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (chainId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chainId), "Chain ID must be greater than 0.");
            }

            var rpc = ThirdwebRPC.GetRpcInstance(client, chainId);
            var balanceHex = await rpc.SendRequestAsync<string>("eth_getBalance", await wallet.GetAddress(), "latest");
            return new HexBigInteger(balanceHex).Value;
        }
    }
}
