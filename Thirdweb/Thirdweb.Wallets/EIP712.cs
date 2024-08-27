using System.Numerics;
using Nethereum.ABI.EIP712;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Model;
using Nethereum.RLP;
using Nethereum.Signer;

namespace Thirdweb;

/// <summary>
/// Provides methods for generating and signing EIP712 compliant messages and transactions.
/// </summary>
public static class EIP712
{
    #region Generation

    /// <summary>
    /// Generates a signature for a smart account permission request.
    /// </summary>
    /// <param name="domainName">The domain name.</param>
    /// <param name="version">The version.</param>
    /// <param name="chainId">The chain ID.</param>
    /// <param name="verifyingContract">The verifying contract.</param>
    /// <param name="signerPermissionRequest">The signer permission request.</param>
    /// <param name="signer">The wallet signer.</param>
    /// <returns>The generated signature.</returns>
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

    /// <summary>
    /// Generates a signature for a smart account message.
    /// </summary>
    /// <param name="domainName">The domain name.</param>
    /// <param name="version">The version.</param>
    /// <param name="chainId">The chain ID.</param>
    /// <param name="verifyingContract">The verifying contract.</param>
    /// <param name="message">The message to sign.</param>
    /// <param name="signer">The wallet signer.</param>
    /// <returns>The generated signature.</returns>
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

    /// <summary>
    /// Generates a signature for a zkSync transaction.
    /// </summary>
    /// <param name="domainName">The domain name.</param>
    /// <param name="version">The version.</param>
    /// <param name="chainId">The chain ID.</param>
    /// <param name="transaction">The zkSync transaction.</param>
    /// <param name="signer">The wallet signer.</param>
    /// <returns>The generated signature.</returns>
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

    /// <summary>
    /// Generates a signature for a minimal forwarder request.
    /// </summary>
    /// <param name="domainName">The domain name.</param>
    /// <param name="version">The version.</param>
    /// <param name="chainId">The chain ID.</param>
    /// <param name="verifyingContract">The verifying contract.</param>
    /// <param name="forwardRequest">The forward request.</param>
    /// <param name="signer">The wallet signer.</param>
    /// <returns>The generated signature.</returns>
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

    /// <summary>
    /// Generates a signature for an ERC20 token mint request.
    /// </summary>
    /// <param name="domainName">The domain name.</param>
    /// <param name="version">The version.</param>
    /// <param name="chainId">The chain ID.</param>
    /// <param name="verifyingContract">The verifying contract.</param>
    /// <param name="mintRequest">The mint request.</param>
    /// <param name="signer">The wallet signer.</param>
    /// <returns>The generated signature.</returns>
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

    /// <summary>
    /// Generates a signature for an ERC721 token mint request.
    /// </summary>
    /// <param name="domainName">The domain name.</param>
    /// <param name="version">The version.</param>
    /// <param name="chainId">The chain ID.</param>
    /// <param name="verifyingContract">The verifying contract.</param>
    /// <param name="mintRequest">The mint request.</param>
    /// <param name="signer">The wallet signer.</param>
    /// <returns>The generated signature.</returns>
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

    /// <summary>
    /// Generates a signature for an ERC1155 token mint request.
    /// </summary>
    /// <param name="domainName">The domain name.</param>
    /// <param name="version">The version.</param>
    /// <param name="chainId">The chain ID.</param>
    /// <param name="verifyingContract">The verifying contract.</param>
    /// <param name="mintRequest">The mint request.</param>
    /// <param name="signer">The wallet signer.</param>
    /// <returns>The generated signature.</returns>
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

    /// <summary>
    /// Gets the typed data definition for a smart account permission request.
    /// </summary>
    /// <param name="domainName">The domain name.</param>
    /// <param name="version">The version.</param>
    /// <param name="chainId">The chain ID.</param>
    /// <param name="verifyingContract">The verifying contract.</param>
    /// <returns>The typed data definition.</returns>
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

    /// <summary>
    /// Gets the typed data definition for a smart account message.
    /// </summary>
    /// <param name="domainName">The domain name.</param>
    /// <param name="version">The version.</param>
    /// <param name="chainId">The chain ID.</param>
    /// <param name="verifyingContract">The verifying contract.</param>
    /// <returns>The typed data definition.</returns>
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

