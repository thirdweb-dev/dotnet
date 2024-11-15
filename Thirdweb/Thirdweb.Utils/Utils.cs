using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ADRaffy.ENSNormalize;
using Nethereum.ABI.EIP712;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.Model;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.Signer;
using Nethereum.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Thirdweb;

/// <summary>
/// Provides utility methods for various operations.
/// </summary>
public static partial class Utils
{
    private static readonly Dictionary<BigInteger, bool> _eip155EnforcedCache = new();
    private static readonly Dictionary<BigInteger, ThirdwebChainData> _chainDataCache = new();
    private static readonly Dictionary<string, string> _ensCache = new();
    private static readonly List<string[]> _errorSubstringsComposite = new() { new string[] { "account", "not found!" }, new[] { "wrong", "chainid" } };

    /// <summary>
    /// Computes the client ID from the given secret key.
    /// </summary>
    /// <param name="secretKey">The secret key.</param>
    /// <returns>The computed client ID.</returns>
    public static string ComputeClientIdFromSecretKey(string secretKey)
    {
#if NETSTANDARD
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(secretKey));
        return BitConverter.ToString(hash).Replace("-", "").ToLower()[..32];
#else
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(secretKey));
        return BitConverter.ToString(hash).Replace("-", "").ToLower()[..32];
