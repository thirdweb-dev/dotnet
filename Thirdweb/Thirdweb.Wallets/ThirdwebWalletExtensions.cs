using System.Numerics;
using Nethereum.Hex.HexTypes;

namespace Thirdweb
{
    public static class ThirdwebWalletExtensions
    {
        public static async Task<BigInteger> GetBalance(this IThirdwebWallet wallet, ThirdwebClient client, BigInteger chainId, string erc20ContractAddress = null)
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

            var address = await wallet.GetAddress().ConfigureAwait(false);

            if (erc20ContractAddress != null)
            {
                var erc20Contract = await ThirdwebContract.Create(client, erc20ContractAddress, chainId).ConfigureAwait(false);
                return await erc20Contract.ERC20_BalanceOf(address).ConfigureAwait(false);
            }

            var rpc = ThirdwebRPC.GetRpcInstance(client, chainId);
            var balanceHex = await rpc.SendRequestAsync<string>("eth_getBalance", address, "latest").ConfigureAwait(false);
            return new HexBigInteger(balanceHex).Value;
        }

        public static async Task<ThirdwebTransactionReceipt> Transfer(this IThirdwebWallet wallet, ThirdwebClient client, BigInteger chainId, string toAddress, BigInteger weiAmount)
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
                From = await wallet.GetAddress().ConfigureAwait(false),
                To = toAddress,
                Value = new HexBigInteger(weiAmount)
            };
            var tx = await ThirdwebTransaction.Create(client, wallet, txInput, chainId).ConfigureAwait(false);
            return await ThirdwebTransaction.SendAndWaitForTransactionReceipt(tx).ConfigureAwait(false);
        }

        // TODO: Tx Listener?
    }
}
