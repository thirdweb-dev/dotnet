using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb;

#region Common

/// <summary>
/// Enumeration representing the type of tokens (ERC721, ERC1155, or ERC20).
/// </summary>
public enum TokenType : byte
{
    /// <summary>
    /// Represents an ERC721 token.
    /// </summary>
    ERC721 = 0,

    /// <summary>
    /// Represents an ERC1155 token.
    /// </summary>
    ERC1155 = 1,

    /// <summary>
    /// Represents an ERC20 token.
    /// </summary>
    ERC20 = 2
}

/// <summary>
/// Enumeration representing the status of an entity (unset, created, completed, or cancelled).
/// </summary>
public enum Status : byte
{
    /// <summary>
    /// The status is not set.
    /// </summary>
    UNSET = 0,

    /// <summary>
    /// The entity is created.
    /// </summary>
    CREATED = 1,

    /// <summary>
    /// The entity is completed.
    /// </summary>
    COMPLETED = 2,

    /// <summary>
    /// The entity is cancelled.
    /// </summary>
    CANCELLED = 3
}

#endregion

#region IDirectListings

/// <summary>
/// Represents the parameters for creating or updating a listing in the marketplace.
/// </summary>
[Struct("ListingParameters")]
public class ListingParameters
{
    /// <summary>
    /// The address of the smart contract of the NFTs being listed.
    /// </summary>
    [Parameter("address", "assetContract", 1)]
    public string AssetContract { get; set; }

    /// <summary>
    /// The tokenId of the NFTs being listed.
    /// </summary>
    [Parameter("uint256", "tokenId", 2)]
    public BigInteger TokenId { get; set; }

    /// <summary>
    /// The quantity of NFTs being listed.
    /// </summary>
    [Parameter("uint256", "quantity", 3)]
    public BigInteger Quantity { get; set; }

    /// <summary>
    /// The currency in which the price must be paid when buying the listed NFTs.
    /// </summary>
    [Parameter("address", "currency", 4)]
    public string Currency { get; set; }

    /// <summary>
    /// The price per token for the NFTs listed.
    /// </summary>
    [Parameter("uint256", "pricePerToken", 5)]
    public BigInteger PricePerToken { get; set; }

    /// <summary>
    /// The UNIX timestamp at and after which NFTs can be bought from the listing.
    /// </summary>
    [Parameter("uint128", "startTimestamp", 6)]
    public BigInteger StartTimestamp { get; set; }

    /// <summary>
    /// The UNIX timestamp after which NFTs cannot be bought from the listing.
    /// </summary>
    [Parameter("uint128", "endTimestamp", 7)]
    public BigInteger EndTimestamp { get; set; }

    /// <summary>
    /// Whether the listing is reserved to be bought from a specific set of buyers.
    /// </summary>
    [Parameter("bool", "reserved", 8)]
    public bool Reserved { get; set; }
}

/// <summary>
/// Represents a listing in the marketplace.
/// </summary>
[FunctionOutput]
public class Listing
{
    /// <summary>
    /// The unique ID of the listing.
    /// </summary>
    [Parameter("uint256", "listingId", 1)]
    public BigInteger ListingId { get; set; }

    /// <summary>
    /// The tokenId of the NFTs being listed.
    /// </summary>
    [Parameter("uint256", "tokenId", 2)]
    public BigInteger TokenId { get; set; }

    /// <summary>
    /// The quantity of NFTs being listed.
    /// </summary>
    [Parameter("uint256", "quantity", 3)]
    public BigInteger Quantity { get; set; }

    /// <summary>
    /// The price per token for the NFTs listed.
    /// </summary>
    [Parameter("uint256", "pricePerToken", 4)]
    public BigInteger PricePerToken { get; set; }

    /// <summary>
    /// The UNIX timestamp at and after which NFTs can be bought from the listing.
    /// </summary>
    [Parameter("uint128", "startTimestamp", 5)]
    public BigInteger StartTimestamp { get; set; }

    /// <summary>
    /// The UNIX timestamp after which NFTs cannot be bought from the listing.
    /// </summary>
    [Parameter("uint128", "endTimestamp", 6)]
    public BigInteger EndTimestamp { get; set; }

    /// <summary>
    /// The address of the listing creator.
    /// </summary>
    [Parameter("address", "listingCreator", 7)]
    public string ListingCreator { get; set; }

    /// <summary>
    /// The address of the smart contract of the NFTs being listed.
    /// </summary>
    [Parameter("address", "assetContract", 8)]
    public string AssetContract { get; set; }

    /// <summary>
    /// The currency in which the price must be paid when buying the listed NFTs.
    /// </summary>
    [Parameter("address", "currency", 9)]
    public string Currency { get; set; }

    /// <summary>
    /// The type of token being listed (ERC721 or ERC1155).
    /// </summary>
    [Parameter("uint8", "tokenType", 10)]
    public TokenType TokenTypeEnum { get; set; }

    /// <summary>
    /// The status of the listing (created, completed, or cancelled).
    /// </summary>
    [Parameter("uint8", "status", 11)]
    public Status StatusEnum { get; set; }

