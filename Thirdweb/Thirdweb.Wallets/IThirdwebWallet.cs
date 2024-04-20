using System.Numerics;
using Nethereum.ABI.EIP712;
using Nethereum.RPC.Eth.DTOs;
using Newtonsoft.Json;

namespace Thirdweb
{
    public interface IThirdwebWallet
    {
        public ThirdwebAccountType AccountType { get; }
        public Task<string> GetAddress();
        public Task<string> EthSign(string message);
        public Task<string> PersonalSign(byte[] rawMessage);
        public Task<string> PersonalSign(string message);
        public Task<string> SignTypedDataV4(string json);
        public Task<string> SignTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData)
            where TDomain : IDomain;
        public Task<bool> IsConnected();
        public Task<string> SignTransaction(TransactionInput transaction, BigInteger chainId);
        public Task<string> Authenticate(string domain, BigInteger chainId, string authPayloadPath = "/auth/payload", string authLoginPath = "/auth/login", HttpClient httpClient = null);
    }

    public enum ThirdwebAccountType
    {
        PrivateKeyAccount,
        SmartAccount
    }

    [Serializable]
    public struct LoginPayload
    {
        public LoginPayloadData payload;
        public string signature;
    }

    [Serializable]
    public class LoginPayloadData
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("statement")]
        public string Statement { get; set; }

        [JsonProperty("uri", NullValueHandling = NullValueHandling.Ignore)]
        public string Uri { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }

        [JsonProperty("chain_id", NullValueHandling = NullValueHandling.Ignore)]
        public string ChainId { get; set; }

        [JsonProperty("nonce", NullValueHandling = NullValueHandling.Ignore)]
        public string Nonce { get; set; }

        [JsonProperty("issued_at", NullValueHandling = NullValueHandling.Ignore)]
        public string IssuedAt { get; set; }

        [JsonProperty("expiration_time", NullValueHandling = NullValueHandling.Ignore)]
        public string ExpirationTime { get; set; }

        [JsonProperty("invalid_before", NullValueHandling = NullValueHandling.Ignore)]
        public string InvalidBefore { get; set; }

        [JsonProperty("resources", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Resources { get; set; }

        public LoginPayloadData()
        {
            Type = "evm";
        }
    }
}
