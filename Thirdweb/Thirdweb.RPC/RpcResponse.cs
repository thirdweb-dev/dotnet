using Newtonsoft.Json;

namespace Thirdweb
{
    /// <summary>
    /// Represents a response from an RPC call.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    public class RpcResponse<T>
    {
        /// <summary>
        /// Gets or sets the JSON-RPC version.
        /// </summary>
        [JsonProperty("jsonrpc")]
        public string Jsonrpc { get; set; }

        /// <summary>
        /// Gets or sets the ID of the RPC request.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the result of the RPC call.
        /// </summary>
        [JsonProperty("result")]
        public T Result { get; set; }

        /// <summary>
        /// Gets or sets the error details if the RPC call fails.
        /// </summary>
        [JsonProperty("error")]
        public RpcError Error { get; set; }
    }
}