    /// <summary>
    /// Gets the typed data definition for a zkSync transaction.
    /// </summary>
    /// <param name="domainName">The domain name.</param>
    /// <param name="version">The version.</param>
    /// <param name="chainId">The chain ID.</param>
    /// <returns>The typed data definition.</returns>
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

    /// <summary>
    /// Gets the typed data definition for a TokenERC20 mint request.
    /// </summary>
    /// <param name="domainName">The domain name.</param>
    /// <param name="version">The version.</param>
    /// <param name="chainId">The chain ID.</param>
    /// <param name="verifyingContract">The verifying contract.</param>
    /// <returns>The typed data definition.</returns>
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

    /// <summary>
    /// Gets the typed data definition for a TokenERC721 mint request.
    /// </summary>
    /// <param name="domainName">The domain name.</param>
    /// <param name="version">The version.</param>
    /// <param name="chainId">The chain ID.</param>
    /// <param name="verifyingContract">The verifying contract.</param>
    /// <returns>The typed data definition.</returns>
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

    /// <summary>
    /// Gets the typed data definition for a TokenERC1155 mint request.
    /// </summary>
    /// <param name="domainName">The domain name.</param>
    /// <param name="version">The version.</param>
    /// <param name="chainId">The chain ID.</param>
    /// <param name="verifyingContract">The verifying contract.</param>
    /// <returns>The typed data definition.</returns>
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

    /// <summary>
    /// Gets the typed data definition for a minimal forwarder request.
    /// </summary>
    /// <param name="domainName">The domain name.</param>
    /// <param name="version">The version.</param>
    /// <param name="chainId">The chain ID.</param>
    /// <param name="verifyingContract">The verifying contract.</param>
    /// <returns>The typed data definition.</returns>
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

    private static readonly int[] _indexOfListDataItems = new int[] { 13, 15 };

    /// <summary>
    /// Serializes an EIP712 zkSync transaction.
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    /// <param name="signature">The ECDSA signature.</param>
    /// <param name="chainId">The chain ID.</param>
    /// <returns>The serialized transaction.</returns>
    private static string SerializeEip712(AccountAbstraction.ZkSyncAATransaction transaction, EthECDSASignature signature, BigInteger chainId)
    {
        if (chainId == 0)
        {
            throw new ArgumentException("Chain ID must be provided for EIP712 transactions!");
        }

        var fields = new List<byte[]>
        {
            transaction.Nonce == 0 ? Array.Empty<byte>() : transaction.Nonce.ToByteArray(isUnsigned: true, isBigEndian: true),
            transaction.MaxPriorityFeePerGas == 0 ? Array.Empty<byte>() : transaction.MaxPriorityFeePerGas.ToByteArray(isUnsigned: true, isBigEndian: true),
            transaction.MaxFeePerGas.ToByteArray(isUnsigned: true, isBigEndian: true),
            transaction.GasLimit.ToByteArray(isUnsigned: true, isBigEndian: true),
            transaction.To.ToByteArray(isUnsigned: true, isBigEndian: true),
            transaction.Value == 0 ? Array.Empty<byte>() : transaction.Value.ToByteArray(isUnsigned: true, isBigEndian: true),
            transaction.Data ?? Array.Empty<byte>(),
            signature.IsVSignedForYParity() ? new byte[] { 0x1b } : new byte[] { 0x1c },
            signature.R,
            signature.S,
            chainId.ToByteArray(isUnsigned: true, isBigEndian: true),
            transaction.From.ToByteArray(isUnsigned: true, isBigEndian: true),
            // Add meta
            transaction.GasPerPubdataByteLimit.ToByteArray(isUnsigned: true, isBigEndian: true),
            Array.Empty<byte>(), // TODO: FactoryDeps
            signature.CreateStringSignature().HexToByteArray(),
            // add array of rlp encoded paymaster/paymasterinput
            transaction.Paymaster != 0
                ? RLP.EncodeElement(transaction.Paymaster.ToByteArray(isUnsigned: true, isBigEndian: true)).Concat(RLP.EncodeElement(transaction.PaymasterInput)).ToArray()
                : new byte[] { 0xc0 }
        };

        return "0x71" + RLP.EncodeDataItemsAsElementOrListAndCombineAsList(fields.ToArray(), _indexOfListDataItems).ToHex();
    }

    #endregion
}
