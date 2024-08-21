using Newtonsoft.Json;

namespace Thirdweb;

public partial class EcosystemWallet
{
    internal class EnclaveWalletResponse
    {
        [JsonProperty("wallet")]
        internal EnclaveWallet Wallet { get; set; }
    }

    internal class EnclaveWallet
    {
        [JsonProperty("address")]
        internal string Address { get; set; }
    }

    internal class EnclaveSignResponse
    {
        [JsonProperty("r")]
        internal string R { get; set; }

        [JsonProperty("s")]
        internal string S { get; set; }

        [JsonProperty("v")]
        internal string V { get; set; }

        [JsonProperty("signature")]
        internal string Signature { get; set; }

        [JsonProperty("hash")]
        internal string Hash { get; set; }
    }
}
