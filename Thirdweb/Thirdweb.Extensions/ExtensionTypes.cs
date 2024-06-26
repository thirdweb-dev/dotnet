using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Newtonsoft.Json;

namespace Thirdweb
{
    #region Common

    public class ContractMetadata
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }
    }

    #endregion

    #region Forwarder

    public class Forwarder_ForwardRequest
    {
        [Parameter("address", "from", 1)]
        [JsonProperty("from")]
        public string From { get; set; }

        [Parameter("address", "to", 2)]
        [JsonProperty("to")]
        public string To { get; set; }

        [Parameter("uint256", "value", 3)]
        [JsonProperty("value")]
        public BigInteger Value { get; set; }

        [Parameter("uint256", "gas", 4)]
        [JsonProperty("gas")]
        public BigInteger Gas { get; set; }

        [Parameter("uint256", "nonce", 5)]
        [JsonProperty("nonce")]
        public BigInteger Nonce { get; set; }

        [Parameter("bytes", "data", 6)]
        [JsonProperty("data")]
        public string Data { get; set; }
    }

    #endregion

    #region NFT

    [Serializable]
    public enum NFTType
    {
        ERC721,
        ERC1155
    }

    [Serializable]
    public struct NFT
    {
        public NFTMetadata Metadata { get; set; }
        public string Owner { get; set; }
        public NFTType Type { get; set; }
        public BigInteger? Supply { get; set; }
        public BigInteger? QuantityOwned { get; set; }
    }

    [Serializable]
    public struct NFTMetadata
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("external_url")]
        public string External_url { get; set; }

        [JsonProperty("attributes")]
        public object Attributes { get; set; }
    }

    #endregion

    #region DropERC20

    public class DropERC20_ClaimCondition
    {
        [Parameter("uint256", "startTimestamp", 1)]
        [JsonProperty("startTimestamp")]
        public BigInteger StartTimestamp { get; set; }

        [Parameter("uint256", "maxClaimableSupply", 2)]
        [JsonProperty("maxClaimableSupply")]
        public BigInteger MaxClaimableSupply { get; set; }

        [Parameter("uint256", "supplyClaimed", 3)]
        [JsonProperty("supplyClaimed")]
        public BigInteger SupplyClaimed { get; set; }

        [Parameter("uint256", "quantityLimitPerWallet", 4)]
        [JsonProperty("quantityLimitPerWallet")]
        public BigInteger QuantityLimitPerWallet { get; set; }

        [Parameter("bytes32", "merkleRoot", 5)]
        [JsonProperty("merkleRoot")]
        public byte[] MerkleRoot { get; set; }

        [Parameter("uint256", "pricePerToken", 6)]
        [JsonProperty("pricePerToken")]
        public BigInteger PricePerToken { get; set; }

        [Parameter("address", "currency", 7)]
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [Parameter("string", "metadata", 8)]
        [JsonProperty("metadata")]
        public string Metadata { get; set; }
    }

    #endregion

    #region TokenERC20

    public class TokenERC20_MintRequest
    {
        [Parameter("address", "to", 1)]
        [JsonProperty("to")]
        public string To { get; set; }

        [Parameter("address", "primarySaleRecipient", 2)]
        [JsonProperty("primarySaleRecipient")]
        public string PrimarySaleRecipient { get; set; }

        [Parameter("uint256", "quantity", 3)]
        [JsonProperty("quantity")]
        public BigInteger Quantity { get; set; }

        [Parameter("uint256", "price", 4)]
        [JsonProperty("price")]
        public BigInteger Price { get; set; }

        [Parameter("address", "currency", 5)]
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [Parameter("uint128", "validityStartTimestamp", 6)]
        [JsonProperty("validityStartTimestamp")]
        public BigInteger ValidityStartTimestamp { get; set; }

        [Parameter("uint128", "validityEndTimestamp", 7)]
        [JsonProperty("validityEndTimestamp")]
        public BigInteger ValidityEndTimestamp { get; set; }

        [Parameter("bytes32", "uid", 8)]
        [JsonProperty("uid")]
        public byte[] Uid { get; set; }
    }

    #endregion

    #region TokenERC721

    public class TokenERC721_MintRequest
    {
        [Parameter("address", "to", 1)]
        [JsonProperty("to")]
        public string To { get; set; }

        [Parameter("address", "royaltyRecipient", 2)]
        [JsonProperty("royaltyRecipient")]
        public string RoyaltyRecipient { get; set; }

        [Parameter("uint256", "royaltyBps", 3)]
        [JsonProperty("royaltyBps")]
        public BigInteger RoyaltyBps { get; set; }

        [Parameter("address", "primarySaleRecipient", 4)]
        [JsonProperty("primarySaleRecipient")]
        public string PrimarySaleRecipient { get; set; }

        [Parameter("string", "uri", 5)]
        [JsonProperty("uri")]
        public string Uri { get; set; }

        [Parameter("uint256", "price", 6)]
        [JsonProperty("price")]
        public BigInteger Price { get; set; }

        [Parameter("address", "currency", 7)]
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [Parameter("uint128", "validityStartTimestamp", 8)]
        [JsonProperty("validityStartTimestamp")]
        public BigInteger ValidityStartTimestamp { get; set; }

        [Parameter("uint128", "validityEndTimestamp", 9)]
        [JsonProperty("validityEndTimestamp")]
        public BigInteger ValidityEndTimestamp { get; set; }

        [Parameter("bytes32", "uid", 10)]
        [JsonProperty("uid")]
        public byte[] Uid { get; set; }
    }

    #endregion

    #region TokenERC1155

    public class TokenERC1155_MintRequest
    {
        [Parameter("address", "to", 1)]
        [JsonProperty("to")]
        public string To { get; set; }

        [Parameter("address", "royaltyRecipient", 2)]
        [JsonProperty("royaltyRecipient")]
        public string RoyaltyRecipient { get; set; }

        [Parameter("uint256", "royaltyBps", 3)]
        [JsonProperty("royaltyBps")]
        public BigInteger RoyaltyBps { get; set; }

        [Parameter("address", "primarySaleRecipient", 4)]
        [JsonProperty("primarySaleRecipient")]
        public string PrimarySaleRecipient { get; set; }

        [Parameter("uint256", "tokenId", 5)]
        [JsonProperty("tokenId")]
        public BigInteger TokenId { get; set; }

        [Parameter("string", "uri", 6)]
        [JsonProperty("uri")]
        public string Uri { get; set; }

        [Parameter("uint256", "quantity", 7)]
        [JsonProperty("quantity")]
        public BigInteger Quantity { get; set; }

        [Parameter("uint256", "pricePerToken", 8)]
        [JsonProperty("pricePerToken")]
        public BigInteger PricePerToken { get; set; }

        [Parameter("address", "currency", 9)]
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [Parameter("uint128", "validityStartTimestamp", 10)]
        [JsonProperty("validityStartTimestamp")]
        public BigInteger ValidityStartTimestamp { get; set; }

        [Parameter("uint128", "validityEndTimestamp", 11)]
        [JsonProperty("validityEndTimestamp")]
        public BigInteger ValidityEndTimestamp { get; set; }

        [Parameter("bytes32", "uid", 12)]
        [JsonProperty("uid")]
        public byte[] Uid { get; set; }
    }

    #endregion
}
