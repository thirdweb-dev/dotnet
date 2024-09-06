using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Nethereum.ABI.EIP712;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Nethereum.Util;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Nethereum.Hex.HexTypes;

namespace Thirdweb
{
    /// <summary>
    /// Provides utility methods for various operations.
    /// </summary>
    public static class Utils
    {
        private static readonly Dictionary<BigInteger, bool> Eip155EnforcedCache = new Dictionary<BigInteger, bool>();
        private static readonly Dictionary<BigInteger, ThirdwebChainData> ChainDataCache = new Dictionary<BigInteger, ThirdwebChainData>();

        /// <summary>
        /// Computes the client ID from the given secret key.
        /// </summary>
        /// <param name="secretKey">The secret key.</param>
        /// <returns>The computed client ID.</returns>
        public static string ComputeClientIdFromSecretKey(string secretKey)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(secretKey));
            return BitConverter.ToString(hash).Replace("-", "").ToLower().Substring(0, 32);
        }

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
            return Sha3Keccack.Current.CalculateHash(Encoding.UTF8.GetBytes(message)).ToHex(true);
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
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60 * 60 * 24 * 365 * 10;
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
        /// <param name="chainId">The chain ID.</param>
        /// <returns>True if it is a zkSync chain ID, otherwise false.</returns>
        public static bool IsZkSync(BigInteger chainId)
        {
            return chainId.Equals(324) || chainId.Equals(300) || chainId.Equals(302);
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

        public static async Task<ThirdwebChainData> FetchThirdwebChainDataAsync(ThirdwebClient client, BigInteger chainId)
        {
            if (ChainDataCache.ContainsKey(chainId))
            {
                return ChainDataCache[chainId];
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (chainId <= 0)
            {
                throw new ArgumentException("Invalid chain ID.");
            }

            var url = $"https://api.thirdweb-dev.com/v1/chains/{chainId}";
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
                    ChainDataCache[chainId] = deserializedResponse.Data;
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

        private static bool IsReferenceType(string typeName)
        {
            if (!new Regex("bytes\\d+").IsMatch(typeName))
            {
                var input = typeName;
                if (!new Regex("uint\\d+").IsMatch(input))
                {
                    var input2 = typeName;
                    if (!new Regex("int\\d+").IsMatch(input2))
                    {
                        switch (typeName)
                        {
                            case "bytes":
                            case "string":
                            case "bool":
                            case "address":
                                break;
                            default:
                                if (typeName.Contains("["))
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
                else if (type.Contains("["))
                {
                    var jProperty2 = new JProperty(name);
                    var jArray = new JArray();
                    var text = type.Substring(0, type.LastIndexOf("["));
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
            if (Eip155EnforcedCache.ContainsKey(chainId))
            {
                return Eip155EnforcedCache[chainId];
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
                    var errorSubstringsComposite = new List<string[]> { new[] { "account", "not found" }, new[] { "wrong", "chainid" } };

                    result = errorSubstringsComposite.Any(arr => arr.All(substring => errorMsg.Contains(substring)));
                }
            }

            Eip155EnforcedCache[chainId] = result;
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

        public static async Task<ThirdwebTransactionReceipt> DeployEntryPoint(ThirdwebClient client, BigInteger chainId, int entryPointVersion, BigInteger? gasLimitOverride = null)
        {
            var entryPointAddress = entryPointVersion == 7 ? "0x0000000071727De22E5E9d8BAf0edAc6f37da032" : "0x5FF137D4b0FDCD49DcA30c7CF57E578a026d2789";
            if (await IsDeployed(client, chainId, entryPointAddress))
            {
                throw new Exception($"Entry point already deployed at {entryPointAddress}.");
            }

            var arachnid = "0x4e59b44847b379578588920cA78FbF26c0B4956C";
            var arachnidDeployer = "0x3fab184622dc19b6109349b94811493bf2a45362";
            var arachnidDeployed = await IsDeployed(client, chainId, arachnid);
            Console.WriteLine($"Arachnid deployed: {arachnidDeployed}");

            if (!arachnidDeployed && await IsEip155Enforced(client, chainId))
            {
                var arachnidDeployerBalance = await ThirdwebExtensions.GetBalanceRaw(client, chainId, arachnidDeployer);
                var eip155StrictlyEnforced = arachnidDeployerBalance >= 10000000000000000;
                if (eip155StrictlyEnforced)
                {
                    throw new Exception("EIP-155 Strictly Enforced, Cannot Deploy Create2Factory.");
                }
                else
                {
                    throw new Exception(
                        $"EIP-155 might be enforced on this chain. Try funding {arachnidDeployer} with 0.01 ETH or equivalent and call this method again.\n"
                            + "If this keeps failing, then EIP-155 is strictly enforced and you must contact protocol devs."
                    );
                }
            }

            var privateKeyWallet = await PrivateKeyWallet.Create(client: client, privateKeyHex: "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80");
            var walletAddress = await privateKeyWallet.GetAddress();
            Console.WriteLine($"Deployer: {walletAddress}");

            var balance = await privateKeyWallet.GetBalance(chainId);
            Console.WriteLine($"Balance: {balance.ToString().ToEth()}");
            if (balance <= 0)
            {
                throw new Exception("Insufficient balance to deploy entry point.");
            }

            var rpc = ThirdwebRPC.GetRpcInstance(client, chainId);
            var nonce = new HexBigInteger(await rpc.SendRequestAsync<string>("eth_getTransactionCount", walletAddress, "pending"));
            var gasPrice = new HexBigInteger(await rpc.SendRequestAsync<string>("eth_gasPrice"));
            var gasLimit = gasLimitOverride ?? 6600000;

            var finalData = entryPointVersion == 7 ? Constants.ENTRY_POINT_07_SALT + Constants.ENTRY_POINT_07_DATA : Constants.ENTRY_POINT_06_DATA;
            var signedTx = privateKeyWallet.SignTransactionLegacy(to: arachnid, value: 0, nonce: nonce, gasPrice: gasPrice, gas: gasLimit, data: finalData, chainId: null);

            var hash = await rpc.SendRequestAsync<string>("eth_sendRawTransaction", signedTx);
            Console.WriteLine($"Transaction hash: {hash}");

            return await ThirdwebTransaction.WaitForTransactionReceipt(client, chainId, hash);
        }

        public static async Task<bool> IsDeployed(ThirdwebClient client, BigInteger chainId, string address)
        {
            var rpc = ThirdwebRPC.GetRpcInstance(client, chainId);
            var code = await rpc.SendRequestAsync<string>("eth_getCode", address, "latest");
            return code != "0x";
        }
    }
}
