using System.Numerics;
using Nethereum.Hex.HexTypes;

namespace Thirdweb.Pay;

/// <summary>
/// Provides methods for processing payments with cryptocurrency.
/// </summary>
public partial class ThirdwebPay
{
    /// <summary>
    /// Initiates a cryptocurrency purchase using the provided wallet and quote.
    /// </summary>
    /// <param name="wallet">The wallet to use for the purchase.</param>
    /// <param name="buyWithCryptoQuote">The quote result containing transaction details.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the transaction hash.</returns>
    public static async Task<string> BuyWithCrypto(IThirdwebWallet wallet, BuyWithCryptoQuoteResult buyWithCryptoQuote)
    {
        if (buyWithCryptoQuote.Approval != null)
        {
            var erc20ToApprove = await ThirdwebContract.Create(wallet.Client, buyWithCryptoQuote.Approval.TokenAddress, buyWithCryptoQuote.Approval.ChainId);
            var currentAllowance = await erc20ToApprove.ERC20_Allowance(await wallet.GetAddress(), buyWithCryptoQuote.Approval.SpenderAddress);
            if (currentAllowance < BigInteger.Parse(buyWithCryptoQuote.Approval.AmountWei))
            {
                _ = await erc20ToApprove.ERC20_Approve(wallet, buyWithCryptoQuote.Approval.SpenderAddress, BigInteger.Parse(buyWithCryptoQuote.Approval.AmountWei));
            }
        }

        var txInput = new ThirdwebTransactionInput(chainId: buyWithCryptoQuote.TransactionRequest.ChainId)
        {
            To = buyWithCryptoQuote.TransactionRequest.To,
            Data = buyWithCryptoQuote.TransactionRequest.Data,
            Value = new HexBigInteger(BigInteger.Parse(buyWithCryptoQuote.TransactionRequest.Value)),
            Gas = new HexBigInteger(BigInteger.Parse(buyWithCryptoQuote.TransactionRequest.GasLimit)),
            GasPrice = new HexBigInteger(BigInteger.Parse(buyWithCryptoQuote.TransactionRequest.GasPrice)),
        };

        var tx = await ThirdwebTransaction.Create(wallet, txInput);

        var hash = await ThirdwebTransaction.Send(tx);

        return hash;
    }
}
