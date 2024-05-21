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
            var typedDataEncoder = new Eip712TypedDataEncoder();
            var hash = typedDataEncoder.EncodeAndHashTypedData(transaction, typedData);
            Console.WriteLine($"Hash: {hash.ToHex(true)}");
            var signatureHex = await signer.EthSign(hash);
            Console.WriteLine($"Signature: {signatureHex}");
            var signatureRaw = EthECDSASignatureFactory.ExtractECDSASignature(signatureHex);
            var serializedTx = SerializeEip712(transaction, signatureRaw, chainId);
            Console.WriteLine($"Serialized: {serializedTx}");
            return serializedTx;
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

            if (string.IsNullOrEmpty(transaction.From))
            {
                throw new ArgumentException("From address must be provided for EIP712 transactions!");
            }

            var fields = new List<byte[]>
            {
                transaction.Nonce.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.MaxPriorityFeePerGas.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.MaxFeePerGas.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.GasLimit.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.To.HexToByteArray(),
                transaction.Value == 0 ? new byte[0] : transaction.Value.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.Data == null ? new byte[0] : transaction.Data,
            };

            fields.Add(signature.IsVSignedForYParity() ? new byte[] { 0x1b } : new byte[] { });
            fields.Add(new BigInteger(signature.R).ToByteArray(isUnsigned: false, isBigEndian: true));
            fields.Add(new BigInteger(signature.S).ToByteArray(isUnsigned: false, isBigEndian: true));

            fields.Add(chainId.ToByteArray(isUnsigned: true, isBigEndian: true));
            fields.Add(transaction.From.HexToByteArray());

            // Add meta
            fields.Add(transaction.GasPerPubdataByteLimit.ToByteArray(isUnsigned: true, isBigEndian: true));
            fields.Add(new byte[] { }); // TODO: FactoryDeps
            fields.Add(signature.CreateStringSignature().HexToByteArray());

            fields.Add(transaction.Paymaster.HexToByteArray());
            fields.Add(transaction.PaymasterInput);

            // 0x71f901240c84017d784084017d78408401312d009483e13cd6b1179be8b8cb5858accbba84394cf9a7808080a0149015c6a2073d376059808a4302d736093646fff75e3b0b44e1f73a41bd00b2a08e4f98ece9fab20e1621e9ad7efe34561ff00d94392afd20469bdacbdd7f660a82012c9483e13cd6b1179be8b8cb5858accbba84394cf9a782c350c0b841b200bd413af7e1440b3b5ef7ff46360936d702438a805960373d07a2c61590140a667fddcbda9b4620fd2a39940df01f5634fe7eade921160eb2fae9ec984f8e1b94ba226d47cbb2731cbaa67c916c57d68484aa269fb8448c5a344500000000000000000000000000000000000000000000000000000000000000200000000000000000000000000000000000000000000000000000000000000000

            // 0x71f901260c84017d784084017d78408401312d009483e13cd6b1179be8b8cb5858accbba84394cf9a7808080a02ce7dea3c25ac28c69ef5d425933bf6195c7c5648e4e228e3dca1f62f147449ea06b626df4ccad17b8c472ddba39b113c0e8f49569572fdd4ac2f6e2ddfc29726682012c9483e13cd6b1179be8b8cb5858accbba84394cf9a782c350c0b8412ce7dea3c25ac28c69ef5d425933bf6195c7c5648e4e228e3dca1f62f147449e6b626df4ccad17b8c472ddba39b113c0e8f49569572fdd4ac2f6e2ddfc2972661bf85b94ba226d47cbb2731cbaa67c916c57d68484aa269fb8448c5a344500000000000000000000000000000000000000000000000000000000000000200000000000000000000000000000000000000000000000000000000000000000

            return "0x71" + RLP.EncodeDataItemsAsElementOrListAndCombineAsList(fields.ToArray(), new int[] { 13 }).ToHex(); // 13 = FactoryDeps
        }
    }
}
