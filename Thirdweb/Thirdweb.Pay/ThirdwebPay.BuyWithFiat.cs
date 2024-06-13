using System;
using System.Threading.Tasks;

namespace Thirdweb.Pay
{
    public partial class ThirdwebPay
    {
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
}
