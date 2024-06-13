using System.Numerics;
using Nethereum.Hex.HexTypes;

namespace Thirdweb
{
    public static class ThirdwebWalletExtensions
    {
        public static async Task<BigInteger> GetBalance(this IThirdwebWallet wallet, ThirdwebClient client, BigInteger chainId)
        {
            var rpc = ThirdwebRPC.GetRpcInstance(client, chainId);
            var balanceHex = await rpc.SendRequestAsync<string>("eth_getBalance", await wallet.GetAddress(), "latest");
            return new HexBigInteger(balanceHex).Value;
        }
    }
}
