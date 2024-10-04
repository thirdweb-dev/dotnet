using System.Numerics;

namespace Thirdweb;

public static class ThirdwebMarketplaceExtensions
{
    #region IDirectListings

    public static async Task<ThirdwebTransactionReceipt> Marketplace_DirectListings_CreateListing(this ThirdwebContract contract, IThirdwebWallet wallet, ListingParameters parameters)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        return await contract.Write(wallet, "createListing", 0, parameters);
    }

    public static async Task<ThirdwebTransactionReceipt> Marketplace_DirectListings_UpdateListing(
        this ThirdwebContract contract,
        IThirdwebWallet wallet,
        BigInteger listingId,
        ListingParameters parameters
    )
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        return await contract.Write(wallet, "updateListing", 0, listingId, parameters);
    }

    public static async Task<ThirdwebTransactionReceipt> Marketplace_DirectListings_CancelListing(this ThirdwebContract contract, IThirdwebWallet wallet, BigInteger listingId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        return await contract.Write(wallet, "cancelListing", 0, listingId);
    }

    public static async Task<ThirdwebTransactionReceipt> Marketplace_DirectListings_ApproveBuyerForListing(
        this ThirdwebContract contract,
        IThirdwebWallet wallet,
        BigInteger listingId,
        string buyer,
        bool toApprove
    )
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        return await contract.Write(wallet, "approveBuyerForListing", 0, listingId, buyer, toApprove);
    }

    public static async Task<ThirdwebTransactionReceipt> Marketplace_DirectListings_ApproveCurrencyForListing(
        this ThirdwebContract contract,
        IThirdwebWallet wallet,
        BigInteger listingId,
        string currency,
        BigInteger pricePerTokenInCurrency
    )
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        return await contract.Write(wallet, "approveCurrencyForListing", 0, listingId, currency, pricePerTokenInCurrency);
    }

    public static async Task<ThirdwebTransactionReceipt> Marketplace_DirectListings_BuyFromListing(
        this ThirdwebContract contract,
        IThirdwebWallet wallet,
        BigInteger listingId,
        string buyFor,
        BigInteger quantity,
        string currency,
        BigInteger expectedTotalPrice
    )
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        var value = BigInteger.Zero;

        if (currency == Constants.NATIVE_TOKEN_ADDRESS)
        {
            value = expectedTotalPrice;
        }

        return await contract.Write(wallet, "buyFromListing", value, listingId, buyFor, quantity, currency, expectedTotalPrice);
    }

    public static async Task<BigInteger> Marketplace_DirectListings_TotalListings(this ThirdwebContract contract)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return await contract.Read<BigInteger>("totalListings");
    }

    public static async Task<List<Listing>> Marketplace_DirectListings_GetAllListings(this ThirdwebContract contract, BigInteger startId, BigInteger endId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return await contract.Read<List<Listing>>("getAllListings", startId, endId);
    }

    public static async Task<List<Listing>> Marketplace_DirectListings_GetAllValidListings(this ThirdwebContract contract, BigInteger startId, BigInteger endId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return await contract.Read<List<Listing>>("getAllValidListings", startId, endId);
    }

    public static async Task<Listing> Marketplace_DirectListings_GetListing(this ThirdwebContract contract, BigInteger listingId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return await contract.Read<Listing>("getListing", listingId);
    }

    #endregion

    #region IEnglishAuctions

    public static async Task<ThirdwebTransactionReceipt> Marketplace_EnglishAuctions_CreateAuction(this ThirdwebContract contract, IThirdwebWallet wallet, AuctionParameters parameters)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        return await contract.Write(wallet, "createAuction", 0, parameters);
    }

    public static async Task<ThirdwebTransactionReceipt> Marketplace_EnglishAuctions_CancelAuction(this ThirdwebContract contract, IThirdwebWallet wallet, BigInteger auctionId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        return await contract.Write(wallet, "cancelAuction", 0, auctionId);
    }

    public static async Task<ThirdwebTransactionReceipt> Marketplace_EnglishAuctions_CollectAuctionPayout(this ThirdwebContract contract, IThirdwebWallet wallet, BigInteger auctionId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        return await contract.Write(wallet, "collectAuctionPayout", 0, auctionId);
    }

    // function collectAuctionTokens(uint256 _auctionId) external;
    public static async Task<ThirdwebTransactionReceipt> Marketplace_EnglishAuctions_CollectAuctionTokens(this ThirdwebContract contract, IThirdwebWallet wallet, BigInteger auctionId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        return await contract.Write(wallet, "collectAuctionTokens", 0, auctionId);
    }

    public static async Task<ThirdwebTransactionReceipt> Marketplace_EnglishAuctions_BidInAuction(this ThirdwebContract contract, IThirdwebWallet wallet, BigInteger auctionId, BigInteger bidAmount)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        var value = BigInteger.Zero;

        var auctionDetails = await contract.Marketplace_EnglishAuctions_GetAuction(auctionId);
        if (auctionDetails.Currency == Constants.NATIVE_TOKEN_ADDRESS)
        {
            value = bidAmount;
        }

        return await contract.Write(wallet, "bidInAuction", value, auctionId, bidAmount);
    }

    public static async Task<bool> Marketplace_EnglishAuctions_IsNewWinningBid(this ThirdwebContract contract, BigInteger auctionId, BigInteger bidAmount)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return await contract.Read<bool>("isNewWinningBid", auctionId, bidAmount);
    }

    public static async Task<Auction> Marketplace_EnglishAuctions_GetAuction(this ThirdwebContract contract, BigInteger auctionId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return await contract.Read<Auction>("getAuction", auctionId);
    }

    public static async Task<List<Auction>> Marketplace_EnglishAuctions_GetAllAuctions(this ThirdwebContract contract, BigInteger startId, BigInteger endId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return await contract.Read<List<Auction>>("getAllAuctions", startId, endId);
    }

    public static async Task<List<Auction>> Marketplace_EnglishAuctions_GetAllValidAuctions(this ThirdwebContract contract, BigInteger startId, BigInteger endId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return await contract.Read<List<Auction>>("getAllValidAuctions", startId, endId);
    }

    public static async Task<(string bidder, string currency, BigInteger bidAmount)> Marketplace_EnglishAuctions_GetWinningBid(this ThirdwebContract contract, BigInteger auctionId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        var res = await contract.Read<List<object>>("getWinningBid", auctionId);
        return (res[0].ToString(), res[1].ToString(), (BigInteger)res[2]);
    }

    public static async Task<bool> Marketplace_EnglishAuctions_IsAuctionExpired(this ThirdwebContract contract, BigInteger auctionId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return await contract.Read<bool>("isAuctionExpired", auctionId);
    }

    #endregion

    #region IOffers

    public static async Task<ThirdwebTransactionReceipt> Marketplace_Offers_MakeOffer(this ThirdwebContract contract, IThirdwebWallet wallet, OfferParams parameters, bool handleApprovals = false)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        var token = parameters.Currency;
        if (token == Constants.NATIVE_TOKEN_ADDRESS)
        {
            throw new ArgumentException("Native token is not supported for offers, please wrap it or use ERC20 to make an offer.", nameof(parameters));
        }

        if (handleApprovals)
        {
            var prepTasks = new List<Task>();

            var tokenContractTask = ThirdwebContract.Create(contract.Client, token, contract.Chain);
            prepTasks.Add(tokenContractTask);

            var walletAddressTask = wallet.GetAddress();
            prepTasks.Add(walletAddressTask);

            await Task.WhenAll(prepTasks);

            var tokenContract = tokenContractTask.Result;
            var walletAddress = walletAddressTask.Result;

            var allowance = await tokenContract.ERC20_Allowance(walletAddress, contract.Address);
            if (allowance < parameters.TotalPrice)
            {
                _ = await tokenContract.ERC20_Approve(wallet, contract.Address, parameters.Quantity);
            }
        }

        return await contract.Write(wallet, "makeOffer", 0, parameters);
    }

    public static async Task<ThirdwebTransactionReceipt> Marketplace_Offers_CancelOffer(this ThirdwebContract contract, IThirdwebWallet wallet, BigInteger offerId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        return await contract.Write(wallet, "cancelOffer", 0, offerId);
    }

    public static async Task<ThirdwebTransactionReceipt> Marketplace_Offers_AcceptOffer(this ThirdwebContract contract, IThirdwebWallet wallet, BigInteger offerId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        if (wallet == null)
        {
            throw new ArgumentNullException(nameof(wallet));
        }

        return await contract.Write(wallet, "acceptOffer", 0, offerId);
    }

    public static async Task<Offer> Marketplace_Offers_GetOffer(this ThirdwebContract contract, BigInteger offerId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return await contract.Read<Offer>("getOffer", offerId);
    }

    public static async Task<List<Offer>> Marketplace_Offers_GetAllOffers(this ThirdwebContract contract, BigInteger startId, BigInteger endId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return await contract.Read<List<Offer>>("getAllOffers", startId, endId);
    }

    public static async Task<List<Offer>> Marketplace_Offers_GetAllValidOffers(this ThirdwebContract contract, BigInteger startId, BigInteger endId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return await contract.Read<List<Offer>>("getAllValidOffers", startId, endId);
    }

    #endregion
}
