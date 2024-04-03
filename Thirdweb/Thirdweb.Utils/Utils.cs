using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Newtonsoft.Json;

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

        public static async Task<TransactionReceipt> GetTransactionReceipt(ThirdwebClient client, BigInteger chainId, string txHash, CancellationToken cancellationToken = default)
        {
            var rpc = ThirdwebRPC.GetRpcInstance(client, chainId);
            var receipt = await rpc.SendRequestAsync<TransactionReceipt>("eth_getTransactionReceipt", txHash).ConfigureAwait(false);
            while (receipt == null)
            {
                if (cancellationToken != CancellationToken.None)
                {
                    await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                }
                else
                {
                    await Task.Delay(1000, CancellationToken.None).ConfigureAwait(false);
                }

                receipt = await rpc.SendRequestAsync<TransactionReceipt>("eth_getTransactionReceipt", txHash).ConfigureAwait(false);
            }

            if (receipt.Failed())
            {
                throw new Exception($"Transaction {txHash} execution reverted.");
            }

            var userOpEvent = receipt.DecodeAllEvents<AccountAbstraction.UserOperationEventEventDTO>();
            if (userOpEvent != null && userOpEvent.Count > 0 && userOpEvent[0].Event.Success == false)
            {
                var revertReasonEvent = receipt.DecodeAllEvents<AccountAbstraction.UserOperationRevertReasonEventDTO>();
                if (revertReasonEvent != null && revertReasonEvent.Count > 0)
                {
                    var revertReason = revertReasonEvent[0].Event.RevertReason;
                    var revertReasonString = new FunctionCallDecoder().DecodeFunctionErrorMessage(revertReason.ToHex(true));
                    throw new Exception($"Transaction {txHash} execution silently reverted: {revertReasonString}");
                }
                else
                {
                    throw new Exception($"Transaction {txHash} execution silently reverted with no reason string");
                }
            }

            return receipt;
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

        public static long GetUnixTimeStampNow()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public static long GetUnixTimeStampIn10Years()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60 * 60 * 24 * 365 * 10;
        }
    }
}
