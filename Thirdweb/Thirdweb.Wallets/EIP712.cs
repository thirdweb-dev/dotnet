using System.Numerics;
using Nethereum.ABI.EIP712;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Model;
using Nethereum.RLP;
using Nethereum.Signer;

namespace Thirdweb
{
    public static class EIP712
    {
        #region Generation

        public static async Task<string> GenerateSignature_SmartAccount(
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

        public static async Task<string> GenerateSignature_SmartAccount_AccountMessage(
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

        public static async Task<string> GenerateSignature_MinimalForwarder(
            string domainName,
            string version,
            BigInteger chainId,
            string verifyingContract,
            Forwarder_ForwardRequest forwardRequest,
            IThirdwebWallet signer
        )
        {
            var typedData = GetTypedDefinition_MinimalForwarder(domainName, version, chainId, verifyingContract);
            return await signer.SignTypedDataV4(forwardRequest, typedData);
        }

        public static async Task<string> GenerateSignature_TokenERC20(
            string domainName,
            string version,
            BigInteger chainId,
            string verifyingContract,
            TokenERC20_MintRequest mintRequest,
            IThirdwebWallet signer
        )
        {
            var typedData = GetTypedDefinition_TokenERC20(domainName, version, chainId, verifyingContract);
            return await signer.SignTypedDataV4(mintRequest, typedData);
        }

        public static async Task<string> GenerateSignature_TokenERC721(
            string domainName,
            string version,
            BigInteger chainId,
            string verifyingContract,
            TokenERC721_MintRequest mintRequest,
            IThirdwebWallet signer
        )
        {
            var typedData = GetTypedDefinition_TokenERC721(domainName, version, chainId, verifyingContract);
            return await signer.SignTypedDataV4(mintRequest, typedData);
        }

        public static async Task<string> GenerateSignature_TokenERC1155(
            string domainName,
            string version,
            BigInteger chainId,
            string verifyingContract,
            TokenERC1155_MintRequest mintRequest,
            IThirdwebWallet signer
        )
        {
            var typedData = GetTypedDefinition_TokenERC1155(domainName, version, chainId, verifyingContract);
            return await signer.SignTypedDataV4(mintRequest, typedData);
        }

        #endregion

        #region Typed Definitions

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
                PrimaryType = "SignerPermissionRequest",
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
                PrimaryType = "AccountMessage",
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

        public static TypedData<Domain> GetTypedDefinition_TokenERC20(string domainName, string version, BigInteger chainId, string verifyingContract)
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
                Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(Domain), typeof(TokenERC20_MintRequest)),
                PrimaryType = "MintRequest",
            };
        }

        public static TypedData<Domain> GetTypedDefinition_TokenERC721(string domainName, string version, BigInteger chainId, string verifyingContract)
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
                Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(Domain), typeof(TokenERC721_MintRequest)),
                PrimaryType = "MintRequest",
            };
        }

        public static TypedData<Domain> GetTypedDefinition_TokenERC1155(string domainName, string version, BigInteger chainId, string verifyingContract)
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
                Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(Domain), typeof(TokenERC1155_MintRequest)),
                PrimaryType = "MintRequest",
            };
        }

        public static TypedData<Domain> GetTypedDefinition_MinimalForwarder(string domainName, string version, BigInteger chainId, string verifyingContract)
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
                Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(Domain), typeof(Forwarder_ForwardRequest)),
                PrimaryType = "ForwardRequest",
            };
        }

        #endregion

        #region Helpers

        private static string SerializeEip712(AccountAbstraction.ZkSyncAATransaction transaction, EthECDSASignature signature, BigInteger chainId)
        {
            if (chainId == 0)
            {
                throw new ArgumentException("Chain ID must be provided for EIP712 transactions!");
            }

            var fields = new List<byte[]>
            {
                transaction.Nonce == 0 ? new byte[0] : transaction.Nonce.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.MaxPriorityFeePerGas == 0 ? new byte[0] : transaction.MaxPriorityFeePerGas.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.MaxFeePerGas.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.GasLimit.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.To.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.Value == 0 ? new byte[0] : transaction.Value.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.Data == null ? new byte[0] : transaction.Data,
            };

            fields.Add(signature.IsVSignedForYParity() ? new byte[] { 0x1b } : new byte[] { 0x1c });
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

        #endregion
    }
}
