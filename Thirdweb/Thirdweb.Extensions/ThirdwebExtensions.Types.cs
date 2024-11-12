using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Newtonsoft.Json;

namespace Thirdweb;

#region Common

/// <summary>
/// Represents the result of a verification operation.
/// </summary>
[FunctionOutput]
public class VerifyResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the verification is valid.
    /// </summary>
    [Parameter("bool", "", 1)]
    [JsonProperty("isValid")]
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the address of the signer.
    /// </summary>
    [Parameter("address", "", 2)]
    [JsonProperty("signer")]
    public string Signer { get; set; }
}

/// <summary>
/// Represents the royalty information result.
/// </summary>
[FunctionOutput]
public class RoyaltyInfoResult
{
    /// <summary>
    /// Gets or sets the recipient address.
    /// </summary>
    [Parameter("address", "", 1)]
    [JsonProperty("recipient")]
    public string Recipient { get; set; }

    /// <summary>
    /// Gets or sets the basis points (bps) for royalty.
    /// </summary>
    [Parameter("uint256", "", 2)]
    [JsonProperty("bps")]
    public BigInteger Bps { get; set; }
}

/// <summary>
/// Represents the metadata of a contract.
/// </summary>
public class ContractMetadata
{
    /// <summary>
    /// Gets or sets the name of the contract.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the symbol of the contract.
    /// </summary>
    [JsonProperty("symbol")]
    public string Symbol { get; set; }

    /// <summary>
    /// Gets or sets the description of the contract.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the image URL of the contract.
    /// </summary>
    [JsonProperty("image")]
    public string Image { get; set; }
}

#endregion

#region Forwarder

/// <summary>
/// Represents a forward request for a forwarder.
/// </summary>
[Struct("ForwardRequest")]
public class Forwarder_ForwardRequest
{
    /// <summary>
    /// Gets or sets the address of the sender.
    /// </summary>
    [Parameter("address", "from", 1)]
    [JsonProperty("from")]
    public string From { get; set; }

    /// <summary>
    /// Gets or sets the address of the recipient.
    /// </summary>
    [Parameter("address", "to", 2)]
    [JsonProperty("to")]
    public string To { get; set; }

    /// <summary>
    /// Gets or sets the value to be transferred.
    /// </summary>
    [Parameter("uint256", "value", 3)]
    [JsonProperty("value")]
    public BigInteger Value { get; set; }

    /// <summary>
    /// Gets or sets the gas limit for the transaction.
    /// </summary>
    [Parameter("uint256", "gas", 4)]
    [JsonProperty("gas")]
    public BigInteger Gas { get; set; }

    /// <summary>
    /// Gets or sets the nonce for the transaction.
    /// </summary>
    [Parameter("uint256", "nonce", 5)]
    [JsonProperty("nonce")]
    public BigInteger Nonce { get; set; }

    /// <summary>
    /// Gets or sets the data to be sent with the transaction.
    /// </summary>
    [Parameter("bytes", "data", 6)]
    [JsonProperty("data")]
    public string Data { get; set; }
}

#endregion

#region NFT

/// <summary>
/// Represents the type of an NFT.
/// </summary>
[Serializable]
public enum NFTType
{
    ERC721,
    ERC1155
}

/// <summary>
/// Represents an NFT with metadata, owner, type, and supply information.
/// </summary>
[Serializable]
public struct NFT
{
    /// <summary>
    /// Gets or sets the metadata of the NFT.
    /// </summary>
    public NFTMetadata Metadata { get; set; }

    /// <summary>
    /// Gets or sets the owner address of the NFT. This is only applicable for ERC721 tokens.
    /// </summary>
    public string Owner { get; set; }

    /// <summary>
    /// Gets or sets the type of the NFT.
    /// </summary>
    public NFTType Type { get; set; }

    /// <summary>
    /// Gets or sets the supply of the NFT.
    /// </summary>
    public BigInteger? Supply { get; set; }

    /// <summary>
    /// Gets or sets the quantity owned by the user. This is only applicable for ERC1155 tokens.
    /// </summary>
    public BigInteger? QuantityOwned { get; set; }
}

/// <summary>
/// Represents the metadata of an NFT.
/// </summary>
[Serializable]
public struct NFTMetadata
{
    /// <summary>
    /// Gets or sets the ID of the NFT.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the URI of the NFT.
    /// </summary>
    [JsonProperty("uri")]
    public string Uri { get; set; }

    /// <summary>
    /// Gets or sets the description of the NFT.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the image URL of the NFT.
    /// </summary>
    [JsonProperty("image")]
    public string Image { get; set; }

    /// <summary>
    /// Gets or sets the name of the NFT.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the animation URL of the NFT.
    /// </summary>
    [JsonProperty("animation_url")]
    public string AnimationUrl { get; set; }

    /// <summary>
    /// Gets or sets the external URL of the NFT.
    /// </summary>
    [JsonProperty("external_url")]
    public string ExternalUrl { get; set; }

