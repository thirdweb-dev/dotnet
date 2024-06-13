using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;

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

        public static async Task<TransactionReceipt> Transfer(this IThirdwebWallet wallet, ThirdwebClient client, BigInteger chainId, string toAddress, BigInteger weiAmount)
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

            if (string.IsNullOrEmpty(toAddress))
            {
                throw new ArgumentException(nameof(toAddress), "Recipient address cannot be null or empty.");
            }

            if (weiAmount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(weiAmount), "Amount must be 0 or greater.");
            }

            var txInput = new ThirdwebTransactionInput()
            {
                From = await wallet.GetAddress(),
                To = toAddress,
                Value = new HexBigInteger(weiAmount)
            };
            var tx = await ThirdwebTransaction.Create(client, wallet, txInput, chainId);
            return await ThirdwebTransaction.SendAndWaitForTransactionReceipt(tx);
        }

        // TODO: Tx Listener?
    }
}
