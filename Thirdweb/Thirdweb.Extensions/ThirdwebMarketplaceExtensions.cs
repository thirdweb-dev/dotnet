using System.Numerics;

namespace Thirdweb;

public static class ThirdwebMarketplaceExtensions
{
    #region IDirectListings

    /// <summary>
    /// Creates a new direct listing for selling NFTs at a fixed price.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="wallet">The wallet used for the transaction.</param>
    /// <param name="parameters">The parameters of the listing to be created.</param>
    /// <param name="handleApprovals">Whether to handle token approvals automatically.</param>
    /// <returns>A task that represents the transaction receipt of the listing creation.</returns>
    public static async Task<ThirdwebTransactionReceipt> Marketplace_DirectListings_CreateListing(
        this ThirdwebContract contract,
        IThirdwebWallet wallet,
        ListingParameters parameters,
        bool handleApprovals = false
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

        if (handleApprovals)
        {
            var assetContractAddress = parameters.AssetContract;

            var prepTasks = new List<Task>();

            var assetContractTask = ThirdwebContract.Create(contract.Client, assetContractAddress, contract.Chain);
            prepTasks.Add(assetContractTask);

            var walletAddressTask = wallet.GetAddress();
            prepTasks.Add(walletAddressTask);

            await Task.WhenAll(prepTasks);

            var assetContract = assetContractTask.Result;
            var walletAddress = walletAddressTask.Result;

            TokenType assetType;
            if (await assetContract.SupportsInterface(Constants.IERC1155_INTERFACE_ID))
            {
                assetType = TokenType.ERC1155;
            }
            else if (await assetContract.SupportsInterface(Constants.IERC721_INTERFACE_ID))
            {
                assetType = TokenType.ERC721;
            }
            else
            {
                throw new ArgumentException("Asset contract does not support ERC1155 or ERC721 interface.");
            }

            if (assetType == TokenType.ERC721)
            {
                var tokenId = parameters.TokenId;
                var @operator = await assetContract.ERC721_GetApproved(tokenId);
                if (@operator != contract.Address)
                {
                    _ = await assetContract.ERC721_Approve(wallet, contract.Address, tokenId);
                }
            }
            else
            {
                var isApprovedForAll = await assetContract.ERC1155_IsApprovedForAll(walletAddress, contract.Address);
                if (!isApprovedForAll)
                {
                    _ = await assetContract.ERC1155_SetApprovalForAll(wallet, contract.Address, true);
                }
            }
        }

        return await contract.Write(wallet, "createListing", 0, parameters);
    }

    /// <summary>
    /// Updates an existing direct listing.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="wallet">The wallet used for the transaction.</param>
    /// <param name="listingId">The ID of the listing to update.</param>
    /// <param name="parameters">The updated parameters of the listing.</param>
    /// <returns>A task that represents the transaction receipt of the listing update.</returns>
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

    /// <summary>
    /// Cancels a direct listing.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="wallet">The wallet used for the transaction.</param>
    /// <param name="listingId">The ID of the listing to cancel.</param>
    /// <returns>A task that represents the transaction receipt of the listing cancellation.</returns>
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

    /// <summary>
    /// Approves a buyer to purchase from a reserved listing.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="wallet">The wallet used for the transaction.</param>
    /// <param name="listingId">The ID of the listing.</param>
    /// <param name="buyer">The address of the buyer to approve.</param>
    /// <param name="toApprove">Whether to approve or disapprove the buyer.</param>
    /// <returns>A task that represents the transaction receipt of the approval.</returns>
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

    /// <summary>
    /// Approves a currency for a direct listing.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="wallet">The wallet used for the transaction.</param>
    /// <param name="listingId">The ID of the listing.</param>
    /// <param name="currency">The address of the currency to approve.</param>
    /// <param name="pricePerTokenInCurrency">The price per token in the specified currency.</param>
    /// <returns>A task that represents the transaction receipt of the currency approval.</returns>
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