    /// <summary>
    /// Whether the listing is reserved for a specific set of buyers.
    /// </summary>
    [Parameter("bool", "reserved", 12)]
    public bool Reserved { get; set; }
}

#endregion

#region IEnglishAuctions

/// <summary>
/// Represents the parameters for creating or updating an auction.
/// </summary>
[Struct("AuctionParameters")]
public class AuctionParameters
{
    /// <summary>
    /// The address of the smart contract of the NFTs being auctioned.
    /// </summary>
    [Parameter("address", "assetContract", 1)]
    public string AssetContract { get; set; }

    /// <summary>
    /// The tokenId of the NFTs being auctioned.
    /// </summary>
    [Parameter("uint256", "tokenId", 2)]
    public BigInteger TokenId { get; set; }

    /// <summary>
    /// The quantity of NFTs being auctioned.
    /// </summary>
    [Parameter("uint256", "quantity", 3)]
    public BigInteger Quantity { get; set; }

    /// <summary>
    /// The currency in which the bid must be made.
    /// </summary>
    [Parameter("address", "currency", 4)]
    public string Currency { get; set; }

    /// <summary>
    /// The minimum bid amount for the auction.
    /// </summary>
    [Parameter("uint256", "minimumBidAmount", 5)]
    public BigInteger MinimumBidAmount { get; set; }

    /// <summary>
    /// The buyout bid amount to instantly purchase the NFTs and close the auction.
    /// </summary>
    [Parameter("uint256", "buyoutBidAmount", 6)]
    public BigInteger BuyoutBidAmount { get; set; }

    /// <summary>
    /// The buffer time in seconds to extend the auction expiration if a new bid is made.
    /// </summary>
    [Parameter("uint64", "timeBufferInSeconds", 7)]
    public ulong TimeBufferInSeconds { get; set; }

    /// <summary>
    /// The bid buffer in basis points to ensure a new bid must be a certain percentage higher than the current bid.
    /// </summary>
    [Parameter("uint64", "bidBufferBps", 8)]
    public ulong BidBufferBps { get; set; }

    /// <summary>
    /// The timestamp at and after which bids can be made to the auction.
    /// </summary>
    [Parameter("uint64", "startTimestamp", 9)]
    public ulong StartTimestamp { get; set; }

    /// <summary>
    /// The timestamp after which bids cannot be made to the auction.
    /// </summary>
    [Parameter("uint64", "endTimestamp", 10)]
    public ulong EndTimestamp { get; set; }
}

/// <summary>
/// Represents an auction in the marketplace.
/// </summary>
[FunctionOutput]
public class Auction
{
    /// <summary>
    /// The unique ID of the auction.
    /// </summary>
    [Parameter("uint256", "auctionId", 1)]
    public BigInteger AuctionId { get; set; }

    /// <summary>
    /// The tokenId of the NFTs being auctioned.
    /// </summary>
    [Parameter("uint256", "tokenId", 2)]
    public BigInteger TokenId { get; set; }

    /// <summary>
    /// The quantity of NFTs being auctioned.
    /// </summary>
    [Parameter("uint256", "quantity", 3)]
    public BigInteger Quantity { get; set; }

    /// <summary>
    /// The minimum bid amount for the auction.
    /// </summary>
    [Parameter("uint256", "minimumBidAmount", 4)]
    public BigInteger MinimumBidAmount { get; set; }

    /// <summary>
    /// The buyout bid amount to instantly purchase the NFTs and close the auction.
    /// </summary>
    [Parameter("uint256", "buyoutBidAmount", 5)]
    public BigInteger BuyoutBidAmount { get; set; }

    /// <summary>
    /// The buffer time in seconds to extend the auction expiration if a new bid is made.
    /// </summary>
    [Parameter("uint64", "timeBufferInSeconds", 6)]
    public ulong TimeBufferInSeconds { get; set; }

    /// <summary>
    /// The bid buffer in basis points to ensure a new bid must be a certain percentage higher than the current bid.
    /// </summary>
    [Parameter("uint64", "bidBufferBps", 7)]
    public ulong BidBufferBps { get; set; }

    /// <summary>
    /// The timestamp at and after which bids can be made to the auction.
    /// </summary>
    [Parameter("uint64", "startTimestamp", 8)]
    public ulong StartTimestamp { get; set; }

    /// <summary>
    /// The timestamp after which bids cannot be made to the auction.
    /// </summary>
    [Parameter("uint64", "endTimestamp", 9)]
    public ulong EndTimestamp { get; set; }

    /// <summary>
    /// The address of the auction creator.
    /// </summary>
    [Parameter("address", "auctionCreator", 10)]
    public string AuctionCreator { get; set; }

    /// <summary>
    /// The address of the smart contract of the NFTs being auctioned.
    /// </summary>
    [Parameter("address", "assetContract", 11)]
    public string AssetContract { get; set; }

    /// <summary>
    /// The currency in which the bid must be made.
    /// </summary>
    [Parameter("address", "currency", 12)]
    public string Currency { get; set; }

