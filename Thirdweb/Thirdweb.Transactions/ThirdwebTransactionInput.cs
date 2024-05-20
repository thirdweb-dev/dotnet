using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Newtonsoft.Json;

namespace Thirdweb
{
    public class ThirdwebTransactionInput
    {
        public ThirdwebTransactionInput() { }

        [JsonProperty(PropertyName = "nonce")]
        public HexBigInteger Nonce { get; set; }

        private string _from;
        private string _to;
        private string _data;

        [JsonProperty(PropertyName = "from")]
        public string From
        {
            get => _from.EnsureHexPrefix();
            set => _from = value;
        }

        [JsonProperty(PropertyName = "to")]
        public string To
        {
            get => _to.EnsureHexPrefix();
            set => _to = value;
        }

        [JsonProperty(PropertyName = "gas")]
        public HexBigInteger Gas { get; set; }

        [JsonProperty(PropertyName = "gasPrice")]
        public HexBigInteger GasPrice { get; set; }

        [JsonProperty(PropertyName = "value")]
        public HexBigInteger Value { get; set; }

        [JsonProperty(PropertyName = "data")]
        public string Data
        {
            get => _data.EnsureHexPrefix();
            set => _data = value;
        }

        [JsonProperty(PropertyName = "maxFeePerGas")]
        public HexBigInteger MaxFeePerGas { get; set; }

        [JsonProperty(PropertyName = "maxPriorityFeePerGas")]
        public HexBigInteger MaxPriorityFeePerGas { get; set; }

        [JsonProperty(PropertyName = "chainId")]
        public HexBigInteger ChainId { get; set; }
    }
}
