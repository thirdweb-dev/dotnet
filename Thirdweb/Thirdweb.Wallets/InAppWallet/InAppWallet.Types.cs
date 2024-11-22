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
    public string Type { get; set; }
    public LinkedAccountDetails Details { get; set; }

    public struct LinkedAccountDetails
    {
        public string Email { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Id { get; set; }
    }

    public override readonly string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
