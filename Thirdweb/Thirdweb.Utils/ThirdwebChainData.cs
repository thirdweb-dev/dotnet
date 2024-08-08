using Newtonsoft.Json;

namespace Thirdweb
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ThirdwebChainDataResponse
    {
        [JsonProperty("data")]
        public ThirdwebChainData Data { get; set; }

        [JsonProperty("error")]
        public object Error { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ThirdwebChainData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("chain")]
        public string Chain { get; set; }

        [JsonProperty("rpc")]
        public List<string> Rpc { get; set; }

        [JsonProperty("nativeCurrency")]
        public ThirdwebChainNativeCurrency NativeCurrency { get; set; }

        [JsonProperty("shortName")]
        public string ShortName { get; set; }

        [JsonProperty("chainId")]
        public int ChainId { get; set; }

        [JsonProperty("networkId")]
        public int NetworkId { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("infoURL")]
        public string InfoURL { get; set; }

        [JsonProperty("icon")]
        public ThirdwebChainIcon Icon { get; set; }

        [JsonProperty("faucets")]
        public List<object> Faucets { get; set; }

        [JsonProperty("slip44")]
        public int? Slip44 { get; set; }

        [JsonProperty("ens")]
        public ThirdwebChainEns Ens { get; set; }

        [JsonProperty("explorers")]
        public List<ThirdwebChainExplorer> Explorers { get; set; }

        [JsonProperty("testnet")]
        public bool Testnet { get; set; }

        [JsonProperty("redFlags")]
        public List<object> RedFlags { get; set; }

        [JsonProperty("parent")]
        public ThirdwebChainParent Parent { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ThirdwebChainNativeCurrency
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("decimals")]
        public int Decimals { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ThirdwebChainIcon
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }
    }

    public class ThirdwebChainEns
    {
        [JsonProperty("registry")]
        public string Registry { get; set; }
    }

    public class ThirdwebChainExplorer
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("standard")]
        public string Standard { get; set; }

        [JsonProperty("icon")]
        public ThirdwebChainIcon Icon { get; set; }
    }

    public class ThirdwebChainParent
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("chain")]
        public string Chain { get; set; }

        [JsonProperty("bridges")]
        public List<ThirdwebChainBridge> Bridges { get; set; }
    }

    public class ThirdwebChainBridge
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
