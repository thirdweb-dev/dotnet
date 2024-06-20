using System.Numerics;
using Newtonsoft.Json;

namespace Thirdweb
{
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
}
