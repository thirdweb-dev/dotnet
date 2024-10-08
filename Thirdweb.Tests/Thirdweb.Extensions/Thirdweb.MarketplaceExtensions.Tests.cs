using System.Numerics;

namespace Thirdweb.Tests.Extensions;

public class MarketplaceExtensionsTests : BaseTests
{
    private readonly string _marketplaceContractAddress = "0xb80E83b73571e63b3581b68f20bFC9E97965F329";
    private readonly string _drop1155ContractAddress = "0x37116cAe5e152C1A7345AAB0EC286Ff6E97c0605";

    private readonly BigInteger _chainId = 421614;

    public MarketplaceExtensionsTests(ITestOutputHelper output)
        : base(output) { }

    private async Task<IThirdwebWallet> GetSmartWallet(int claimAmount)
    {
        var privateKeyWallet = await PrivateKeyWallet.Generate(this.Client);
        var smartWallet = await SmartWallet.Create(personalWallet: privateKeyWallet, chainId: 421614);

        if (claimAmount > 0)
        {
            var drop1155Contract = await ThirdwebContract.Create(this.Client, this._drop1155ContractAddress, this._chainId);
            var tokenId = 0;
            _ = await drop1155Contract.DropERC1155_Claim(smartWallet, await smartWallet.GetAddress(), tokenId, claimAmount);
        }

        return smartWallet;
    }

    private async Task<ThirdwebContract> GetMarketplaceContract()
    {
        return await ThirdwebContract.Create(this.Client, this._marketplaceContractAddress, this._chainId);
    }

    #region IDirectListings

    [Fact(Timeout = 120000)]
    public async Task Marketplace_DirectListings_CreateListing_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var wallet = await this.GetSmartWallet(1);

        var listingParams = new ListingParameters()
        {
            AssetContract = this._drop1155ContractAddress,
            TokenId = 0,
            Quantity = 1,
            Currency = Constants.NATIVE_TOKEN_ADDRESS,
            PricePerToken = 1,
            StartTimestamp = Utils.GetUnixTimeStampNow(),
            EndTimestamp = Utils.GetUnixTimeStampNow() + 3600,
            Reserved = false
        };

        var receipt = await contract.Marketplace_DirectListings_CreateListing(wallet, listingParams, true);

        Assert.NotNull(receipt);
        Assert.True(receipt.TransactionHash.Length == 66);

        var listingId = await contract.Marketplace_DirectListings_TotalListings() - 1;
        var listing = await contract.Marketplace_DirectListings_GetListing(listingId);

        Assert.NotNull(listing);
        Assert.Equal(listing.ListingId, listingId);
        Assert.Equal(listing.TokenId, listingParams.TokenId);
        Assert.Equal(listing.Quantity, listingParams.Quantity);
        Assert.Equal(listing.PricePerToken, listingParams.PricePerToken);
        Assert.True(listing.StartTimestamp >= listingParams.StartTimestamp);
        Assert.True(listing.EndTimestamp >= listingParams.EndTimestamp);
        Assert.Equal(listing.ListingCreator, await wallet.GetAddress());
        Assert.Equal(listing.AssetContract, listingParams.AssetContract);
        Assert.Equal(listing.Currency, listingParams.Currency);
        Assert.Equal(TokenType.ERC1155, listing.TokenTypeEnum);
        Assert.Equal(Status.CREATED, listing.StatusEnum);
        Assert.Equal(listing.Reserved, listingParams.Reserved);

        var isCurrencyApproved = await contract.Marketplace_DirectListings_IsCurrencyApprovedForListing(listingId, Constants.NATIVE_TOKEN_ADDRESS);
        Assert.True(isCurrencyApproved);