    /// <summary>
    /// Buys from a direct listing.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="wallet">The wallet used for the transaction.</param>
    /// <param name="listingId">The ID of the listing.</param>
    /// <param name="buyFor">The recipient address for the purchased NFTs.</param>
    /// <param name="quantity">The quantity of NFTs to buy.</param>
    /// <param name="currency">The currency to use for the purchase.</param>
    /// <param name="expectedTotalPrice">The expected total price to pay.</param>
    /// <param name="handleApprovals">Whether to handle token approvals automatically.</param>
    /// <returns>A task that represents the transaction receipt of the purchase.</returns>
    public static async Task<ThirdwebTransactionReceipt> Marketplace_DirectListings_BuyFromListing(
        this ThirdwebContract contract,
        IThirdwebWallet wallet,
        BigInteger listingId,
        string buyFor,
        BigInteger quantity,
        string currency,
        BigInteger expectedTotalPrice,
        bool handleApprovals = false
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
        else if (handleApprovals)
        {
            var tokenContractAddress = currency;

            var prepTasks = new List<Task>();

            var tokenContractTask = ThirdwebContract.Create(contract.Client, tokenContractAddress, contract.Chain);
            prepTasks.Add(tokenContractTask);

            var walletAddressTask = wallet.GetAddress();
            prepTasks.Add(walletAddressTask);

            await Task.WhenAll(prepTasks);

            var tokenContract = tokenContractTask.Result;
            var walletAddress = walletAddressTask.Result;

            var allowance = await tokenContract.ERC20_Allowance(walletAddress, contract.Address);
            if (allowance < expectedTotalPrice)
            {
                _ = await tokenContract.ERC20_Approve(wallet, contract.Address, expectedTotalPrice);
            }
        }

        return await contract.Write(wallet, "buyFromListing", value, listingId, buyFor, quantity, currency, expectedTotalPrice);
    }

