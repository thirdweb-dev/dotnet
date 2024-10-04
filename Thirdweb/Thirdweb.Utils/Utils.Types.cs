using Newtonsoft.Json;

namespace Thirdweb;

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

    [JsonProperty("ens")]
    public ThirdwebChainEns Ens { get; set; }

    [JsonProperty("explorers")]
    public List<ThirdwebChainExplorer> Explorers { get; set; }

    [JsonProperty("testnet")]
    public bool Testnet { get; set; }

    [JsonProperty("stackType")]
    public string StackType { get; set; }
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

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class FarcasterProfile
{
    [JsonProperty("fid")]
    public int? Fid { get; set; }

    [JsonProperty("bio")]
    public string Bio { get; set; }

    [JsonProperty("pfp")]
    public string Pfp { get; set; }

    [JsonProperty("display")]
    public string Display { get; set; }

    [JsonProperty("username")]
    public string Username { get; set; }

    [JsonProperty("custodyAddress")]
    public string CustodyAddress { get; set; }

    [JsonProperty("addresses")]
    public List<string> Addresses { get; set; }
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class LensProfile
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("bio")]
    public string Bio { get; set; }

    [JsonProperty("picture")]
    public string Picture { get; set; }

    [JsonProperty("coverPicture")]
    public string CoverPicture { get; set; }
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class EnsProfile
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("address")]
    public string Address { get; set; }

    [JsonProperty("avatar")]
    public string Avatar { get; set; }

    [JsonProperty("display")]
    public string Display { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("keywords")]
    public List<string> Keywords { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("mail")]
    public string Mail { get; set; }

    [JsonProperty("notice")]
    public string Notice { get; set; }

    [JsonProperty("location")]
    public string Location { get; set; }

    [JsonProperty("phone")]
    public string Phone { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("twitter")]
    public string Twitter { get; set; }

    [JsonProperty("github")]
    public string Github { get; set; }

    [JsonProperty("discord")]
    public string Discord { get; set; }

    [JsonProperty("telegram")]
    public string Telegram { get; set; }
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class SocialProfileGeneric
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("avatar")]
    public string Avatar { get; set; }

    [JsonProperty("bio")]
    public string Bio { get; set; }

    [JsonProperty("metadata")]
    public object Metadata { get; set; }
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class SocialProfileResponse
{
    [JsonProperty("data")]
    public List<SocialProfileGeneric> Data { get; set; }

    [JsonProperty("error")]
    public string Error { get; set; }
}

/// <summary>
/// SocialProfiles object that contains all the different types of social profiles and their respective metadata.
/// </summary>
public class SocialProfiles
{
    public List<EnsProfile> EnsProfiles { get; set; }
    public List<FarcasterProfile> FarcasterProfiles { get; set; }
    public List<LensProfile> LensProfiles { get; set; }
    public List<SocialProfileGeneric> OtherProfiles { get; set; }

    public SocialProfiles(List<SocialProfileGeneric> profiles)
    {
        this.EnsProfiles = new List<EnsProfile>();
        this.FarcasterProfiles = new List<FarcasterProfile>();
        this.LensProfiles = new List<LensProfile>();
        this.OtherProfiles = new List<SocialProfileGeneric>();

        foreach (var profile in profiles)
        {
            switch (profile.Type)
            {
                case "ens":
                    this.EnsProfiles.Add(JsonConvert.DeserializeObject<EnsProfile>(JsonConvert.SerializeObject(profile.Metadata)));
                    break;
                case "farcaster":
                    this.FarcasterProfiles.Add(JsonConvert.DeserializeObject<FarcasterProfile>(JsonConvert.SerializeObject(profile.Metadata)));
                    break;
                case "lens":
                    this.LensProfiles.Add(JsonConvert.DeserializeObject<LensProfile>(JsonConvert.SerializeObject(profile.Metadata)));
                    break;
                default:
                    this.OtherProfiles.Add(profile);
                    break;
            }
        }
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
