using System.Numerics;
using Nethereum.ABI.EIP712;
using Nethereum.Hex.HexConvertors.Extensions;
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
            var typedData = GetTypedDefinition_ZkSyncTransaction(domainName, version, chainId, transaction.From);

            var typedDataSigner = new Eip712TypedDataSigner();
            var encodedTypedData = typedDataSigner.EncodeTypedData(transaction, typedData);
            var hash = new Sha3Keccack().CalculateHash(encodedTypedData);
            var signatureHex = await signer.EthSign(hash);
            var signatureRaw = ECDSASignatureFactory.ExtractECDSASignature(signatureHex);
            var serializedTx = SerializeEip712(transaction, signatureRaw, signatureHex, chainId);
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

        public static TypedData<Domain> GetTypedDefinition_ZkSyncTransaction(string domainName, string version, BigInteger chainId, string verifyingContract)
        {
            return new TypedData<Domain>
            {
                Domain = new Domain
                {
                    Name = domainName,
                    Version = version,
                    ChainId = chainId,
                    VerifyingContract = verifyingContract
                },
                Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(Domain), typeof(AccountAbstraction.ZkSyncAATransaction)),
                PrimaryType = nameof(AccountAbstraction.ZkSyncAATransaction),
            };
        }

        private static string SerializeEip712(AccountAbstraction.ZkSyncAATransaction transaction, ECDSASignature signature, string signatureHex, BigInteger chainId)
        {
            if (chainId == 0)
            {
                throw new ArgumentException("Chain ID must be provided for EIP712 transactions!");
            }

            if (string.IsNullOrEmpty(transaction.From))
            {
                throw new ArgumentException("From address must be provided for EIP712 transactions!");
            }

            return "0x71"
                + RLP.EncodeList(
                        transaction.Nonce.ToBytesForRLPEncoding(),
                        transaction.MaxPriorityFeePerGas.ToBytesForRLPEncoding(),
                        transaction.MaxFeePerGas.ToBytesForRLPEncoding(),
                        transaction.GasLimit.ToBytesForRLPEncoding(),
                        transaction.To.ToBytesForRLPEncoding(),
                        transaction.Value.ToBytesForRLPEncoding(),
                        transaction.Data.ToHex().ToBytesForRLPEncoding(),
                        signature.V.ToHex().ToBytesForRLPEncoding(),
                        signature.R.ToByteArray().ToHex().ToBytesForRLPEncoding(),
                        signature.S.ToByteArray().ToHex().ToBytesForRLPEncoding(),
                        chainId.ToBytesForRLPEncoding(),
                        transaction.From.ToBytesForRLPEncoding(),
                        transaction.GasPerPubdataByteLimit.ToBytesForRLPEncoding(),
                        transaction.FactoryDeps.ToHex().ToBytesForRLPEncoding() ?? Array.Empty<byte>().ToHex().ToBytesForRLPEncoding(),
                        signatureHex.ToBytesForRLPEncoding(),
                        transaction.Paymaster.ToBytesForRLPEncoding(),
                        transaction.PaymasterInput.ToHex().ToBytesForRLPEncoding()
                    )
                    .ToHex();
        }
    }
}
