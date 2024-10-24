using System.Numerics;
using Newtonsoft.Json;

namespace Thirdweb;

public partial class EcosystemWallet
{
    internal class EnclaveUserStatusResponse
    {
        [JsonProperty("linkedAccounts")]
        internal List<LinkedAccount> LinkedAccounts { get; set; }

        [JsonProperty("wallets")]
        internal List<ShardedOrEnclaveWallet> Wallets { get; set; }
    }

    internal class ShardedOrEnclaveWallet
    {
        [JsonProperty("address")]
        internal string Address { get; set; }

        [JsonProperty("createdAt")]
        internal DateTime CreatedAt { get; set; }

        [JsonProperty("type")]
        internal string Type { get; set; }
    }

    internal class EnclaveGenerateResponse
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

    public class EcosystemDetails
    {
        [JsonProperty("thirdwebAccountId")]
        public string ThirdwebAccountId { get; set; }

        [JsonProperty("permission")]
        public string Permission { get; set; }

        [JsonProperty("authOptions")]
        public List<string> AuthOptions { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty("smartAccountOptions")]
        public EcosystemDetails_SmartAccountOptions? SmartAccountOptions { get; set; }
    }

    public struct EcosystemDetails_SmartAccountOptions
    {
        [JsonProperty("chainIds")]
        public List<BigInteger> ChainIds { get; set; }

        [JsonProperty("sponsorGas")]
        public bool SponsorGas { get; set; }

        [JsonProperty("accountFactoryAddress")]
        public string AccountFactoryAddress { get; set; }
    }
}
