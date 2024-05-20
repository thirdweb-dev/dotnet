using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Nethereum.Util;

namespace Thirdweb
{
    public static class Utils
    {
        public static string ComputeClientIdFromSecretKey(string secretKey)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(secretKey));
            return BitConverter.ToString(hash).Replace("-", "").ToLower().Substring(0, 32);
        }

        public static string HexConcat(params string[] hexStrings)
        {
            var hex = new StringBuilder("0x");

            foreach (var hexStr in hexStrings)
            {
                _ = hex.Append(hexStr[2..]);
            }

            return hex.ToString();
        }

        public static byte[] HashPrefixedMessage(this byte[] messageBytes)
        {
            var signer = new EthereumMessageSigner();
            return signer.HashPrefixedMessage(messageBytes);
        }

        public static string HashPrefixedMessage(this string message)
        {
            var signer = new EthereumMessageSigner();
            return signer.HashPrefixedMessage(Encoding.UTF8.GetBytes(message)).ToHex(true);
        }

        public static byte[] HashMessage(this byte[] messageBytes)
        {
            return Sha3Keccack.Current.CalculateHash(messageBytes);
        }

        public static string HashMessage(this string message)
        {
            return Sha3Keccack.Current.CalculateHash(Encoding.UTF8.GetBytes(message)).ToHex(true);
        }

        public static string BytesToHex(byte[] bytes)
        {
            return bytes.ToHex(true);
        }

        public static byte[] HexToBytes(string hex)
        {
            return hex.HexToByteArray();
        }

        public static string StringToHex(string str)
        {
            return "0x" + Encoding.UTF8.GetBytes(str).ToHex();
        }

        public static string HexToString(string hex)
        {
            var array = HexToBytes(hex);
            return Encoding.UTF8.GetString(array, 0, array.Length);
        }

        public static long GetUnixTimeStampNow()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public static long GetUnixTimeStampIn10Years()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60 * 60 * 24 * 365 * 10;
        }

        public static string ReplaceIPFS(this string uri, string gateway = null)
        {
            gateway ??= Constants.FALLBACK_IPFS_GATEWAY;
            return !string.IsNullOrEmpty(uri) && uri.StartsWith("ipfs://") ? uri.Replace("ipfs://", gateway) : uri;
        }

        public static Dictionary<string, string> GetThirdwebHeaders(ThirdwebClient client)
        {
            var headers = new Dictionary<string, string>
            {
                { "x-sdk-name", "Thirdweb.NET" },
                { "x-sdk-os", System.Runtime.InteropServices.RuntimeInformation.OSDescription },
                { "x-sdk-platform", "dotnet" },
                { "x-sdk-version", Constants.VERSION }
            };

            if (!string.IsNullOrEmpty(client.ClientId))
            {
                headers.Add("x-client-id", client.ClientId);
            }

            if (!string.IsNullOrEmpty(client.SecretKey))
            {
                headers.Add("x-secret-key", client.SecretKey);
            }

            if (!string.IsNullOrEmpty(client.BundleId))
            {
                headers.Add("x-bundle-id", client.BundleId);
            }

            return headers;
        }

        public static string ToWei(this string eth)
        {
            if (!double.TryParse(eth, NumberStyles.Number, CultureInfo.InvariantCulture, out var ethDouble))
            {
                throw new ArgumentException("Invalid eth value.");
            }

            var wei = (BigInteger)(ethDouble * Constants.DECIMALS_18);
            return wei.ToString();
        }

        public static string ToEth(this string wei, int decimalsToDisplay = 4, bool addCommas = false)
        {
            return FormatERC20(wei, decimalsToDisplay, 18, addCommas);
        }

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
    }
}