    /// <summary>
    /// Gets or sets the background color of the NFT.
    /// </summary>
    [JsonProperty("background_color")]
    public string BackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets the attributes of the NFT.
    /// </summary>
    [JsonProperty("attributes")]
    public object Attributes { get; set; }

    /// <summary>
    /// Gets or sets the properties of the NFT.
    /// </summary>
    [JsonProperty("properties")]
    public object Properties { get; set; }
}

#endregion

#region Drop

/// <summary>
/// Represents a claim condition for a drop.
/// </summary>
public class Drop_ClaimCondition
{
    /// <summary>
    /// Gets or sets the start timestamp of the claim condition.
    /// </summary>
    [Parameter("uint256", "startTimestamp", 1)]
    [JsonProperty("startTimestamp")]
    public BigInteger StartTimestamp { get; set; }

    /// <summary>
    /// Gets or sets the maximum claimable supply.
    /// </summary>
    [Parameter("uint256", "maxClaimableSupply", 2)]
    [JsonProperty("maxClaimableSupply")]
    public BigInteger MaxClaimableSupply { get; set; }

    /// <summary>
    /// Gets or sets the supply claimed so far.
    /// </summary>
    [Parameter("uint256", "supplyClaimed", 3)]
    [JsonProperty("supplyClaimed")]
    public BigInteger SupplyClaimed { get; set; }

    /// <summary>
    /// Gets or sets the quantity limit per wallet.
    /// </summary>
    [Parameter("uint256", "quantityLimitPerWallet", 4)]
    [JsonProperty("quantityLimitPerWallet")]
    public BigInteger QuantityLimitPerWallet { get; set; }

    /// <summary>
    /// Gets or sets the Merkle root for the claim condition.
    /// </summary>
    [Parameter("bytes32", "merkleRoot", 5)]
    [JsonProperty("merkleRoot")]
    public byte[] MerkleRoot { get; set; }

    /// <summary>
    /// Gets or sets the price per token for the claim condition.
    /// </summary>
    [Parameter("uint256", "pricePerToken", 6)]
    [JsonProperty("pricePerToken")]
    public BigInteger PricePerToken { get; set; }

    /// <summary>
    /// Gets or sets the currency address for the claim condition.
    /// </summary>
    [Parameter("address", "currency", 7)]
    [JsonProperty("currency")]
    public string Currency { get; set; }

    /// <summary>
    /// Gets or sets the metadata for the claim condition.
    /// </summary>
    [Parameter("string", "metadata", 8)]
    [JsonProperty("metadata")]
    public string Metadata { get; set; }
}

#endregion

#region TokenERC20

/// <summary>
/// Represents a mint request for an ERC20 token.
/// </summary>
[Struct("MintRequest")]
public class TokenERC20_MintRequest
{
    /// <summary>
    /// Gets or sets the address to mint the tokens to.
    /// </summary>
    [Parameter("address", "to", 1)]
    [JsonProperty("to")]
    public string To { get; set; }

    /// <summary>
    /// Gets or sets the primary sale recipient address.
    /// </summary>
    [Parameter("address", "primarySaleRecipient", 2)]
    [JsonProperty("primarySaleRecipient")]
    public string PrimarySaleRecipient { get; set; }

    /// <summary>
    /// Gets or sets the quantity of tokens to mint.
    /// </summary>
    [Parameter("uint256", "quantity", 3)]
    [JsonProperty("quantity")]
    public BigInteger Quantity { get; set; }

    /// <summary>
    /// Gets or sets the price of the tokens.
    /// </summary>
    [Parameter("uint256", "price", 4)]
    [JsonProperty("price")]
    public BigInteger Price { get; set; }

    /// <summary>
    /// Gets or sets the currency address.
    /// </summary>
    [Parameter("address", "currency", 5)]
    [JsonProperty("currency")]
    public string Currency { get; set; }

    /// <summary>
    /// Gets or sets the validity start timestamp.
    /// </summary>
    [Parameter("uint128", "validityStartTimestamp", 6)]
    [JsonProperty("validityStartTimestamp")]
    public BigInteger ValidityStartTimestamp { get; set; }

    /// <summary>
    /// Gets or sets the validity end timestamp.
    /// </summary>
    [Parameter("uint128", "validityEndTimestamp", 7)]
    [JsonProperty("validityEndTimestamp")]
    public BigInteger ValidityEndTimestamp { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the mint request.
    /// </summary>
    [Parameter("bytes32", "uid", 8)]
    [JsonProperty("uid")]
    public byte[] Uid { get; set; }
}

#endregion

#region TokenERC721

/// <summary>
/// Represents a mint request for an ERC721 token.
/// </summary>
[Struct("MintRequest")]
public class TokenERC721_MintRequest
{
    /// <summary>
    /// Gets or sets the address to mint the token to.
    /// </summary>
    [Parameter("address", "to", 1)]
    [JsonProperty("to")]
    public string To { get; set; }

