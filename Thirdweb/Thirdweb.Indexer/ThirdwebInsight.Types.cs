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

public class Token
{
    [JsonProperty("chainId", Required = Required.Always)]
    public BigInteger ChainId { get; set; }

    [JsonProperty("balance", Required = Required.Always)]
    public BigInteger Balance { get; set; }

    [JsonProperty("tokenAddress", Required = Required.Always)]
    public string TokenAddress { get; set; }
}

public class Token_ERC20 : Token { }

public class Token_ERC721 : Token
{
    [JsonProperty("tokenId", Required = Required.Always)]
    public BigInteger TokenId { get; set; }
}

public class Token_ERC1155 : Token
{
    [JsonProperty("tokenId", Required = Required.Always)]
    public BigInteger TokenId { get; set; }
}

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
    public Decoded Decoded { get; set; } = null!;
}

public class Transaction
{
    [JsonProperty("chain_id", Required = Required.Always)]
    public BigInteger ChainId { get; set; }

    [JsonProperty("block_number", Required = Required.Always)]
    public string BlockNumber { get; set; } = null!;

    [JsonProperty("block_hash", Required = Required.Always)]
    public string BlockHash { get; set; } = null!;

    [JsonProperty("block_timestamp", Required = Required.Always)]
    public string BlockTimestamp { get; set; } = null!;

    [JsonProperty("hash", Required = Required.Always)]
    public string Hash { get; set; } = null!;

    [JsonProperty("nonce", Required = Required.Always)]
    public BigInteger Nonce { get; set; }

    [JsonProperty("transaction_index", Required = Required.Always)]
    public BigInteger TransactionIndex { get; set; }

    [JsonProperty("from_address", Required = Required.Always)]
    public string FromAddress { get; set; } = null!;

    [JsonProperty("to_address", Required = Required.Always)]
    public string ToAddress { get; set; } = null!;

    [JsonProperty("value", Required = Required.Always)]
    public BigInteger Value { get; set; }

    [JsonProperty("gas_price", Required = Required.Always)]
    public BigInteger GasPrice { get; set; }

    [JsonProperty("gas", Required = Required.Always)]
    public BigInteger Gas { get; set; }

    [JsonProperty("function_selector", Required = Required.Always)]
    public string FunctionSelector { get; set; } = null!;

    [JsonProperty("data", Required = Required.Always)]
    public string Data { get; set; } = null!;

    [JsonProperty("max_fee_per_gas", Required = Required.Always)]
    public BigInteger MaxFeePerGas { get; set; }

    [JsonProperty("max_priority_fee_per_gas", Required = Required.Always)]
    public BigInteger MaxPriorityFeePerGas { get; set; }

    [JsonProperty("transaction_type", Required = Required.Always)]
    public BigInteger TransactionType { get; set; }

    [JsonProperty("r", Required = Required.Always)]
    public BigInteger R { get; set; }

    [JsonProperty("s", Required = Required.Always)]
    public BigInteger S { get; set; }

    [JsonProperty("v", Required = Required.Always)]
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

public class Decoded
{
    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("signature")]
    public string Signature { get; set; } = null!;

    [JsonProperty("indexedParams")]
    public JObject IndexedParams { get; set; } = new();

    [JsonProperty("nonIndexedParams")]
    public JObject NonIndexedParams { get; set; } = new();
}

public class Meta
{
    [JsonProperty("chain_ids", Required = Required.Always)]
    public List<BigInteger> ChainIds { get; set; } = new();

    [JsonProperty("address")]
    public string Address { get; set; }

    [JsonProperty("signature")]
    public string Signature { get; set; }

    [JsonProperty("page", Required = Required.Always)]
    public BigInteger Page { get; set; }

    [JsonProperty("limit_per_chain", Required = Required.Always)]
    public BigInteger LimitPerChain { get; set; }

    [JsonProperty("total_items", Required = Required.Always)]
    public BigInteger TotalItems { get; set; }

    [JsonProperty("total_pages", Required = Required.Always)]
    public BigInteger TotalPages { get; set; }
}
