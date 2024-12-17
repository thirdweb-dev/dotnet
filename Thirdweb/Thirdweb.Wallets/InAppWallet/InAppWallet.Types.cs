using Newtonsoft.Json;

namespace Thirdweb;

/// <summary>
/// Specifies the authentication providers available for the in-app wallet.
/// </summary>
public enum AuthProvider
{
    Default,
    Google,
    Apple,
    Facebook,
    JWT,
    AuthEndpoint,
    Discord,
    Farcaster,
    Telegram,
    Siwe,
    Line,
    Guest,
    X,
    Coinbase,
    Github,
    Twitch,
    Steam
}

/// <summary>
/// Represents a linked account.
/// </summary>
public struct LinkedAccount
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("details")]
    public LinkedAccountDetails Details { get; set; }

    public struct LinkedAccountDetails
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("name")]
        public string Address { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public override readonly string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
