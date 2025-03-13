using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Thirdweb.Indexer;

/// <summary>
/// Represents the response model wrapping the result of an API call.
/// </summary>
/// <typeparam name="T">The type of the result.</typeparam>
internal class ResponseModel<T>
{
    /// <summary>
    /// The result returned by the API.
    /// </summary>
    [JsonProperty("data")]
    internal T[] Data { get; set; }

    [JsonProperty("aggregations")]
    public object Aggregations { get; set; } = null!;

    [JsonProperty("meta")]
    public Meta Meta { get; set; } = null!;
}

public class Meta
{
    [JsonProperty("chain_ids")]
    public List<BigInteger> ChainIds { get; set; } = new();

    [JsonProperty("address")]
    public string Address { get; set; }

    [JsonProperty("signature")]
    public string Signature { get; set; }

    [JsonProperty("page")]
    public BigInteger Page { get; set; }

    [JsonProperty("limit_per_chain")]
    public BigInteger LimitPerChain { get; set; }

    [JsonProperty("total_items")]
    public BigInteger TotalItems { get; set; }

    [JsonProperty("total_pages")]
    public BigInteger TotalPages { get; set; }
}

#region Price API

public class Token_Price
{
    [JsonProperty("chain_id")]
    public BigInteger ChainId { get; set; }

    [JsonProperty("address")]
    public string Address { get; set; }

    [JsonProperty("symbol")]
    public string Symbol { get; set; }

    [JsonProperty("price_usd")]
    public double PriceUsd { get; set; }

    [JsonProperty("price_usd_cents")]
    public double PriceUsdCents { get; set; }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}

#endregion

#region Tokens API

public class Token_ERC20 : Token { }

public class Token_ERC721 : Token_NFT { }

public class Token_ERC1155 : Token_NFT { }

public class Token
{
    [JsonProperty("chain_id")]
    public BigInteger ChainId { get; set; }

    [JsonProperty("balance")]
    public BigInteger Balance { get; set; }

    [JsonProperty("token_address")]
    public string TokenAddress { get; set; }
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class Token_NFT : Token
{
    [JsonProperty("token_id")]
    public string TokenId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("image_url")]
    public string ImageUrl { get; set; }

    [JsonProperty("video_url")]
    public string VideoUrl { get; set; }

    [JsonProperty("animation_url")]
    public string AnimationUrl { get; set; }

    [JsonProperty("background_color")]
    public string BackgroundColor { get; set; }

    [JsonProperty("external_url")]
    public string ExternalUrl { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("extra_metadata")]
    public NFT_ExtraMetadata ExtraMetadata { get; set; }

    [JsonProperty("collection")]
    public NFT_Collection Collection { get; set; }

    [JsonProperty("contract")]
    public NFT_Contract Contract { get; set; }
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class NFT_ExtraMetadata
{
    [JsonProperty("attributes")]
    public object Attributes { get; set; }

    [JsonProperty("properties")]
    public object Properties { get; set; }
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class AttributeData
{
    [JsonProperty("trait_type")]
    public string TraitType { get; set; }

    [JsonProperty("value")]
    public object Value { get; set; }

    [JsonProperty("display_type")]
    public string DisplayType { get; set; }
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class NFT_Collection
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("image_url")]
    public string ImageUrl { get; set; }

    [JsonProperty("banner_image_url")]
    public string BannerImageUrl { get; set; }

    [JsonProperty("featured_image_url")]
    public string FeaturedImageUrl { get; set; }

    [JsonProperty("external_link")]
    public string ExternalLink { get; set; }
}

public class NFT_Contract
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("symbol")]
    public string Symbol { get; set; }

    [JsonProperty("type")]
    internal string Type { get; set; } // ERC721, ERC1155
}

#endregion

#region Events API

public class Event
{
    [JsonProperty("chain_id")]
    public BigInteger ChainId { get; set; }

    [JsonProperty("block_number")]
    public string BlockNumber { get; set; } = null!;

    [JsonProperty("block_hash")]
    public string BlockHash { get; set; } = null!;

    [JsonProperty("block_timestamp")]
    public string BlockTimestamp { get; set; } = null!;

    [JsonProperty("transaction_hash")]
    public string TransactionHash { get; set; } = null!;

    [JsonProperty("transaction_index")]
    public BigInteger TransactionIndex { get; set; }

    [JsonProperty("log_index")]
    public BigInteger LogIndex { get; set; }

    [JsonProperty("address")]
    public string Address { get; set; } = null!;

    [JsonProperty("data")]
    public string Data { get; set; } = null!;

    [JsonProperty("topics")]
    public List<string> Topics { get; set; } = new();

    [JsonProperty("decoded")]
    public Event_Decoded Decoded { get; set; } = null!;
}

public class Event_Decoded
{
    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("signature")]
    public string Signature { get; set; } = null!;

    [JsonProperty("indexed_params")]
    public JObject IndexedParams { get; set; } = new();

    [JsonProperty("non_indexed_params")]
    public JObject NonIndexedParams { get; set; } = new();
}

#endregion

#region Transactions API

public class Transaction
{
    [JsonProperty("chain_id")]
    public BigInteger ChainId { get; set; }

    [JsonProperty("block_number")]
    public string BlockNumber { get; set; } = null!;

    [JsonProperty("block_hash")]
    public string BlockHash { get; set; } = null!;

    [JsonProperty("block_timestamp")]
    public string BlockTimestamp { get; set; } = null!;

    [JsonProperty("hash")]
    public string Hash { get; set; } = null!;

    [JsonProperty("nonce")]
    public BigInteger Nonce { get; set; }

    [JsonProperty("transaction_index")]
    public BigInteger TransactionIndex { get; set; }

    [JsonProperty("from_address")]
    public string FromAddress { get; set; } = null!;

    [JsonProperty("to_address")]
    public string ToAddress { get; set; } = null!;

    [JsonProperty("value")]
    public BigInteger Value { get; set; }

    [JsonProperty("gas_price")]
    public BigInteger GasPrice { get; set; }

    [JsonProperty("gas")]
    public BigInteger Gas { get; set; }

    [JsonProperty("function_selector")]
    public string FunctionSelector { get; set; } = null!;

    [JsonProperty("data")]
    public string Data { get; set; } = null!;

    [JsonProperty("max_fee_per_gas")]
    public BigInteger MaxFeePerGas { get; set; }

    [JsonProperty("max_priority_fee_per_gas")]
    public BigInteger MaxPriorityFeePerGas { get; set; }

    [JsonProperty("transaction_type")]
    public BigInteger TransactionType { get; set; }

    [JsonProperty("r")]
    public BigInteger R { get; set; }

    [JsonProperty("s")]
    public BigInteger S { get; set; }

    [JsonProperty("v")]
    public BigInteger V { get; set; }

    [JsonProperty("access_list_json")]
    public string AccessListJson { get; set; }

    [JsonProperty("contract_address")]
    public string ContractAddress { get; set; }

    [JsonProperty("gas_used")]
    public BigInteger? GasUsed { get; set; }

    [JsonProperty("cumulative_gas_used")]
    public BigInteger? CumulativeGasUsed { get; set; }

    [JsonProperty("effective_gas_price")]
    public BigInteger? EffectiveGasPrice { get; set; }

    [JsonProperty("blob_gas_used")]
    public BigInteger? BlobGasUsed { get; set; }

    [JsonProperty("blob_gas_price")]
    public BigInteger? BlobGasPrice { get; set; }

    [JsonProperty("logs_bloom")]
    public string LogsBloom { get; set; }

    [JsonProperty("status")]
    public BigInteger? Status { get; set; }
}

#endregion
