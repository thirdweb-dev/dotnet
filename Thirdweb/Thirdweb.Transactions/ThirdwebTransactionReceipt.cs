using Nethereum.Hex.HexTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Thirdweb
{
    public class ThirdwebTransactionReceipt
    {
        [JsonProperty(PropertyName = "transactionHash")]
        public string TransactionHash { get; set; }

        [JsonProperty(PropertyName = "transactionIndex")]
        public HexBigInteger TransactionIndex { get; set; }

        [JsonProperty(PropertyName = "blockHash")]
        public string BlockHash { get; set; }

        [JsonProperty(PropertyName = "blockNumber")]
        public HexBigInteger BlockNumber { get; set; }

        [JsonProperty(PropertyName = "from")]
        public string From { get; set; }

        [JsonProperty(PropertyName = "to")]
        public string To { get; set; }

        [JsonProperty(PropertyName = "cumulativeGasUsed")]
        public HexBigInteger CumulativeGasUsed { get; set; }

        [JsonProperty(PropertyName = "gasUsed")]
        public HexBigInteger GasUsed { get; set; }

        [JsonProperty(PropertyName = "effectiveGasPrice")]
        public HexBigInteger EffectiveGasPrice { get; set; }

        [JsonProperty(PropertyName = "contractAddress")]
        public string ContractAddress { get; set; }

        [JsonProperty(PropertyName = "status")]
        public HexBigInteger Status { get; set; }

        [JsonProperty(PropertyName = "logs")]
        public JArray Logs { get; set; }

        [JsonProperty(PropertyName = "type")]
        public HexBigInteger Type { get; set; }

        [JsonProperty(PropertyName = "logsBloom")]
        public string LogsBloom { get; set; }

        [JsonProperty(PropertyName = "root")]
        public string Root { get; set; }
    }
}
