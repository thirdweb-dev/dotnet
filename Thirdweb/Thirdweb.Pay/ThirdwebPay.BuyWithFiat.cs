namespace Thirdweb.Pay;

/// <summary>
/// Provides methods for processing payments with cryptocurrency and fiat.
/// </summary>
public partial class ThirdwebPay
{
    /// <summary>
    /// Initiates a purchase using fiat currency through an on-ramp service.
    /// </summary>
    /// <param name="buyWithFiatQuote">The quote result containing the on-ramp link.</param>
    /// <returns>The on-ramp link for the fiat purchase.</returns>
    /// <exception cref="ArgumentException">Thrown if the on-ramp link is null or empty.</exception>
    public static string BuyWithFiat(BuyWithFiatQuoteResult buyWithFiatQuote)
    {
        if (string.IsNullOrEmpty(buyWithFiatQuote.OnRampLink))
        {
            throw new ArgumentException("On-ramp link cannot be null or empty.");
        }

        var onRampLink = buyWithFiatQuote.OnRampLink;

        return onRampLink;
    }
}
