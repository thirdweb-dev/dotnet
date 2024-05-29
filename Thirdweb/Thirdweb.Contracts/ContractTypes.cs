using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Newtonsoft.Json;

namespace Thirdweb.Contracts.TokenERC20
{
    public partial class MintRequest : MintRequestBase { }

    public class MintRequestBase
    {
        [Parameter("address", "to", 1)]
        public virtual string To { get; set; }

        [Parameter("address", "primarySaleRecipient", 2)]
        public virtual string PrimarySaleRecipient { get; set; }

        [Parameter("uint256", "quantity", 3)]
        public virtual BigInteger Quantity { get; set; }

        [Parameter("uint256", "price", 4)]
        public virtual BigInteger Price { get; set; }

        [Parameter("address", "currency", 5)]
        public virtual string Currency { get; set; }

        [Parameter("uint128", "validityStartTimestamp", 6)]
        public virtual BigInteger ValidityStartTimestamp { get; set; }

        [Parameter("uint128", "validityEndTimestamp", 7)]
        public virtual BigInteger ValidityEndTimestamp { get; set; }

        [Parameter("bytes32", "uid", 8)]
        public virtual byte[] Uid { get; set; }
    }
}

namespace Thirdweb.Contracts.TokenERC721
{
    public partial class MintRequest : MintRequestBase { }

    public class MintRequestBase
    {
        [Parameter("address", "to", 1)]
        public virtual string To { get; set; }

        [Parameter("address", "royaltyRecipient", 2)]
        public virtual string RoyaltyRecipient { get; set; }

        [Parameter("uint256", "royaltyBps", 3)]
        public virtual BigInteger RoyaltyBps { get; set; }

        [Parameter("address", "primarySaleRecipient", 4)]
        public virtual string PrimarySaleRecipient { get; set; }

        [Parameter("string", "uri", 5)]
        public virtual string Uri { get; set; }

        [Parameter("uint256", "price", 6)]
        public virtual BigInteger Price { get; set; }

        [Parameter("address", "currency", 7)]
        public virtual string Currency { get; set; }

        [Parameter("uint128", "validityStartTimestamp", 8)]
        public virtual BigInteger ValidityStartTimestamp { get; set; }

        [Parameter("uint128", "validityEndTimestamp", 9)]
        public virtual BigInteger ValidityEndTimestamp { get; set; }

        [Parameter("bytes32", "uid", 10)]
        public virtual byte[] Uid { get; set; }
    }
}

namespace Thirdweb.Contracts.TokenERC1155
{
    public partial class MintRequest : MintRequestBase { }

    public class MintRequestBase
    {
        [Parameter("address", "to", 1)]
        public virtual string To { get; set; }

        [Parameter("address", "royaltyRecipient", 2)]
        public virtual string RoyaltyRecipient { get; set; }

        [Parameter("uint256", "royaltyBps", 3)]
        public virtual BigInteger RoyaltyBps { get; set; }

        [Parameter("address", "primarySaleRecipient", 4)]
        public virtual string PrimarySaleRecipient { get; set; }

        [Parameter("uint256", "tokenId", 5)]
        public virtual BigInteger TokenId { get; set; }

        [Parameter("string", "uri", 6)]
        public virtual string Uri { get; set; }

        [Parameter("uint256", "quantity", 7)]
        public virtual BigInteger Quantity { get; set; }

        [Parameter("uint256", "pricePerToken", 8)]
        public virtual BigInteger PricePerToken { get; set; }

        [Parameter("address", "currency", 9)]
        public virtual string Currency { get; set; }

        [Parameter("uint128", "validityStartTimestamp", 10)]
        public virtual BigInteger ValidityStartTimestamp { get; set; }

        [Parameter("uint128", "validityEndTimestamp", 11)]
        public virtual BigInteger ValidityEndTimestamp { get; set; }

        [Parameter("bytes32", "uid", 12)]
        public virtual byte[] Uid { get; set; }
    }
}

namespace Thirdweb.Contracts.Forwarder
{
    public partial class ForwardRequest : ForwardRequestBase { }

    public class ForwardRequestBase
    {
        [Parameter("address", "from", 1)]
        [JsonProperty("from")]
        public virtual string From { get; set; }

        [Parameter("address", "to", 2)]
        [JsonProperty("to")]
        public virtual string To { get; set; }

        [Parameter("uint256", "value", 3)]
        [JsonProperty("value")]
        public virtual BigInteger Value { get; set; }

        [Parameter("uint256", "gas", 4)]
        [JsonProperty("gas")]
        public virtual BigInteger Gas { get; set; }

        [Parameter("uint256", "nonce", 5)]
        [JsonProperty("nonce")]
        public virtual BigInteger Nonce { get; set; }

        [Parameter("bytes", "data", 6)]
        [JsonProperty("data")]
        public virtual string Data { get; set; }
    }
}
