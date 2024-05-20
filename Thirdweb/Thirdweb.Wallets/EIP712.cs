using System.Numerics;
using Nethereum.ABI.EIP712;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;
using Newtonsoft.Json;

namespace Thirdweb
{
    public static class EIP712
    {
        public async static Task<string> GenerateSignature_SmartAccount(
            string domainName,
            string version,
            BigInteger chainId,
            string verifyingContract,
            AccountAbstraction.SignerPermissionRequest signerPermissionRequest,
            IThirdwebWallet signer
        )
        {
            var typedData = GetTypedDefinition_SmartAccount(domainName, version, chainId, verifyingContract);
            return await signer.SignTypedDataV4(signerPermissionRequest, typedData);
        }

        public async static Task<string> GenerateSignature_SmartAccount_AccountMessage(
            string domainName,
            string version,
            BigInteger chainId,
            string verifyingContract,
            byte[] message,
            IThirdwebWallet signer
        )
        {
            var typedData = GetTypedDefinition_SmartAccount_AccountMessage(domainName, version, chainId, verifyingContract);
            var accountMessage = new AccountAbstraction.AccountMessage { Message = message };
            return await signer.SignTypedDataV4(accountMessage, typedData);
        }

        public static async Task<string> GenerateSignature_ZkSyncTransaction(
            string domainName,
            string version,
            BigInteger chainId,
            AccountAbstraction.ZkSyncAATransaction transaction,
            IThirdwebWallet signer
        )
        {
            var typedData = GetTypedDefinition_ZkSyncTransaction(domainName, version, chainId);
            return await signer.SignTypedDataV4(transaction, typedData);
        }

        // private static EthECDSASignature ParseSignature(string signatureHex)
        // {
        //     var signatureBytes = signatureHex.HexToByteArray();
        //     if (signatureBytes.Length != 65)
        //     {
        //         throw new ArgumentException("Invalid signature length.");
        //     }

        //     var r = new byte[32];
        //     var s = new byte[32];
        //     var v = new byte[1];

        //     Array.Copy(signatureBytes, 0, r, 0, 32);
        //     Array.Copy(signatureBytes, 32, s, 0, 32);
        //     Array.Copy(signatureBytes, 64, v, 0, 1);

        //     var vValue = (v[0] == 0 || v[0] == 1) ? v[0] + 27 : v[0]; // Adjust v value if necessary

        //     return new EthECDSASignature(new Org.BouncyCastle.Math.BigInteger(r), new Org.BouncyCastle.Math.BigInteger(s), vValue.ToBytesForRLPEncoding());
        // }

        public static TypedData<Domain> GetTypedDefinition_SmartAccount(string domainName, string version, BigInteger chainId, string verifyingContract)
        {
            return new TypedData<Domain>
            {
                Domain = new Domain
                {
                    Name = domainName,
                    Version = version,
                    ChainId = chainId,
                    VerifyingContract = verifyingContract,
                },
                Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(Domain), typeof(AccountAbstraction.SignerPermissionRequest)),
                PrimaryType = nameof(AccountAbstraction.SignerPermissionRequest),
            };
        }

        public static TypedData<Domain> GetTypedDefinition_SmartAccount_AccountMessage(string domainName, string version, BigInteger chainId, string verifyingContract)
        {
            return new TypedData<Domain>
            {
                Domain = new Domain
                {
                    Name = domainName,
                    Version = version,
                    ChainId = chainId,
                    VerifyingContract = verifyingContract,
                },
                Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(Domain), typeof(AccountAbstraction.AccountMessage)),
                PrimaryType = nameof(AccountAbstraction.AccountMessage),
            };
        }

        public static TypedData<Domain> GetTypedDefinition_ZkSyncTransaction(string domainName, string version, BigInteger chainId)
        {
            return new TypedData<Domain>
            {
                Domain = new Domain
                {
                    Name = domainName,
                    Version = version,
                    ChainId = chainId
                },
                Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(Domain), typeof(AccountAbstraction.ZkSyncAATransaction)),
                PrimaryType = nameof(AccountAbstraction.ZkSyncAATransaction)
            };
        }

        // private static string SerializeEip712(AccountAbstraction.ZkSyncAATransaction transaction, EthECDSASignature signature, BigInteger chainId)
        // {
        //     if (transaction.From == null)
        //     {
        //         throw new ArgumentException("Explicitly providing `from` field is required for EIP712 transactions!");
        //     }

        //     var fields = new List<byte[]>
        //     {
        //         RLP.EncodeElement(transaction.Nonce.ToByteArray()),
        //         RLP.EncodeElement(transaction.MaxPriorityFeePerGas.ToByteArray()),
        //         RLP.EncodeElement(transaction.MaxFeePerGas.ToByteArray()),
        //         RLP.EncodeElement(transaction.GasLimit.ToByteArray()),
        //         RLP.EncodeElement(transaction.To.HexToByteArray()),
        //         RLP.EncodeElement(transaction.Value.ToByteArray()),
        //         RLP.EncodeElement(transaction.Data)
        //     };

        //     if (signature != null)
        //     {
        //         fields.Add(RLP.EncodeElement(signature.V));
        //         fields.Add(RLP.EncodeElement(signature.R));
        //         fields.Add(RLP.EncodeElement(signature.S));
        //     }
        //     else
        //     {
        //         fields.Add(RLP.EncodeElement(chainId.ToByteArray()));
        //         fields.Add(RLP.EncodeElement(Array.Empty<byte>()));
        //         fields.Add(RLP.EncodeElement(Array.Empty<byte>()));
        //     }

        //     fields.Add(RLP.EncodeElement(chainId.ToByteArray()));
        //     fields.Add(RLP.EncodeElement(transaction.From.HexToByteArray()));

        //     // Add meta
        //     fields.Add(RLP.EncodeElement(transaction.GasPerPubdataByteLimit.ToByteArray()));
        //     fields.Add(RLP.EncodeList(Array.Empty<byte[]>()));

        //     if (transaction.PaymasterInput.Length == 0)
        //     {
        //         throw new ArgumentException("Empty signatures are not supported!");
        //     }
        //     fields.Add(RLP.EncodeElement(transaction.PaymasterInput));

        //     if (transaction.Paymaster != null)
        //     {
        //         fields.Add(RLP.EncodeList(new byte[][] { RLP.EncodeElement(transaction.Paymaster.HexToByteArray()), RLP.EncodeElement(transaction.PaymasterInput) }));
        //     }
        //     else
        //     {
        //         fields.Add(RLP.EncodeList(Array.Empty<byte[]>()));
        //     }

        //     var rlpEncoded = RLP.EncodeList(fields.ToArray());
        //     return "0x" + BitConverter.ToString(rlpEncoded).Replace("-", "");
        // }
    }
}
