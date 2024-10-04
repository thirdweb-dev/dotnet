using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb;

#region Common

public enum TokenType : byte
{
    ERC721 = 0,
    ERC1155 = 1,
    ERC20 = 2
}

public enum Status : byte
{
    UNSET = 0,
    CREATED = 1,
    COMPLETED = 2,
    CANCELLED = 3
}

#endregion

#region IDirectListings

[Struct("ListingParameters")]
public class ListingParameters
{
    [Parameter("address", "assetContract", 1)]
    public string AssetContract { get; set; }

    [Parameter("uint256", "tokenId", 2)]
    public BigInteger TokenId { get; set; }

    [Parameter("uint256", "quantity", 3)]
    public BigInteger Quantity { get; set; }

    [Parameter("address", "currency", 4)]
    public string Currency { get; set; }

    [Parameter("uint256", "pricePerToken", 5)]
    public BigInteger PricePerToken { get; set; }

    [Parameter("uint128", "startTimestamp", 6)]
    public BigInteger StartTimestamp { get; set; }

    [Parameter("uint128", "endTimestamp", 7)]
    public BigInteger EndTimestamp { get; set; }

    [Parameter("bool", "reserved", 8)]
    public bool Reserved { get; set; }
}

[Struct("Listing")]
public class Listing
{
    [Parameter("uint256", "listingId", 1)]
    public BigInteger ListingId { get; set; }

    [Parameter("uint256", "tokenId", 2)]
    public BigInteger TokenId { get; set; }

    [Parameter("uint256", "quantity", 3)]
    public BigInteger Quantity { get; set; }

    [Parameter("uint256", "pricePerToken", 4)]
    public BigInteger PricePerToken { get; set; }

    [Parameter("uint128", "startTimestamp", 5)]
    public BigInteger StartTimestamp { get; set; }

    [Parameter("uint128", "endTimestamp", 6)]
    public BigInteger EndTimestamp { get; set; }

    [Parameter("address", "listingCreator", 7)]
    public string ListingCreator { get; set; }

    [Parameter("address", "assetContract", 8)]
    public string AssetContract { get; set; }

    [Parameter("address", "currency", 9)]
    public string Currency { get; set; }

    [Parameter("uint8", "tokenType", 10)]
    public TokenType TokenTypeEnum { get; set; }

    [Parameter("uint8", "status", 11)]
    public Status StatusEnum { get; set; }

    [Parameter("bool", "reserved", 12)]
    public bool Reserved { get; set; }
}

#endregion

#region IEnglishAuctions

[Struct("AuctionParameters")]
public class AuctionParameters
{
    [Parameter("address", "assetContract", 1)]
    public string AssetContract { get; set; }

    [Parameter("uint256", "tokenId", 2)]
    public BigInteger TokenId { get; set; }

    [Parameter("uint256", "quantity", 3)]
    public BigInteger Quantity { get; set; }

    [Parameter("address", "currency", 4)]
    public string Currency { get; set; }

    [Parameter("uint256", "minimumBidAmount", 5)]
    public BigInteger MinimumBidAmount { get; set; }

    [Parameter("uint256", "buyoutBidAmount", 6)]
    public BigInteger BuyoutBidAmount { get; set; }

    [Parameter("uint64", "timeBufferInSeconds", 7)]
    public ulong TimeBufferInSeconds { get; set; }

    [Parameter("uint64", "bidBufferBps", 8)]
    public ulong BidBufferBps { get; set; }

    [Parameter("uint64", "startTimestamp", 9)]
    public ulong StartTimestamp { get; set; }

    [Parameter("uint64", "endTimestamp", 10)]
    public ulong EndTimestamp { get; set; }
}

[Struct("Auction")]
public class Auction
{
    [Parameter("uint256", "auctionId", 1)]
    public BigInteger AuctionId { get; set; }

    [Parameter("uint256", "tokenId", 2)]
    public BigInteger TokenId { get; set; }

    [Parameter("uint256", "quantity", 3)]
    public BigInteger Quantity { get; set; }

    [Parameter("uint256", "minimumBidAmount", 4)]
    public BigInteger MinimumBidAmount { get; set; }

    [Parameter("uint256", "buyoutBidAmount", 5)]
    public BigInteger BuyoutBidAmount { get; set; }

    [Parameter("uint64", "timeBufferInSeconds", 6)]
    public ulong TimeBufferInSeconds { get; set; }

    [Parameter("uint64", "bidBufferBps", 7)]
    public ulong BidBufferBps { get; set; }

    [Parameter("uint64", "startTimestamp", 8)]
    public ulong StartTimestamp { get; set; }

    [Parameter("uint64", "endTimestamp", 9)]
    public ulong EndTimestamp { get; set; }

    [Parameter("address", "auctionCreator", 10)]
    public string AuctionCreator { get; set; }

    [Parameter("address", "assetContract", 11)]
    public string AssetContract { get; set; }

    [Parameter("address", "currency", 12)]
    public string Currency { get; set; }

    [Parameter("uint8", "tokenType", 13)]
    public TokenType TokenTypeEnum { get; set; }

    [Parameter("uint8", "status", 14)]
    public Status StatusEnum { get; set; }
}

[Struct("Bid")]
public class Bid
{
    [Parameter("uint256", "auctionId", 1)]
    public BigInteger AuctionId { get; set; }

    [Parameter("address", "bidder", 2)]
    public string Bidder { get; set; }

    [Parameter("uint256", "bidAmount", 3)]
    public BigInteger BidAmount { get; set; }
}

[Struct("AuctionPayoutStatus")]
public class AuctionPayoutStatus
{
    [Parameter("bool", "paidOutAuctionTokens", 1)]
    public bool PaidOutAuctionTokens { get; set; }

    [Parameter("bool", "paidOutBidAmount", 2)]
    public bool PaidOutBidAmount { get; set; }
}

#endregion

#region IOffers

[Struct("OfferParams")]
public class OfferParams
{
    [Parameter("address", "assetContract", 1)]
    public string AssetContract { get; set; }

    [Parameter("uint256", "tokenId", 2)]
    public BigInteger TokenId { get; set; }

    [Parameter("uint256", "quantity", 3)]
    public BigInteger Quantity { get; set; }

    [Parameter("address", "currency", 4)]
    public string Currency { get; set; }

    [Parameter("uint256", "totalPrice", 5)]
    public BigInteger TotalPrice { get; set; }

    [Parameter("uint256", "expirationTimestamp", 6)]
    public BigInteger ExpirationTimestamp { get; set; }
}

[Struct("Offer")]
public class Offer
{
    [Parameter("uint256", "offerId", 1)]
    public BigInteger OfferId { get; set; }

    [Parameter("uint256", "tokenId", 2)]
    public BigInteger TokenId { get; set; }

    [Parameter("uint256", "quantity", 3)]
    public BigInteger Quantity { get; set; }

    [Parameter("uint256", "totalPrice", 4)]
    public BigInteger TotalPrice { get; set; }

    [Parameter("uint256", "expirationTimestamp", 5)]
    public BigInteger ExpirationTimestamp { get; set; }

    [Parameter("address", "offeror", 6)]
    public string Offeror { get; set; }

    [Parameter("address", "assetContract", 7)]
    public string AssetContract { get; set; }

    [Parameter("address", "currency", 8)]
    public string Currency { get; set; }

    [Parameter("uint8", "tokenType", 9)]
    public TokenType TokenTypeEnum { get; set; }

    [Parameter("uint8", "status", 10)]
    public Status StatusEnum { get; set; }
}

#endregion