        var currencyPriceForListing = await contract.Marketplace_DirectListings_CurrencyPriceForListing(listingId, Constants.NATIVE_TOKEN_ADDRESS);
        Assert.Equal(currencyPriceForListing, listingParams.PricePerToken);
    }

    [Fact(Timeout = 120000)]
    public async Task Marketplace_DirectListings_UpdateListing_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var wallet = await this.GetSmartWallet(1);

        var originalTotal = await contract.Marketplace_DirectListings_TotalListings();

        var originalListing = new ListingParameters()
        {
            AssetContract = this._drop1155ContractAddress,
            TokenId = 0,
            Quantity = 1,
            Currency = Constants.NATIVE_TOKEN_ADDRESS,
            PricePerToken = 1,
            StartTimestamp = Utils.GetUnixTimeStampNow() + 1800,
            EndTimestamp = Utils.GetUnixTimeStampNow() + 3600,
            Reserved = false
        };

        var receipt = await contract.Marketplace_DirectListings_CreateListing(wallet, originalListing, true);
        Assert.NotNull(receipt);

        var listingId = await contract.Marketplace_DirectListings_TotalListings() - 1;
        Assert.True(listingId == originalTotal);

        var updatedListingParams = originalListing;
        updatedListingParams.PricePerToken = 2;

        var updatedReceipt = await contract.Marketplace_DirectListings_UpdateListing(wallet, listingId, updatedListingParams);
        Assert.NotNull(updatedReceipt);
        Assert.True(updatedReceipt.TransactionHash.Length == 66);

        var listing = await contract.Marketplace_DirectListings_GetListing(listingId);
        Assert.NotNull(listing);
        Assert.Equal(listing.PricePerToken, 2);
    }

    [Fact(Timeout = 120000)]
    public async Task Marketplace_DirectListings_CancelListing_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var wallet = await this.GetSmartWallet(1);

        var originalTotal = await contract.Marketplace_DirectListings_TotalListings();

        var originalListing = new ListingParameters()
        {
            AssetContract = this._drop1155ContractAddress,
            TokenId = 0,
            Quantity = 1,
            Currency = Constants.NATIVE_TOKEN_ADDRESS,
            PricePerToken = 1,
            StartTimestamp = Utils.GetUnixTimeStampNow() + 1800,
            EndTimestamp = Utils.GetUnixTimeStampNow() + 3600,
            Reserved = false
        };

        var receipt = await contract.Marketplace_DirectListings_CreateListing(wallet, originalListing, true);
        Assert.NotNull(receipt);

        var listingId = await contract.Marketplace_DirectListings_TotalListings() - 1;
        Assert.True(listingId == originalTotal);

        var removeReceipt = await contract.Marketplace_DirectListings_CancelListing(wallet, listingId);
        Assert.NotNull(removeReceipt);
        Assert.True(removeReceipt.TransactionHash.Length == 66);
    }

    [Fact(Timeout = 120000)]
    public async Task Marketplace_DirectListings_ApproveBuyerForListing()
    {
        var contract = await this.GetMarketplaceContract();
        var wallet = await this.GetSmartWallet(1);

        var reservedListing = new ListingParameters()
        {
            AssetContract = this._drop1155ContractAddress,
            TokenId = 0,
            Quantity = 1,
            Currency = Constants.NATIVE_TOKEN_ADDRESS,
            PricePerToken = 1,
            StartTimestamp = Utils.GetUnixTimeStampNow(),
            EndTimestamp = Utils.GetUnixTimeStampNow() + 3600,
            Reserved = true
        };

        var receipt = await contract.Marketplace_DirectListings_CreateListing(wallet, reservedListing, true);
        Assert.NotNull(receipt);
        Assert.True(receipt.TransactionHash.Length == 66);

        var listingId = await contract.Marketplace_DirectListings_TotalListings() - 1;

        var buyer = await PrivateKeyWallet.Generate(this.Client);
        var approveReceipt = await contract.Marketplace_DirectListings_ApproveBuyerForListing(wallet, listingId, await buyer.GetAddress(), true);
        Assert.NotNull(approveReceipt);
        Assert.True(approveReceipt.TransactionHash.Length == 66);

        var isApproved = await contract.Marketplace_DirectListings_IsBuyerApprovedForListing(listingId, await buyer.GetAddress());
        Assert.True(isApproved);
    }

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
