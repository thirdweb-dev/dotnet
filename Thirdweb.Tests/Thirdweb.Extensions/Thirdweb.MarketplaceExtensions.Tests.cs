using System.Numerics;

namespace Thirdweb.Tests.Extensions;

public class MarketplaceExtensionsTests : BaseTests
{
    private readonly string _marketplaceContractAddress = "0xc9671F631E8313D53ec0b5358e1a499c574fCe6A";

    private readonly BigInteger _chainId = 421614;

    public MarketplaceExtensionsTests(ITestOutputHelper output)
        : base(output) { }

    private async Task<IThirdwebWallet> GetSmartWallet()
    {
        var privateKeyWallet = await PrivateKeyWallet.Generate(this.Client);
        return await SmartWallet.Create(personalWallet: privateKeyWallet, chainId: 421614);
    }

    private async Task<ThirdwebContract> GetMarketplaceContract()
    {
        return await ThirdwebContract.Create(this.Client, this._marketplaceContractAddress, this._chainId);
    }

    #region IDirectListings

    #endregion

    #region IEnglishAuctions

    #endregion

    #region IOffers

    #endregion
}
