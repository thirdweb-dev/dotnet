using Newtonsoft.Json;

namespace Thirdweb
{
    public class RpcResponse<T>
    {
        [JsonProperty("jsonrpc")]
        public string Jsonrpc { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("result")]
        public T Result { get; set; }

        [JsonProperty("error")]
        public RpcError Error { get; set; }
    }
}