    /// <summary>
    /// Gets or sets the royalty recipient address.
    /// </summary>
    [Parameter("address", "royaltyRecipient", 2)]
    [JsonProperty("royaltyRecipient")]
    public string RoyaltyRecipient { get; set; }

    /// <summary>
    /// Gets or sets the royalty basis points.
    /// </summary>
    [Parameter("uint256", "royaltyBps", 3)]
    [JsonProperty("royaltyBps")]
    public BigInteger RoyaltyBps { get; set; }

    /// <summary>
    /// Gets or sets the primary sale recipient address.
    /// </summary>
    [Parameter("address", "primarySaleRecipient", 4)]
    [JsonProperty("primarySaleRecipient")]
    public string PrimarySaleRecipient { get; set; }

    /// <summary>
    /// Gets or sets the URI of the token.
    /// </summary>
    [Parameter("string", "uri", 5)]
    [JsonProperty("uri")]
    public string Uri { get; set; }

    /// <summary>
    /// Gets or sets the price of the token.
    /// </summary>
    [Parameter("uint256", "price", 6)]
    [JsonProperty("price")]
    public BigInteger Price { get; set; }

    /// <summary>
    /// Gets or sets the currency address.
    /// </summary>
    [Parameter("address", "currency", 7)]
    [JsonProperty("currency")]
    public string Currency { get; set; }

    /// <summary>
    /// Gets or sets the validity start timestamp.
    /// </summary>
    [Parameter("uint128", "validityStartTimestamp", 8)]
    [JsonProperty("validityStartTimestamp")]
    public BigInteger ValidityStartTimestamp { get; set; }

    /// <summary>
    /// Gets or sets the validity end timestamp.
    /// </summary>
    [Parameter("uint128", "validityEndTimestamp", 9)]
    [JsonProperty("validityEndTimestamp")]
    public BigInteger ValidityEndTimestamp { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the mint request.
    /// </summary>
    [Parameter("bytes32", "uid", 10)]
    [JsonProperty("uid")]
    public byte[] Uid { get; set; }
}

#endregion

#region TokenERC1155

/// <summary>
/// Represents a mint request for an ERC1155 token.
/// </summary>
[Struct("MintRequest")]
public class TokenERC1155_MintRequest
{
    /// <summary>
    /// Gets or sets the address to mint the token to.
    /// </summary>
    [Parameter("address", "to", 1)]
    [JsonProperty("to")]
    public string To { get; set; }

    /// <summary>
    /// Gets or sets the royalty recipient address.
    /// </summary>
    [Parameter("address", "royaltyRecipient", 2)]
    [JsonProperty("royaltyRecipient")]
    public string RoyaltyRecipient { get; set; }

    /// <summary>
    /// Gets or sets the royalty basis points.
    /// </summary>
    [Parameter("uint256", "royaltyBps", 3)]
    [JsonProperty("royaltyBps")]
    public BigInteger RoyaltyBps { get; set; }

    /// <summary>
    /// Gets or sets the primary sale recipient address.
    /// </summary>
    [Parameter("address", "primarySaleRecipient", 4)]
    [JsonProperty("primarySaleRecipient")]
    public string PrimarySaleRecipient { get; set; }

    /// <summary>
    /// Gets or sets the token ID.
    /// </summary>
    [Parameter("uint256", "tokenId", 5)]
    [JsonProperty("tokenId")]
    public BigInteger? TokenId { get; set; }

    /// <summary>
    /// Gets or sets the URI of the token.
    /// </summary>
    [Parameter("string", "uri", 6)]
    [JsonProperty("uri")]
    public string Uri { get; set; }

    /// <summary>
    /// Gets or sets the quantity of tokens to mint.
    /// </summary>
    [Parameter("uint256", "quantity", 7)]
    [JsonProperty("quantity")]
    public BigInteger Quantity { get; set; }

    /// <summary>
    /// Gets or sets the price per token.
    /// </summary>
    [Parameter("uint256", "pricePerToken", 8)]
    [JsonProperty("pricePerToken")]
    public BigInteger PricePerToken { get; set; }

    /// <summary>
    /// Gets or sets the currency address.
    /// </summary>
    [Parameter("address", "currency", 9)]
    [JsonProperty("currency")]
    public string Currency { get; set; }

    /// <summary>
    /// Gets or sets the validity start timestamp.
    /// </summary>
    [Parameter("uint128", "validityStartTimestamp", 10)]
    [JsonProperty("validityStartTimestamp")]
    public BigInteger ValidityStartTimestamp { get; set; }

    /// <summary>
    /// Gets or sets the validity end timestamp.
    /// </summary>
    [Parameter("uint128", "validityEndTimestamp", 11)]
    [JsonProperty("validityEndTimestamp")]
    public BigInteger ValidityEndTimestamp { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the mint request.
    /// </summary>
    [Parameter("bytes32", "uid", 12)]
    [JsonProperty("uid")]
    public byte[] Uid { get; set; }
}

#endregion
