using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;

namespace Thirdweb.Pay
{
    public partial class ThirdwebPay
    {
        public static async Task<string> BuyWithCrypto(ThirdwebClient client, IThirdwebWallet wallet, BuyWithCryptoQuoteResult buyWithCryptoQuote)
        {
            if (buyWithCryptoQuote.Approval != null)
            {
                var erc20ToApprove = await ThirdwebContract.Create(client, buyWithCryptoQuote.Approval.TokenAddress, buyWithCryptoQuote.Approval.ChainId);
                var currentAllowance = await erc20ToApprove.ERC20_Allowance(await wallet.GetAddress(), buyWithCryptoQuote.Approval.SpenderAddress);
                if (currentAllowance < BigInteger.Parse(buyWithCryptoQuote.Approval.AmountWei))
                {
                    _ = await erc20ToApprove.ERC20_Approve(wallet, buyWithCryptoQuote.Approval.SpenderAddress, BigInteger.Parse(buyWithCryptoQuote.Approval.AmountWei));
                }
            }

            var txInput = new ThirdwebTransactionInput()
            {
                From = buyWithCryptoQuote.TransactionRequest.From,
                To = buyWithCryptoQuote.TransactionRequest.To,
                Data = buyWithCryptoQuote.TransactionRequest.Data,
                Value = new HexBigInteger(BigInteger.Parse(buyWithCryptoQuote.TransactionRequest.Value)),
                Gas = new HexBigInteger(BigInteger.Parse(buyWithCryptoQuote.TransactionRequest.GasLimit)),
                GasPrice = new HexBigInteger(BigInteger.Parse(buyWithCryptoQuote.TransactionRequest.GasPrice)),
            };

            var tx = await ThirdwebTransaction.Create(client, wallet, txInput, buyWithCryptoQuote.TransactionRequest.ChainId);

            var hash = await ThirdwebTransaction.Send(tx);

            return hash;
        }
    }
}
