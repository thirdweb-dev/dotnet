using System.Numerics;
using Nethereum.ABI.EIP712;
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
            var serializedTx = SerializeEip712(transaction, signatureRaw, signatureHex, chainId);
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
                PrimaryType = nameof(AccountAbstraction.ZkSyncAATransaction),
            };
        }

        private static string SerializeEip712(AccountAbstraction.ZkSyncAATransaction transaction, EthECDSASignature signature, string signatureHex, BigInteger chainId)
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

            fields.Add(new BigInteger(signature.V).ToByteArray(isUnsigned: false, isBigEndian: true));
            fields.Add(new BigInteger(signature.R).ToByteArray(isUnsigned: false, isBigEndian: true));
            fields.Add(new BigInteger(signature.S).ToByteArray(isUnsigned: false, isBigEndian: true));

            fields.Add(chainId.ToByteArray(isUnsigned: true, isBigEndian: true));
            fields.Add(transaction.From.HexToByteArray());

            // Add meta
            fields.Add(transaction.GasPerPubdataByteLimit.ToByteArray(isUnsigned: true, isBigEndian: true));
            fields.Add(new byte[] { }); // TODO: FactoryDeps
            fields.Add(signatureHex.HexToByteArray());

            fields.Add(transaction.Paymaster.HexToByteArray());
            fields.Add(transaction.PaymasterInput);

            // 0x71f901240c84017d784084017d78408401312d009483e13cd6b1179be8b8cb5858accbba84394cf9a780801ca095bdae3d9ee4919b95ccb65008fb834b876cf6daab54f08914a3682b692dac3ba007de707e93d03249ffcaac274caf27e513cbe96d78d24c0136ab8a2aae94a66782012c9483e13cd6b1179be8b8cb5858accbba84394cf9a782c350c0b8413bac2d692b68a31489f054abdaf66c874b83fb0850b6cc959b91e49e3daebd9567a694ae2a8aab36014cd2786de9cb13e527af4c27accaff4932d0937e70de071c94ba226d47cbb2731cbaa67c916c57d68484aa269fb8448c5a344500000000000000000000000000000000000000000000000000000000000000200000000000000000000000000000000000000000000000000000000000000000

            // 0x71f901260c84017d784084017d78408401312d009483e13cd6b1179be8b8cb5858accbba84394cf9a7808080a02ce7dea3c25ac28c69ef5d425933bf6195c7c5648e4e228e3dca1f62f147449ea06b626df4ccad17b8c472ddba39b113c0e8f49569572fdd4ac2f6e2ddfc29726682012c9483e13cd6b1179be8b8cb5858accbba84394cf9a782c350c0b8412ce7dea3c25ac28c69ef5d425933bf6195c7c5648e4e228e3dca1f62f147449e6b626df4ccad17b8c472ddba39b113c0e8f49569572fdd4ac2f6e2ddfc2972661bf85b94ba226d47cbb2731cbaa67c916c57d68484aa269fb8448c5a344500000000000000000000000000000000000000000000000000000000000000200000000000000000000000000000000000000000000000000000000000000000

            return "0x71" + RLP.EncodeDataItemsAsElementOrListAndCombineAsList(fields.ToArray(), new int[] { 13 }).ToHex(); // 13 = FactoryDeps
        }
    }
}