#endif
    }

    // public static byte[] StringToSha256(string bytes)
    // {
    //     #if NETSTANDARD
    //     using var sha256 = SHA256.Create();
    //     return sha256.ComputeHash(bytes);
    // }

    /// <summary>
    /// Concatenates the given hex strings.
    /// </summary>
    /// <param name="hexStrings">The hex strings to concatenate.</param>
    /// <returns>The concatenated hex string.</returns>
    public static string HexConcat(params string[] hexStrings)
    {
        var hex = new StringBuilder("0x");

        foreach (var hexStr in hexStrings)
        {
            _ = hex.Append(hexStr[2..]);
        }

        return hex.ToString();
    }

    /// <summary>
    /// Hashes the given message bytes with a prefixed message.
    /// </summary>
    /// <param name="messageBytes">The message bytes to hash.</param>
    /// <returns>The hashed message bytes.</returns>
    public static byte[] HashPrefixedMessage(this byte[] messageBytes)
    {
        var signer = new EthereumMessageSigner();
        return signer.HashPrefixedMessage(messageBytes);
    }

    /// <summary>
    /// Hashes the given message with a prefixed message.
    /// </summary>
    /// <param name="message">The message to hash.</param>
    /// <returns>The hashed message.</returns>
    public static string HashPrefixedMessage(this string message)
    {
        var signer = new EthereumMessageSigner();
        return signer.HashPrefixedMessage(Encoding.UTF8.GetBytes(message)).ToHex(true);
    }

    /// <summary>
    /// Hashes the given message bytes.
    /// </summary>
    /// <param name="messageBytes">The message bytes to hash.</param>
    /// <returns>The hashed message bytes.</returns>
    public static byte[] HashMessage(this byte[] messageBytes)
    {
        return Sha3Keccack.Current.CalculateHash(messageBytes);
    }

    /// <summary>
    /// Hashes the given message.
    /// </summary>
    /// <param name="message">The message to hash.</param>
    /// <returns>The hashed message.</returns>
    public static string HashMessage(this string message)
    {
        return Sha3Keccack.Current.CalculateHash(message);
    }

    /// <summary>
    /// Converts the given bytes to a hex string.
    /// </summary>
    /// <param name="bytes">The bytes to convert.</param>
    /// <returns>The hex string.</returns>
    public static string BytesToHex(this byte[] bytes)
    {
        return bytes.ToHex(true);
    }

    /// <summary>
    /// Converts the given hex string to bytes.
    /// </summary>
    /// <param name="hex">The hex string to convert.</param>
    /// <returns>The bytes.</returns>
    public static byte[] HexToBytes(this string hex)
    {
        return hex.HexToByteArray();
    }

    /// <summary>
    /// Converts the given hex string to a big integer.
    /// </summary>
    /// <param name="hex">The hex string to convert.</param>
    /// <returns>The big integer.</returns>
    public static BigInteger HexToBigInt(this string hex)
    {
        return new HexBigInteger(hex).Value;
    }

    /// <summary>
    /// Converts the given string to a hex string.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>The hex string.</returns>
    public static string StringToHex(this string str)
    {
        return "0x" + Encoding.UTF8.GetBytes(str).ToHex();
    }

    /// <summary>
    /// Converts the given hex string to a regular string.
    /// </summary>
    /// <param name="hex">The hex string to convert.</param>
    /// <returns>The regular string.</returns>
    public static string HexToString(this string hex)
    {
        var array = HexToBytes(hex);
        return Encoding.UTF8.GetString(array, 0, array.Length);
    }

    /// <summary>
    /// Gets the current Unix timestamp.
    /// </summary>
    /// <returns>The current Unix timestamp.</returns>
    public static long GetUnixTimeStampNow()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    /// <summary>
    /// Gets the Unix timestamp for 10 years from now.
    /// </summary>
    /// <returns>The Unix timestamp for 10 years from now.</returns>
    public static long GetUnixTimeStampIn10Years()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (60 * 60 * 24 * 365 * 10);
    }

    /// <summary>
    /// Replaces the IPFS URI with a specified gateway.
    /// </summary>
    /// <param name="uri">The URI to replace.</param>
    /// <param name="gateway">The gateway to use.</param>
    /// <returns>The replaced URI.</returns>
    public static string ReplaceIPFS(this string uri, string gateway = null)
    {
        gateway ??= Constants.FALLBACK_IPFS_GATEWAY;
        return !string.IsNullOrEmpty(uri) && uri.StartsWith("ipfs://") ? uri.Replace("ipfs://", gateway) : uri;
    }

    /// <summary>
    /// Converts the given ether value to wei.
    /// </summary>
    /// <param name="eth">The ether value to convert.</param>
    /// <returns>The wei value.</returns>
    public static string ToWei(this string eth)
    {
        if (!double.TryParse(eth, NumberStyles.Number, CultureInfo.InvariantCulture, out var ethDouble))
        {
            throw new ArgumentException("Invalid eth value.");
        }

        var wei = (BigInteger)(ethDouble * Constants.DECIMALS_18);
        return wei.ToString();
    }

    /// <summary>
    /// Converts the given wei value to ether.
    /// </summary>
    /// <param name="wei">The wei value to convert.</param>
    /// <param name="decimalsToDisplay">The number of decimals to display.</param>
    /// <param name="addCommas">Whether to add commas to the output.</param>
    /// <returns>The ether value.</returns>
    public static string ToEth(this string wei, int decimalsToDisplay = 4, bool addCommas = false)
    {
        return FormatERC20(wei, decimalsToDisplay, 18, addCommas);
    }

    /// <summary>
    /// Formats the given ERC20 token value.
    /// </summary>
    /// <param name="wei">The wei value to format.</param>
    /// <param name="decimalsToDisplay">The number of decimals to display.</param>
    /// <param name="decimals">The number of decimals of the token.</param>
    /// <param name="addCommas">Whether to add commas to the output.</param>
    /// <returns>The formatted token value.</returns>
    public static string FormatERC20(this string wei, int decimalsToDisplay = 4, int decimals = 18, bool addCommas = false)
    {
        if (!BigInteger.TryParse(wei, out var weiBigInt))
        {
            throw new ArgumentException("Invalid wei value.");
        }

        var eth = (double)weiBigInt / Math.Pow(10.0, decimals);
        var format = addCommas ? "#,0" : "#0";
        if (decimalsToDisplay > 0)
        {
            format += ".";
            format += new string('0', decimalsToDisplay);
        }

        return eth.ToString(format);
    }

    /// <summary>
    /// Generates a Sign-In With Ethereum (SIWE) message.
    /// </summary>
    /// <param name="loginPayloadData">The login payload data.</param>
    /// <returns>The generated SIWE message.</returns>
    public static string GenerateSIWE(LoginPayloadData loginPayloadData)
    {
        if (loginPayloadData == null)
        {
            throw new ArgumentNullException(nameof(loginPayloadData));
        }
        else if (string.IsNullOrEmpty(loginPayloadData.Domain))
        {
            throw new ArgumentNullException(nameof(loginPayloadData.Domain));
        }
        else if (string.IsNullOrEmpty(loginPayloadData.Address))
        {
            throw new ArgumentNullException(nameof(loginPayloadData.Address));
        }
        else if (string.IsNullOrEmpty(loginPayloadData.Version))
        {
            throw new ArgumentNullException(nameof(loginPayloadData.Version));
        }
        else if (string.IsNullOrEmpty(loginPayloadData.ChainId))
        {
            throw new ArgumentNullException(nameof(loginPayloadData.ChainId));
        }
        else if (string.IsNullOrEmpty(loginPayloadData.Nonce))
        {
            throw new ArgumentNullException(nameof(loginPayloadData.Nonce));
        }
        else if (string.IsNullOrEmpty(loginPayloadData.IssuedAt))
        {
            throw new ArgumentNullException(nameof(loginPayloadData.IssuedAt));
        }

        var resourcesString = loginPayloadData.Resources != null ? "\nResources:" + string.Join("", loginPayloadData.Resources.Select(r => $"\n- {r}")) : string.Empty;
        var payloadToSign =
            $"{loginPayloadData.Domain} wants you to sign in with your Ethereum account:"
            + $"\n{loginPayloadData.Address}\n\n"
            + $"{(string.IsNullOrEmpty(loginPayloadData.Statement) ? "" : $"{loginPayloadData.Statement}\n")}"
            + $"{(string.IsNullOrEmpty(loginPayloadData.Uri) ? "" : $"\nURI: {loginPayloadData.Uri}")}"
            + $"\nVersion: {loginPayloadData.Version}"
            + $"\nChain ID: {loginPayloadData.ChainId}"
            + $"\nNonce: {loginPayloadData.Nonce}"
            + $"\nIssued At: {loginPayloadData.IssuedAt}"
            + $"{(string.IsNullOrEmpty(loginPayloadData.ExpirationTime) ? "" : $"\nExpiration Time: {loginPayloadData.ExpirationTime}")}"
            + $"{(string.IsNullOrEmpty(loginPayloadData.InvalidBefore) ? "" : $"\nNot Before: {loginPayloadData.InvalidBefore}")}"
            + resourcesString;
        return payloadToSign;
    }

    /// <summary>
    /// Checks if the chain ID corresponds to zkSync.
    /// </summary>
    /// <param name="client">The Thirdweb client.</param>
    /// <param name="chainId">The chain ID.</param>
    /// <returns>True if it is a zkSync chain ID, otherwise false.</returns>
    public static async Task<bool> IsZkSync(ThirdwebClient client, BigInteger chainId)
    {
        if (chainId.Equals(324) || chainId.Equals(300) || chainId.Equals(302) || chainId.Equals(11124) || chainId.Equals(4654) || chainId.Equals(333271) || chainId.Equals(37111))
        {
            return true;
        }
        else
        {
            try
            {
                var chainData = await GetChainMetadata(client, chainId).ConfigureAwait(false);
                return !string.IsNullOrEmpty(chainData.StackType) && chainData.StackType.Contains("zksync", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                // Assume it is not zkSync if the chain data could not be fetched
                return false;
            }
        }
    }

    /// <summary>
    /// Converts an Ethereum address to its checksum format.
    /// </summary>
    /// <param name="address">The Ethereum address.</param>
    /// <returns>The checksummed Ethereum address.</returns>
    public static string ToChecksumAddress(this string address)
    {
        return new AddressUtil().ConvertToChecksumAddress(address);
    }

    /// <summary>
    /// Decodes all events of the specified type from the transaction receipt logs.
    /// </summary>
    /// <typeparam name="TEventDTO">The event DTO type.</typeparam>
    /// <param name="transactionReceipt">The transaction receipt.</param>
    /// <returns>A list of decoded events.</returns>
    public static List<EventLog<TEventDTO>> DecodeAllEvents<TEventDTO>(this ThirdwebTransactionReceipt transactionReceipt)
        where TEventDTO : new()
    {
        return transactionReceipt.Logs.DecodeAllEvents<TEventDTO>();
    }

    /// <summary>
    /// Adjusts the value's decimals.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="fromDecimals">The original number of decimals.</param>
    /// <param name="toDecimals">The target number of decimals.</param>
    /// <returns>The value adjusted to the new decimals.</returns>
    public static BigInteger AdjustDecimals(this BigInteger value, int fromDecimals, int toDecimals)
    {
        var differenceInDecimals = fromDecimals - toDecimals;

        if (differenceInDecimals > 0)
        {
            return value / BigInteger.Pow(10, differenceInDecimals);
        }
        else if (differenceInDecimals < 0)
        {
            return value * BigInteger.Pow(10, -differenceInDecimals);
        }

        return value;
    }

    public static async Task<ThirdwebChainData> GetChainMetadata(ThirdwebClient client, BigInteger chainId)
    {
        if (_chainDataCache.TryGetValue(chainId, out var value))
        {
            return value;
        }

        if (client == null)
        {
            throw new ArgumentNullException(nameof(client));
        }

        if (chainId <= 0)
        {
            throw new ArgumentException("Invalid chain ID.");
        }

        var url = $"https://api.thirdweb.com/v1/chains/{chainId}";
        try
        {
            var response = await client.HttpClient.GetAsync(url).ConfigureAwait(false);
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var deserializedResponse = JsonConvert.DeserializeObject<ThirdwebChainDataResponse>(json);

            if (deserializedResponse == null || deserializedResponse.Error != null)
            {
                throw new Exception($"Failed to fetch chain data for chain ID {chainId}. Error: {JsonConvert.SerializeObject(deserializedResponse?.Error)}");
            }
            else
            {
                deserializedResponse.Data.Explorers = deserializedResponse.Data.Explorers == null || deserializedResponse.Data.Explorers.Count == 0 ? null : deserializedResponse.Data.Explorers;
                _chainDataCache[chainId] = deserializedResponse.Data;
                return deserializedResponse.Data;
            }
        }
        catch (HttpRequestException httpEx)
        {
            throw new Exception($"HTTP request error while fetching chain data for chain ID {chainId}: {httpEx.Message}", httpEx);
        }
        catch (JsonException jsonEx)
        {
            throw new Exception($"JSON deserialization error while fetching chain data for chain ID {chainId}: {jsonEx.Message}", jsonEx);
        }
        catch (Exception ex)
        {
            throw new Exception($"Unexpected error while fetching chain data for chain ID {chainId}: {ex.Message}", ex);
        }
    }

    public static int GetEntryPointVersion(string address)
    {
        if (address == null)
        {
            return 6; // non-4337
        }

        address = address.ToChecksumAddress();
        return address switch
        {
            Constants.ENTRYPOINT_ADDRESS_V06 => 6,
            Constants.ENTRYPOINT_ADDRESS_V07 => 7,
            _ => 6,
        };
    }

    public static byte[] HexToBytes32(this string hex)
    {
        if (hex.StartsWith("0x"))
        {
            hex = hex[2..];
        }

        if (hex.Length > 64)
        {
            throw new ArgumentException("Hex string is too long to fit into 32 bytes.");
        }

        hex = hex.PadLeft(64, '0');

        var bytes = new byte[32];
        for (var i = 0; i < hex.Length; i += 2)
        {
            bytes[i / 2] = byte.Parse(hex.AsSpan(i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        return bytes;
    }

    public static string ToJsonExternalWalletFriendly<TMessage, TDomain>(TypedData<TDomain> typedData, TMessage message)
    {
        typedData.EnsureDomainRawValuesAreInitialised();
        typedData.Message = MemberValueFactory.CreateFromMessage(message);
        var obj = (JObject)JToken.FromObject(typedData);
        var jProperty = new JProperty("domain");
        var jProperties = GetJProperties("EIP712Domain", typedData.DomainRawValues, typedData);
        object[] content = jProperties.ToArray();
        jProperty.Value = new JObject(content);
        obj.Add(jProperty);
        var jProperty2 = new JProperty("message");
        var jProperties2 = GetJProperties(typedData.PrimaryType, typedData.Message, typedData);
        content = jProperties2.ToArray();
        jProperty2.Value = new JObject(content);
        obj.Add(jProperty2);
        return obj.ToString();
    }

#if NET8_0_OR_GREATER
    [GeneratedRegex("bytes\\d+")]
    private static partial Regex BytesRegex();

    [GeneratedRegex("uint\\d+")]
    private static partial Regex UintRegex();

    [GeneratedRegex("int\\d+")]
    private static partial Regex IntRegex();

    private static bool IsReferenceType(string typeName)
    {
        if (!BytesRegex().IsMatch(typeName))
        {
            if (!UintRegex().IsMatch(typeName))
            {
                if (!IntRegex().IsMatch(typeName))
                {
                    switch (typeName)
                    {
                        case "bytes":
                        case "string":
                        case "bool":
                        case "address":
                            break;
                        default:
                            if (typeName.Contains('['))
                            {
                                return false;
                            }
                            return true;
                    }
                }
            }
        }
        return false;
    }
#else
    private static bool IsReferenceType(string typeName)
    {
        if (!new Regex("bytes\\d+").IsMatch(typeName))
        {
            if (!new Regex("uint\\d+").IsMatch(typeName))
            {
                if (!new Regex("int\\d+").IsMatch(typeName))
                {
                    switch (typeName)
                    {
                        case "bytes":
                        case "string":
                        case "bool":
                        case "address":
                            break;
                        default:
                            if (typeName.Contains('['))
                            {
                                return false;
                            }
                            return true;
                    }
                }
            }
        }
        return false;
    }
#endif

    private static List<JProperty> GetJProperties(string mainTypeName, MemberValue[] values, TypedDataRaw typedDataRaw)
    {
        var list = new List<JProperty>();
        var array = typedDataRaw.Types[mainTypeName];
        for (var i = 0; i < array.Length; i++)
        {
            var type = array[i].Type;
            var name = array[i].Name;
            if (IsReferenceType(type))
            {
                var jProperty = new JProperty(name);
                if (values[i].Value != null)
                {
                    object[] content = GetJProperties(type, (MemberValue[])values[i].Value, typedDataRaw).ToArray();
                    jProperty.Value = new JObject(content);
                }
                else
                {
                    jProperty.Value = null;
                }

                list.Add(jProperty);
            }
            else if (type.StartsWith("bytes"))
            {
                var name2 = name;
                if (values[i].Value is byte[] v)
                {
                    var content2 = v.BytesToHex();
                    list.Add(new JProperty(name2, content2));
                }
                else
                {
                    var value = values[i].Value;
                    list.Add(new JProperty(name2, value));
                }
            }
            else if (type.Contains('['))
            {
                var jProperty2 = new JProperty(name);
                var jArray = new JArray();
                var text = type[..type.LastIndexOf('[')];
                if (values[i].Value == null)
                {
                    jProperty2.Value = null;
                    list.Add(jProperty2);
                    continue;
                }

                if (IsReferenceType(text))
                {
                    foreach (var item in (List<MemberValue[]>)values[i].Value)
                    {
                        object[] content = GetJProperties(text, item, typedDataRaw).ToArray();
                        jArray.Add(new JObject(content));
                    }

                    jProperty2.Value = jArray;
                    list.Add(jProperty2);
                    continue;
                }

                foreach (var item2 in (System.Collections.IList)values[i].Value)
                {
                    jArray.Add(item2);
                }

                jProperty2.Value = jArray;
                list.Add(jProperty2);
            }
            else
            {
                var name3 = name;
                var value2 = values[i].Value;
                list.Add(new JProperty(name3, value2));
            }
        }

        return list;
    }

    public static async Task<bool> IsEip155Enforced(ThirdwebClient client, BigInteger chainId)
    {
        if (_eip155EnforcedCache.TryGetValue(chainId, out var value))
        {
            return value;
        }

        var result = false;
        var rpc = ThirdwebRPC.GetRpcInstance(client, chainId);

        try
        {
            // Pre-155 tx that will fail
            var rawTransaction =
                "0xf8a58085174876e800830186a08080b853604580600e600039806000f350fe7fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffe03601600081602082378035828234f58015156039578182fd5b8082525050506014600cf31ba02222222222222222222222222222222222222222222222222222222222222222a02222222222222222222222222222222222222222222222222222222222222222";
            _ = await rpc.SendRequestAsync<string>("eth_sendRawTransaction", rawTransaction);
        }
        catch (Exception e)
        {
            var errorMsg = e.Message.ToLower();

            var errorSubstrings = new List<string>
            {
                "eip-155",
                "eip155",
                "protected",
                "invalid chain id for signer",
                "chain id none",
                "chain_id mismatch",
                "recovered sender mismatch",
                "transaction hash mismatch",
                "chainid no support",
                "chainid (0)",
                "chainid(0)",
                "invalid sender"
            };

            if (errorSubstrings.Any(errorMsg.Contains))
            {
                result = true;
            }
            else
            {
                // Check if all substrings in any of the composite substrings are present
                result = _errorSubstringsComposite.Any(arr => arr.All(substring => errorMsg.Contains(substring)));
            }
        }

        _eip155EnforcedCache[chainId] = result;

        return result;
    }

    public static bool IsEip1559Supported(string chainId)
    {
        switch (chainId)
        {
            // BNB Mainnet
            case "56":
            // BNB Testnet
            case "97":
            // opBNB Mainnet
            case "204":
            // opBNB Testnet
            case "5611":
            // Oasys Mainnet
            case "248":
            // Oasys Testnet
            case "9372":
            // Vanar Mainnet
            case "2040":
            // Vanar Testnet (Vanguard)
            case "78600":
            // Taraxa Mainnet
            case "841":
            // Taraxa Testnet
            case "842":
                return false;
            default:
                return true;
        }
    }

#if NET8_0_OR_GREATER
    [GeneratedRegex("^\\.|\\.$")]
    private static partial Regex PacketRegex();
#endif

    public static byte[] PacketToBytes(string packet)
    {
#if !NET8_0_OR_GREATER
        var value = new Regex("^\\.|\\.$").Replace(packet, "");
#else
        var value = PacketRegex().Replace(packet, "");
#endif
        if (string.IsNullOrEmpty(value))
        {
            return new byte[] { 0 };
        }

        var labels = value.Split(".");
        using var memoryStream = new MemoryStream();
        foreach (var label in labels)
        {
            byte[] labelBytes;

            if (label.Length > 63)
            {
                labelBytes = label.HashMessage().HexToBytes();
            }
            else
            {
                labelBytes = Encoding.UTF8.GetBytes(label);
            }

            if (labelBytes.Length > 255)
            {
                throw new ArgumentException("Label is too long after encoding.");
            }

            memoryStream.WriteByte((byte)labelBytes.Length);
            memoryStream.Write(labelBytes, 0, labelBytes.Length);
        }

        memoryStream.WriteByte(0);
        return memoryStream.ToArray();
    }

    public static async Task<string> GetENSFromAddress(ThirdwebClient client, string address)
    {
        if (string.IsNullOrEmpty(address))
        {
            throw new ArgumentNullException(nameof(address));
        }

        if (!address.IsValidAddress())
        {
            throw new ArgumentException("Invalid address.");
        }

        if (_ensCache.TryGetValue(address, out var value))
        {
            return value;
        }

        var contract = await ThirdwebContract.Create(client: client, address: Constants.ENS_REGISTRY_ADDRESS, chain: 1).ConfigureAwait(false);
        var reverseName = address.ToLower()[2..] + ".addr.reverse";
        var reverseNameBytes = PacketToBytes(reverseName);
        var ensName = await contract.Read<string>("reverse", reverseNameBytes).ConfigureAwait(false);
        _ensCache[address] = ensName;
        return ensName;
    }

    public static async Task<string> GetAddressFromENS(ThirdwebClient client, string ensName)
    {
        if (string.IsNullOrEmpty(ensName))
        {
            throw new ArgumentNullException(nameof(ensName));
        }

        if (ensName.IsValidAddress())
        {
            return ensName.ToChecksumAddress();
        }

        if (!ensName.Contains('.'))
        {
            throw new ArgumentException("Invalid ENS name.");
        }

        if (_ensCache.TryGetValue(ensName, out var value))
        {
            return value;
        }

        var registry = await ThirdwebContract.Create(client: client, address: Constants.ENS_REGISTRY_ADDRESS, chain: 1).ConfigureAwait(false);
        var functionCallEncoder = new FunctionCallEncoder();
        var encodedAddr = "addr(bytes32)"
            .HashMessage()
            .HexToBytes()
            .Take(4)
            .ToArray()
            .Concat(functionCallEncoder.EncodeParameters(new Parameter[] { new("bytes32", "name") }, new object[] { NameHash(ensName).HexToBytes() }))
            .ToArray();
        var result = await registry.Read<ResolveReturnType>("resolve(bytes name, bytes data)", PacketToBytes(ensName), encodedAddr).ConfigureAwait(false);
        var address = ("0x" + result.Bytes.BytesToHex()[26..]).ToChecksumAddress();
        _ensCache[ensName] = address;
        return address;
    }

    public static bool IsValidAddress(this string address)
    {
        if (string.IsNullOrEmpty(address))
        {
            return false;
        }

        if (address.StartsWith("0x") && address.Length == 42 && !address.Contains('.'))
        {
            return true;
        }

        return false;
    }

    [FunctionOutput]
    private class ResolveReturnType
    {
        [Parameter("bytes", "", 1)]
        public byte[] Bytes { get; set; }

        [Parameter("address", "", 2)]
        public string Address { get; set; }
    }

    private static string NameHash(string name)
    {
        var node = "0x0000000000000000000000000000000000000000000000000000000000000000";
        if (!string.IsNullOrEmpty(name))
        {
            name = ENSNormalize.ENSIP15.Normalize(name);
            var labels = name.Split('.');
            for (var i = labels.Length - 1; i >= 0; i--)
            {
                var byteInput = (node + labels[i].HashMessage()).HexToByteArray();
                node = byteInput.HashMessage().BytesToHex();
            }
        }
        return node.EnsureHexPrefix();
    }

    public static async Task<bool> IsDeployed(ThirdwebClient client, BigInteger chainId, string address)
    {
        var rpc = ThirdwebRPC.GetRpcInstance(client, chainId);
        var code = await rpc.SendRequestAsync<string>("eth_getCode", address, "latest");
        return code != "0x";
    }

    public static async Task<BigInteger> FetchGasPrice(ThirdwebClient client, BigInteger chainId, bool withBump = true)
    {
        var rpc = ThirdwebRPC.GetRpcInstance(client, chainId);
        var hex = await rpc.SendRequestAsync<string>("eth_gasPrice").ConfigureAwait(false);
        var gasPrice = hex.HexToBigInt();
        return withBump ? gasPrice * 10 / 9 : gasPrice;
    }

    public static async Task<(BigInteger maxFeePerGas, BigInteger maxPriorityFeePerGas)> FetchGasFees(ThirdwebClient client, BigInteger chainId, bool withBump = true)
    {
        var rpc = ThirdwebRPC.GetRpcInstance(client, chainId);

        // Polygon Mainnet & Amoy
        if (chainId == (BigInteger)137 || chainId == (BigInteger)80002)
        {
            var gasPrice = await FetchGasPrice(client, chainId, withBump).ConfigureAwait(false);
            return (gasPrice * 3 / 2, gasPrice * 4 / 3);
        }

        // Celo Mainnet, Alfajores & Baklava
        if (chainId == (BigInteger)42220 || chainId == (BigInteger)44787 || chainId == (BigInteger)62320)
        {
            var gasPrice = await FetchGasPrice(client, chainId, withBump).ConfigureAwait(false);
            return (gasPrice, gasPrice);
        }

        try
        {
            var block = await rpc.SendRequestAsync<JObject>("eth_getBlockByNumber", "latest", true).ConfigureAwait(false);
            var baseBlockFee = block["baseFeePerGas"]?.ToObject<HexBigInteger>();
            var maxFeePerGas = baseBlockFee.Value * 2;
            var maxPriorityFeePerGas = ((await rpc.SendRequestAsync<HexBigInteger>("eth_maxPriorityFeePerGas").ConfigureAwait(false))?.Value) ?? maxFeePerGas / 2;

            if (maxPriorityFeePerGas > maxFeePerGas)
            {
                maxPriorityFeePerGas = maxFeePerGas / 2;
            }

            return (maxFeePerGas + (maxPriorityFeePerGas * 10 / 9), maxPriorityFeePerGas * 10 / 9);
        }
        catch
        {
            var gasPrice = await FetchGasPrice(client, chainId, withBump).ConfigureAwait(false);
            return (gasPrice, gasPrice);
        }
    }

    public static IThirdwebHttpClient ReconstructHttpClient(IThirdwebHttpClient httpClient, Dictionary<string, string> defaultHeaders = null)
    {
        var reconstructedHttpClient = httpClient.GetType().GetConstructor(Type.EmptyTypes).Invoke(null) as IThirdwebHttpClient;
        if (defaultHeaders != null)
        {
            reconstructedHttpClient.SetHeaders(defaultHeaders ?? httpClient.Headers);
        }
        return reconstructedHttpClient;
    }

    /// <summary>
    /// Gets the social profiles for the given address or ENS.
    /// </summary>
    /// <param name="client">The Thirdweb client.</param>
    /// <param name="addressOrEns">The wallet address or ENS.</param>
    /// <returns>A <see cref="SocialProfiles"/> object containing the social profiles.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the address or ENS is null or empty.</exception>
    /// <exception cref="ArgumentException">Thrown when the address or ENS is invalid.</exception>
    /// <exception cref="Exception">Thrown when the social profiles could not be fetched.</exception>
    public static async Task<SocialProfiles> GetSocialProfiles(ThirdwebClient client, string addressOrEns)
    {
        if (string.IsNullOrEmpty(addressOrEns))
        {
            throw new ArgumentNullException(nameof(addressOrEns));
        }

        if (!addressOrEns.IsValidAddress() && !addressOrEns.Contains('.'))
        {
            throw new ArgumentException("Invalid address or ENS.");
        }

        addressOrEns = await GetAddressFromENS(client, addressOrEns).ConfigureAwait(false) ?? addressOrEns;

        var url = $"{Constants.SOCIAL_API_URL}/v1/profiles/{addressOrEns}";
        var response = await client.HttpClient.GetAsync(url).ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var deserializedResponse = JsonConvert.DeserializeObject<SocialProfileResponse>(json);
        if (deserializedResponse == null || deserializedResponse.Error != null)
        {
            throw new Exception($"Failed to fetch social profiles for address {addressOrEns}. Error: {deserializedResponse?.Error}");
        }

        return new SocialProfiles(deserializedResponse.Data);
    }

    /// <summary>
    /// Preprocesses the typed data JSON to stringify large numbers.
    /// </summary>
    /// <param name="json">The typed data JSON.</param>
    /// <returns>The preprocessed typed data JSON.</returns>
    public static string PreprocessTypedDataJson(string json)
    {
        var jObject = JObject.Parse(json);

        static void StringifyLargeNumbers(JToken token)
        {
            if (token is JObject obj)
            {
                foreach (var property in obj.Properties().ToList())
                {
                    StringifyLargeNumbers(property.Value);
                }
            }
            else if (token is JArray array)
            {
                foreach (var item in array.ToList())
                {
                    StringifyLargeNumbers(item);
                }
            }
            else if (token is JValue value)
            {
                if (value.Type == JTokenType.Integer)
                {
                    try
                    {
                        var bigInt = BigInteger.Parse(value.ToString());
                        if (bigInt > new BigInteger(uint.MaxValue) || bigInt < BigInteger.Zero)
                        {
                            value.Replace(bigInt.ToString());
                        }
                    }
                    catch (FormatException)
                    {
                        // Skip if the value isn't properly formatted as an integer
                    }
                }
            }
        }

        StringifyLargeNumbers(jObject);

        return jObject.ToString();
    }
}
