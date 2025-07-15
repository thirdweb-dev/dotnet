using System.Numerics;
using Newtonsoft.Json;

namespace Thirdweb;

/// <summary>
/// Base class for execution options
/// </summary>
[JsonObject]
public class ExecutionOptions
{
    [JsonProperty("chainId")]
    public BigInteger? ChainId { get; set; } = null;

    [JsonProperty("idempotencyKey")]
    public string IdempotencyKey { get; set; }
}

/// <summary>
/// Auto determine execution options
/// </summary>
[JsonObject]
public class AutoExecutionOptions : ExecutionOptions
{
    [JsonProperty("type")]
    public string Type { get; set; } = "auto";

    [JsonProperty("from")]
    public string From { get; set; }
}

/// <summary>
/// ERC-4337 execution options
/// </summary>
[JsonObject]
public class ERC4337ExecutionOptions : ExecutionOptions
{
    [JsonProperty("type")]
    public string Type { get; set; } = "ERC4337";

    [JsonProperty("signerAddress")]
    public string SignerAddress { get; set; }

    [JsonProperty("accountSalt")]
    public string AccountSalt { get; set; }

    [JsonProperty("smartAccountAddress")]
    public string SmartAccountAddress { get; set; }

    [JsonProperty("entrypointAddress")]
    public string EntrypointAddress { get; set; }

    [JsonProperty("entrypointVersion")]
    public string EntrypointVersion { get; set; }

    [JsonProperty("factoryAddress")]
    public string FactoryAddress { get; set; }

    public ERC4337ExecutionOptions(BigInteger chainId, string signerAddress)
    {
        this.ChainId = chainId;
        this.SignerAddress = signerAddress;
    }
}

/// <summary>
/// Response wrapper for queued transactions
/// </summary>
[JsonObject]
internal class QueuedTransactionResponse
{
    [JsonProperty("result")]
    public QueuedTransactionResult Result { get; set; }
}

/// <summary>
/// Result containing the transactions array
/// </summary>
[JsonObject]
internal class QueuedTransactionResult
{
    [JsonProperty("transactions")]
    public QueuedTransaction[] Transactions { get; set; }
}

/// <summary>
/// Queued transaction response
/// </summary>
[JsonObject]
internal class QueuedTransaction
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("batchIndex")]
    public long BatchIndex { get; set; }

    [JsonProperty("executionParams")]
    public ExecutionOptions ExecutionParams { get; set; }

    [JsonProperty("transactionParams")]
    public InnerTransaction[] TransactionParams { get; set; }
}

/// <summary>
/// Inner transaction data
/// </summary>
[JsonObject]
internal class InnerTransaction
{
    [JsonProperty("to")]
    public string To { get; set; }

    [JsonProperty("data")]
    public string Data { get; set; }

    [JsonProperty("value")]
    public string Value { get; set; }
}
