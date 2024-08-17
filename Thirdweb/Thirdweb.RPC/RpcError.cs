using Newtonsoft.Json;

namespace Thirdweb;

/// <summary>
/// Represents an error returned from an RPC call.
/// </summary>
public class RpcError
{
    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    [JsonProperty("code")]
    public int Code { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    [JsonProperty("message")]
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets additional data about the error.
    /// </summary>
    [JsonProperty("data")]
    public string Data { get; set; }
}
