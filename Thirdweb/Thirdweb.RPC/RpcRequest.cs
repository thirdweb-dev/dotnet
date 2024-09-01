using Newtonsoft.Json;

namespace Thirdweb;

/// <summary>
/// Represents an RPC request.
/// </summary>
public class RpcRequest
{
    /// <summary>
    /// Gets or sets the JSON-RPC version.
    /// </summary>
    [JsonProperty("jsonrpc")]
    public string Jsonrpc { get; set; } = "2.0";

    /// <summary>
    /// Gets or sets the method name for the RPC request.
    /// </summary>
    [JsonProperty("method")]
    public string Method { get; set; }

    /// <summary>
    /// Gets or sets the parameters for the RPC request.
    /// </summary>
    [JsonProperty("params")]
    public object[] Params { get; set; }

    /// <summary>
    /// Gets or sets the ID of the RPC request.
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; set; }

    public RpcRequest() { }

    public RpcRequest(string method, params object[] parameters)
    {
        this.Method = method;
        this.Params = parameters;
    }
}
