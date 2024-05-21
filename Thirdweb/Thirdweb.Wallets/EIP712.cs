using System.Numerics;
using Nethereum.ABI.EIP712;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.Model;
using Nethereum.RLP;
using Nethereum.Signer;
using Nethereum.Signer.Crypto;
using Nethereum.Signer.EIP712;
using Nethereum.Util;
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
            var signatureHex = await signer.SignTypedDataV4(transaction, typedData);
            var signatureRaw = EthECDSASignatureFactory.ExtractECDSASignature(signatureHex);
            return SerializeEip712(transaction, signatureRaw, chainId);
        }

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

        public static TypedData<DomainWithNameVersionAndChainId> GetTypedDefinition_ZkSyncTransaction(string domainName, string version, BigInteger chainId)
        {
            return new TypedData<DomainWithNameVersionAndChainId>
            {
                Domain = new DomainWithNameVersionAndChainId
                {
                    Name = domainName,
                    Version = version,
                    ChainId = chainId,
                },
                Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(DomainWithNameVersionAndChainId), typeof(AccountAbstraction.ZkSyncAATransaction)),
                PrimaryType = "Transaction",
            };
        }

        private static string SerializeEip712(AccountAbstraction.ZkSyncAATransaction transaction, EthECDSASignature signature, BigInteger chainId)
        {
            if (chainId == 0)
            {
                throw new ArgumentException("Chain ID must be provided for EIP712 transactions!");
            }

            var fields = new List<byte[]>
            {
                transaction.Nonce.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.MaxPriorityFeePerGas.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.MaxFeePerGas.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.GasLimit.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.To.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.Value == 0 ? new byte[0] : transaction.Value.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.Data == null ? new byte[0] : transaction.Data,
            };

            fields.Add(signature.IsVSignedForYParity() ? new byte[] { 0x1b } : new byte[] { });
            fields.Add(signature.R);
            fields.Add(signature.S);

            fields.Add(chainId.ToByteArray(isUnsigned: true, isBigEndian: true));
            fields.Add(transaction.From.ToByteArray(isUnsigned: true, isBigEndian: true));

            // Add meta
            fields.Add(transaction.GasPerPubdataByteLimit.ToByteArray(isUnsigned: true, isBigEndian: true));
            fields.Add(new byte[] { }); // TODO: FactoryDeps
            fields.Add(signature.CreateStringSignature().HexToByteArray());
            // add array of rlp encoded paymaster/paymasterinput
            fields.Add(RLP.EncodeElement(transaction.Paymaster.ToByteArray(isUnsigned: true, isBigEndian: true)).Concat(RLP.EncodeElement(transaction.PaymasterInput)).ToArray());

            return "0x71" + RLP.EncodeDataItemsAsElementOrListAndCombineAsList(fields.ToArray(), new int[] { 13, 15 }).ToHex();
        }
    }
}
