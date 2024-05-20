using System.Numerics;
using Nethereum.ABI.EIP712;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer.EIP712;

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

        public static async Task<string> GenerateSignature_ZkSyncTransaction(string domainName, string version, BigInteger chainId, object transaction, IThirdwebWallet signer)
        {
            var typedData = GetTypedDefinition_ZkSyncTransaction(domainName, version, chainId);

            var typedDataSigner = new Eip712TypedDataSigner();
            var encodedTypedData = typedDataSigner.EncodeTypedData(transaction, typedData);

            var hash = Utils.HashMessage(encodedTypedData);
            return await signer.EthSign(hash);
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
                Types = new Dictionary<string, MemberDescription[]>
                {
                    ["EIP712Domain"] = new[]
                    {
                        new MemberDescription { Name = "name", Type = "string" },
                        new MemberDescription { Name = "version", Type = "string" },
                        new MemberDescription { Name = "chainId", Type = "uint256" }
                    },
                    ["Transaction"] = new[]
                    {
                        new MemberDescription { Name = "txType", Type = "uint256" },
                        new MemberDescription { Name = "from", Type = "address" },
                        new MemberDescription { Name = "to", Type = "address" },
                        new MemberDescription { Name = "gasLimit", Type = "uint256" },
                        new MemberDescription { Name = "gasPerPubdataByteLimit", Type = "uint256" },
                        new MemberDescription { Name = "maxFeePerGas", Type = "uint256" },
                        new MemberDescription { Name = "maxPriorityFeePerGas", Type = "uint256" },
                        new MemberDescription { Name = "paymaster", Type = "address" },
                        new MemberDescription { Name = "nonce", Type = "uint256" },
                        new MemberDescription { Name = "value", Type = "uint256" },
                        new MemberDescription { Name = "data", Type = "bytes" },
                        new MemberDescription { Name = "factoryDeps", Type = "bytes32[]" },
                        new MemberDescription { Name = "paymasterInput", Type = "bytes" }
                    }
                },
                PrimaryType = "Transaction"
            };
        }
    }
}
