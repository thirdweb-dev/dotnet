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

    [Fact(Timeout = 120000)]
    public async Task Marketplace_DirectListings_TotalListings_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var totalListings = await contract.Marketplace_DirectListings_TotalListings();
        Assert.True(totalListings >= 0);
    }

    [Fact(Timeout = 120000)]
    public async Task Marketplace_DirectListings_GetAllListings_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var startId = BigInteger.Zero;
        var endId = 10;

        var listings = await contract.Marketplace_DirectListings_GetAllListings(startId, endId);
        Assert.NotNull(listings);
        Assert.True(listings.Count >= 0);
    }

    [Fact(Timeout = 120000)]
    public async Task Marketplace_DirectListings_GetAllValidListings_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var startId = BigInteger.Zero;
        var endId = 10;

        var listings = await contract.Marketplace_DirectListings_GetAllValidListings(startId, endId);
        Assert.NotNull(listings);
        Assert.True(listings.Count >= 0);
    }

    [Fact(Timeout = 120000)]
    public async Task Marketplace_DirectListings_GetListing_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var listingId = BigInteger.One;

        var listing = await contract.Marketplace_DirectListings_GetListing(listingId);
        Assert.NotNull(listing);
    }

    #endregion

    #region IEnglishAuctions

    [Fact(Timeout = 120000)]
    public async Task Marketplace_EnglishAuctions_GetAuction_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var auctionId = BigInteger.One;

        var auction = await contract.Marketplace_EnglishAuctions_GetAuction(auctionId);
        Assert.NotNull(auction);
    }

    [Fact(Timeout = 120000)]
    public async Task Marketplace_EnglishAuctions_GetAllAuctions_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var startId = BigInteger.Zero;
        var endId = BigInteger.Zero;

        var auctions = await contract.Marketplace_EnglishAuctions_GetAllAuctions(startId, endId);
        Assert.NotNull(auctions);
    }

    [Fact(Timeout = 120000)]
    public async Task Marketplace_EnglishAuctions_GetAllValidAuctions_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var startId = BigInteger.Zero;
        var endId = BigInteger.Zero;

        var auctions = await contract.Marketplace_EnglishAuctions_GetAllValidAuctions(startId, endId);
        Assert.NotNull(auctions);
        Assert.True(auctions.Count >= 0);
    }

    // [Fact(Timeout = 120000)]
    // public async Task Marketplace_EnglishAuctions_GetWinningBid_Success()
    // {
    //     var contract = await this.GetMarketplaceContract();
    //     var auctionId = BigInteger.One;

    //     var (bidder, currency, bidAmount) = await contract.Marketplace_EnglishAuctions_GetWinningBid(auctionId);
    //     Assert.NotNull(bidder);
    //     Assert.NotNull(currency);
    //     Assert.True(bidAmount >= 0);
    // }

    // [Fact(Timeout = 120000)]
    // public async Task Marketplace_EnglishAuctions_IsAuctionExpired_Success()
    // {
    //     var contract = await this.GetMarketplaceContract();
    //     var auctionId = BigInteger.One;

    //     _ = await contract.Marketplace_EnglishAuctions_IsAuctionExpired(auctionId);
    // }

    #endregion

    #region IOffers

    [Fact(Timeout = 120000)]
    public async Task Marketplace_Offers_GetOffer_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var offerId = BigInteger.One;

        var offer = await contract.Marketplace_Offers_GetOffer(offerId);
        Assert.NotNull(offer);
    }

    [Fact(Timeout = 120000)]
    public async Task Marketplace_Offers_GetAllOffers_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var startId = BigInteger.Zero;
        var endId = BigInteger.Zero;

        var offers = await contract.Marketplace_Offers_GetAllOffers(startId, endId);
        Assert.NotNull(offers);
        Assert.True(offers.Count >= 0);
    }

    [Fact(Timeout = 120000)]
    public async Task Marketplace_Offers_GetAllValidOffers_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var startId = BigInteger.Zero;
        var endId = BigInteger.Zero;

        var offers = await contract.Marketplace_Offers_GetAllValidOffers(startId, endId);
        Assert.NotNull(offers);
        Assert.True(offers.Count >= 0);
    }

    #endregion
}
