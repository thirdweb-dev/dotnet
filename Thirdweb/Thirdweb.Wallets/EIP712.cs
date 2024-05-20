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

        public static async Task<string> GenerateSignature_ZkSyncTransaction(string domainName, string version, BigInteger chainId, dynamic transaction, IThirdwebWallet signer)
        {
            var typedData = GetTypedDefinition_ZkSyncTransaction(domainName, version, chainId);

            var typedDataSigner = new Eip712TypedDataSigner();
            var encodedTypedData = typedDataSigner.EncodeTypedData(transaction, typedData);

            var hash = Utils.HashMessage(encodedTypedData);
            var signature = await signer.EthSign(hash);
            var signatureHex = signature.ToHex();

            transaction.customData = new
            {
                gasPerPubdata = transaction.gasPerPubdataByteLimit,
                paymasterParams = transaction.paymasterParams,
                customSignature = signatureHex,
            };

            var serializedTx = SerializeEip712(transaction, signature);
            Console.WriteLine($"Serialized tx: {serializedTx}");
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

        private static string SerializeEip712(dynamic transaction, EthECDSASignature signature)
        {
            if (transaction.chainId == null)
            {
                throw new ArgumentException("Transaction chainId isn't set!");
            }

            if (transaction.from == null)
            {
                throw new ArgumentException("Explicitly providing `from` field is required for EIP712 transactions!");
            }

            var fields = new List<byte[]>
            {
                RLP.EncodeElement(transaction.nonce.ToByteArray()),
                RLP.EncodeElement(transaction.maxPriorityFeePerGas.ToByteArray()),
                RLP.EncodeElement(transaction.maxFeePerGas.ToByteArray()),
                RLP.EncodeElement(transaction.gasLimit.ToByteArray()),
                RLP.EncodeElement(transaction.to.HexToByteArray()),
                RLP.EncodeElement(transaction.value.ToByteArray()),
                RLP.EncodeElement(transaction.data.HexToByteArray())
            };

            if (signature != null)
            {
                fields.Add(RLP.EncodeElement(signature.V));
                fields.Add(RLP.EncodeElement(signature.R));
                fields.Add(RLP.EncodeElement(signature.S));
            }
            else
            {
                fields.Add(RLP.EncodeElement(transaction.chainId.ToByteArray()));
                fields.Add(RLP.EncodeElement(Array.Empty<byte>()));
                fields.Add(RLP.EncodeElement(Array.Empty<byte>()));
            }

            fields.Add(RLP.EncodeElement(transaction.chainId.ToByteArray()));
            fields.Add(RLP.EncodeElement(transaction.from.HexToByteArray()));

            // Add meta
            fields.Add(RLP.EncodeElement(transaction.customData.gasPerPubdata.ToByteArray()));
            fields.Add(RLP.EncodeList(Array.Empty<byte>()));

            if (transaction.customData.customSignature.Length == 0)
            {
                throw new ArgumentException("Empty signatures are not supported!");
            }
            fields.Add(RLP.EncodeElement(transaction.customData.customSignature.HexToByteArray()));

            if (transaction.customData.paymasterParams != null)
            {
                fields.Add(
                    RLP.EncodeList(
                        new byte[]
                        {
                            RLP.EncodeElement(transaction.customData.paymasterParams.paymaster.HexToByteArray()),
                            RLP.EncodeElement(transaction.customData.paymasterParams.paymasterInput.HexToByteArray())
                        }
                    )
                );
            }
            else
            {
                fields.Add(RLP.EncodeList(Array.Empty<byte>()));
            }

            var rlpEncoded = RLP.EncodeList(fields.ToArray());
            return "0x" + BitConverter.ToString(rlpEncoded).Replace("-", "");
        }
    }
}