    /// <summary>
    /// Gets the total number of direct listings created.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <returns>A task that represents the total number of direct listings.</returns>
    public static async Task<BigInteger> Marketplace_DirectListings_TotalListings(this ThirdwebContract contract)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return await contract.Read<BigInteger>("totalListings");
    }

    /// <summary>
    /// Gets all direct listings within a given range of IDs.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="startId">The start ID of the range.</param>
    /// <param name="endId">The end ID of the range.</param>
    /// <returns>A task that represents a list of listings within the range.</returns>
    public static async Task<List<Listing>> Marketplace_DirectListings_GetAllListings(this ThirdwebContract contract, BigInteger startId, BigInteger endId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return await contract.Read<List<Listing>>("getAllListings", startId, endId);
    }

    /// <summary>
    /// Gets all valid direct listings within a given range of IDs.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="startId">The start ID of the range.</param>
    /// <param name="endId">The end ID of the range.</param>
    /// <returns>A task that represents a list of valid listings within the range.</returns>
    public static async Task<List<Listing>> Marketplace_DirectListings_GetAllValidListings(this ThirdwebContract contract, BigInteger startId, BigInteger endId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return await contract.Read<List<Listing>>("getAllValidListings", startId, endId);
    }

    /// <summary>
    /// Gets a specific direct listing by its ID.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="listingId">The ID of the listing to fetch.</param>
    /// <returns>A task that represents the requested listing.</returns>
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

    /// <summary>
    /// Creates a new auction listing for NFTs.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="wallet">The wallet used for the transaction.</param>
    /// <param name="parameters">The parameters of the auction to be created.</param>
    /// <param name="handleApprovals">Whether to handle token approvals automatically.</param>
    /// <returns>A task that represents the transaction receipt of the auction creation.</returns>
    public static async Task<ThirdwebTransactionReceipt> Marketplace_EnglishAuctions_CreateAuction(
        this ThirdwebContract contract,
        IThirdwebWallet wallet,
        AuctionParameters parameters,
        bool handleApprovals = false
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

        if (handleApprovals)
        {
            var assetContractAddress = parameters.AssetContract;

            var prepTasks = new List<Task>();

            var assetContractTask = ThirdwebContract.Create(contract.Client, assetContractAddress, contract.Chain);
            prepTasks.Add(assetContractTask);

            var walletAddressTask = wallet.GetAddress();
            prepTasks.Add(walletAddressTask);

            await Task.WhenAll(prepTasks);

            var assetContract = assetContractTask.Result;
            var walletAddress = walletAddressTask.Result;

            TokenType assetType;
            if (await assetContract.SupportsInterface(Constants.IERC1155_INTERFACE_ID))
            {
                assetType = TokenType.ERC1155;
            }
            else if (await assetContract.SupportsInterface(Constants.IERC721_INTERFACE_ID))
            {
                assetType = TokenType.ERC721;
            }
            else
            {
                throw new ArgumentException("Asset contract does not support ERC1155 or ERC721 interface.");
            }

            if (assetType == TokenType.ERC721)
            {
                var tokenId = parameters.TokenId;
                var @operator = await assetContract.ERC721_GetApproved(tokenId);
                if (@operator != contract.Address)
                {
                    _ = await assetContract.ERC721_Approve(wallet, contract.Address, tokenId);
                }
            }
            else
            {
                var isApprovedForAll = await assetContract.ERC1155_IsApprovedForAll(walletAddress, contract.Address);
                if (!isApprovedForAll)
                {
                    _ = await assetContract.ERC1155_SetApprovalForAll(wallet, contract.Address, true);
                }
            }
        }

        return await contract.Write(wallet, "createAuction", 0, parameters);
    }

    /// <summary>
    /// Cancels an existing auction listing.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="wallet">The wallet used for the transaction.</param>
    /// <param name="auctionId">The ID of the auction to cancel.</param>
    /// <returns>A task that represents the transaction receipt of the auction cancellation.</returns>
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

    /// <summary>
    /// Collects the payout for a completed auction.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="wallet">The wallet used for the transaction.</param>
    /// <param name="auctionId">The ID of the auction for which to collect the payout.</param>
    /// <returns>A task that represents the transaction receipt of the auction payout collection.</returns>
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

    /// <summary>
    /// Collects the tokens from a completed auction.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="wallet">The wallet used for the transaction.</param>
    /// <param name="auctionId">The ID of the auction for which to collect the tokens.</param>
    /// <returns>A task that represents the transaction receipt of the auction token collection.</returns>
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

    /// <summary>
    /// Places a bid in an auction.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="wallet">The wallet used for the transaction.</param>
    /// <param name="auctionId">The ID of the auction to bid in.</param>
    /// <param name="bidAmount">The bid amount to place.</param>
    /// <param name="handleApprovals">Whether to handle token approvals automatically.</param>
    /// <returns>A task that represents the transaction receipt of the placed bid.</returns>
    public static async Task<ThirdwebTransactionReceipt> Marketplace_EnglishAuctions_BidInAuction(
        this ThirdwebContract contract,
        IThirdwebWallet wallet,
        BigInteger auctionId,
        BigInteger bidAmount,
        bool handleApprovals = false
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

        var auctionDetails = await contract.Marketplace_EnglishAuctions_GetAuction(auctionId);
        if (auctionDetails.Currency == Constants.NATIVE_TOKEN_ADDRESS)
        {
            value = bidAmount;
        }
        else if (handleApprovals)
        {
            var tokenContractAddress = auctionDetails.Currency;

            var prepTasks = new List<Task>();

            var tokenContractTask = ThirdwebContract.Create(contract.Client, tokenContractAddress, contract.Chain);
            prepTasks.Add(tokenContractTask);

            var walletAddressTask = wallet.GetAddress();
            prepTasks.Add(walletAddressTask);

            await Task.WhenAll(prepTasks);

            var tokenContract = tokenContractTask.Result;
            var walletAddress = walletAddressTask.Result;

            var allowance = await tokenContract.ERC20_Allowance(walletAddress, contract.Address);
            if (allowance < bidAmount)
            {
                _ = await tokenContract.ERC20_Approve(wallet, contract.Address, bidAmount);
            }
        }

        return await contract.Write(wallet, "bidInAuction", value, auctionId, bidAmount);
    }

    /// <summary>
    /// Checks whether the bid amount would make for a winning bid in an auction.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="auctionId">The ID of the auction.</param>
    /// <param name="bidAmount">The bid amount to check.</param>
    /// <returns>A task that represents a boolean indicating if the bid would be a winning bid.</returns>
    public static async Task<bool> Marketplace_EnglishAuctions_IsNewWinningBid(this ThirdwebContract contract, BigInteger auctionId, BigInteger bidAmount)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return await contract.Read<bool>("isNewWinningBid", auctionId, bidAmount);
    }

    /// <summary>
    /// Retrieves the details of a specific auction by its ID.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="auctionId">The ID of the auction to fetch.</param>
    /// <returns>A task that represents the requested auction details.</returns>
    public static async Task<Auction> Marketplace_EnglishAuctions_GetAuction(this ThirdwebContract contract, BigInteger auctionId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return await contract.Read<Auction>("getAuction", auctionId);
    }

    /// <summary>
    /// Gets all auctions within a given range of IDs.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="startId">The start ID of the range.</param>
    /// <param name="endId">The end ID of the range.</param>
    /// <returns>A task that represents a list of auctions within the range.</returns>
    public static async Task<List<Auction>> Marketplace_EnglishAuctions_GetAllAuctions(this ThirdwebContract contract, BigInteger startId, BigInteger endId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return await contract.Read<List<Auction>>("getAllAuctions", startId, endId);
    }

    /// <summary>
    /// Gets all valid auctions within a given range of IDs.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="startId">The start ID of the range.</param>
    /// <param name="endId">The end ID of the range.</param>
    /// <returns>A task that represents a list of valid auctions within the range.</returns>
    public static async Task<List<Auction>> Marketplace_EnglishAuctions_GetAllValidAuctions(this ThirdwebContract contract, BigInteger startId, BigInteger endId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return await contract.Read<List<Auction>>("getAllValidAuctions", startId, endId);
    }

    /// <summary>
    /// Gets the winning bid of a specific auction.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="auctionId">The ID of the auction to retrieve the winning bid from.</param>
    /// <returns>A task that represents the winning bid details (bidder, currency, bidAmount).</returns>
    public static async Task<(string bidder, string currency, BigInteger bidAmount)> Marketplace_EnglishAuctions_GetWinningBid(this ThirdwebContract contract, BigInteger auctionId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        var res = await contract.Read<List<object>>("getWinningBid", auctionId);
        return (res[0].ToString(), res[1].ToString(), (BigInteger)res[2]);
    }

    /// <summary>
    /// Checks whether an auction is expired.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="auctionId">The ID of the auction to check.</param>
    /// <returns>A task that represents a boolean indicating if the auction is expired.</returns>
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

    /// <summary>
    /// Makes an offer for NFTs.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="wallet">The wallet used for the transaction.</param>
    /// <param name="parameters">The parameters of the offer to make.</param>
    /// <param name="handleApprovals">Whether to handle token approvals automatically.</param>
    /// <returns>A task that represents the transaction receipt of the offer creation.</returns>
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

    /// <summary>
    /// Cancels an existing offer.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="wallet">The wallet used for the transaction.</param>
    /// <param name="offerId">The ID of the offer to cancel.</param>
    /// <returns>A task that represents the transaction receipt of the offer cancellation.</returns>
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

    /// <summary>
    /// Accepts an existing offer.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="wallet">The wallet used for the transaction.</param>
    /// <param name="offerId">The ID of the offer to accept.</param>
    /// <returns>A task that represents the transaction receipt of the offer acceptance.</returns>
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

    /// <summary>
    /// Retrieves the details of a specific offer by its ID.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="offerId">The ID of the offer to fetch.</param>
    /// <returns>A task that represents the requested offer details.</returns>
    public static async Task<Offer> Marketplace_Offers_GetOffer(this ThirdwebContract contract, BigInteger offerId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return await contract.Read<Offer>("getOffer", offerId);
    }

    /// <summary>
    /// Gets all offers within a given range of IDs.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="startId">The start ID of the range.</param>
    /// <param name="endId">The end ID of the range.</param>
    /// <returns>A task that represents a list of offers within the range.</returns>
    public static async Task<List<Offer>> Marketplace_Offers_GetAllOffers(this ThirdwebContract contract, BigInteger startId, BigInteger endId)
    {
        if (contract == null)
        {
            throw new ArgumentNullException(nameof(contract));
        }

        return await contract.Read<List<Offer>>("getAllOffers", startId, endId);
    }

    /// <summary>
    /// Gets all valid offers within a given range of IDs.
    /// </summary>
    /// <param name="contract">The contract instance.</param>
    /// <param name="startId">The start ID of the range.</param>
    /// <param name="endId">The end ID of the range.</param>
    /// <returns>A task that represents a list of valid offers within the range.</returns>
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