    /// <summary>
    /// The type of token being auctioned (ERC721 or ERC1155).
    /// </summary>
    [Parameter("uint8", "tokenType", 13)]
    public TokenType TokenTypeEnum { get; set; }

    /// <summary>
    /// The status of the auction (created, completed, or cancelled).
    /// </summary>
    [Parameter("uint8", "status", 14)]
    public Status StatusEnum { get; set; }
}

/// <summary>
/// Represents a bid in an auction.
/// </summary>
[Struct("Bid")]
public class Bid
{
    /// <summary>
    /// The unique ID of the auction for which the bid is placed.
    /// </summary>
    [Parameter("uint256", "auctionId", 1)]
    public BigInteger AuctionId { get; set; }

    /// <summary>
    /// The address of the bidder.
    /// </summary>
    [Parameter("address", "bidder", 2)]
    public string Bidder { get; set; }

    /// <summary>
    /// The bid amount for the auction.
    /// </summary>
    [Parameter("uint256", "bidAmount", 3)]
    public BigInteger BidAmount { get; set; }
}

/// <summary>
/// Represents the status of the payout for an auction.
/// </summary>
[Struct("AuctionPayoutStatus")]
public class AuctionPayoutStatus
{
    /// <summary>
    /// Whether the auction tokens have been paid out.
    /// </summary>
    [Parameter("bool", "paidOutAuctionTokens", 1)]
    public bool PaidOutAuctionTokens { get; set; }

    /// <summary>
    /// Whether the bid amount has been paid out.
    /// </summary>
    [Parameter("bool", "paidOutBidAmount", 2)]
    public bool PaidOutBidAmount { get; set; }
}

#endregion

#region IOffers

/// <summary>
/// Represents the parameters for making an offer on NFTs.
/// </summary>
[Struct("OfferParams")]
public class OfferParams
{
    /// <summary>
    /// The contract address of the NFTs for which the offer is being made.
    /// </summary>
    [Parameter("address", "assetContract", 1)]
    public string AssetContract { get; set; }

    /// <summary>
    /// The tokenId of the NFTs for which the offer is being made.
    /// </summary>
    [Parameter("uint256", "tokenId", 2)]
    public BigInteger TokenId { get; set; }

    /// <summary>
    /// The quantity of NFTs desired in the offer.
    /// </summary>
    [Parameter("uint256", "quantity", 3)]
    public BigInteger Quantity { get; set; }

    /// <summary>
    /// The currency offered in exchange for the NFTs.
    /// </summary>
    [Parameter("address", "currency", 4)]
    public string Currency { get; set; }

    /// <summary>
    /// The total price offered for the NFTs.
    /// </summary>
    [Parameter("uint256", "totalPrice", 5)]
    public BigInteger TotalPrice { get; set; }

    /// <summary>
    /// The UNIX timestamp after which the offer cannot be accepted.
    /// </summary>
    [Parameter("uint256", "expirationTimestamp", 6)]
    public BigInteger ExpirationTimestamp { get; set; }
}

/// <summary>
/// Represents an offer made on NFTs.
/// </summary>
[Struct("Offer")]
public class Offer
{
    /// <summary>
    /// The unique ID of the offer.
    /// </summary>
    [Parameter("uint256", "offerId", 1)]
    public BigInteger OfferId { get; set; }

    /// <summary>
    /// The tokenId of the NFTs for which the offer is being made.
    /// </summary>
    [Parameter("uint256", "tokenId", 2)]
    public BigInteger TokenId { get; set; }

    /// <summary>
    /// The quantity of NFTs desired in the offer.
    /// </summary>
    [Parameter("uint256", "quantity", 3)]
    public BigInteger Quantity { get; set; }

    /// <summary>
    /// The total price offered for the NFTs.
    /// </summary>
    [Parameter("uint256", "totalPrice", 4)]
    public BigInteger TotalPrice { get; set; }

    /// <summary>
    /// The UNIX timestamp after which the offer cannot be accepted.
    /// </summary>
    [Parameter("uint256", "expirationTimestamp", 5)]
    public BigInteger ExpirationTimestamp { get; set; }

    /// <summary>
    /// The address of the offeror.
    /// </summary>
    [Parameter("address", "offeror", 6)]
    public string Offeror { get; set; }

    /// <summary>
    /// The contract address of the NFTs for which the offer is made.
    /// </summary>
    [Parameter("address", "assetContract", 7)]
    public string AssetContract { get; set; }

    /// <summary>
    /// The currency offered in exchange for the NFTs.
    /// </summary>
    [Parameter("address", "currency", 8)]
    public string Currency { get; set; }

    /// <summary>
    /// The type of token being offered (ERC721, ERC1155, or ERC20).
    /// </summary>
    [Parameter("uint8", "tokenType", 9)]
    public TokenType TokenTypeEnum { get; set; }

    /// <summary>
    /// The status of the offer (created, completed, or cancelled).
    /// </summary>
    [Parameter("uint8", "status", 10)]
    public Status StatusEnum { get; set; }
}

#endregion
