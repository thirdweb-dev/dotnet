using Newtonsoft.Json;

namespace Thirdweb
{
    public class RpcRequest
    {
        [JsonProperty("jsonrpc")]
        public string Jsonrpc { get; set; } = "2.0";

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        public object[] Params { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
