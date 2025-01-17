using System.Numerics;
using System.Text;
using Nethereum.ABI.EIP712;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;

namespace Thirdweb;

/// <summary>
/// Represents a wallet that uses a private key for signing transactions and messages.
/// </summary>
public class PrivateKeyWallet : IThirdwebWallet
{
    public ThirdwebClient Client { get; }

    public ThirdwebAccountType AccountType => ThirdwebAccountType.PrivateKeyAccount;

    protected EthECKey EcKey { get; set; }

    protected PrivateKeyWallet(ThirdwebClient client, EthECKey key)
    {
        this.Client = client;
        this.EcKey = key;
    }

    /// <summary>
    /// Creates a new instance of <see cref="PrivateKeyWallet"/> using the provided private key.
    /// </summary>
    /// <param name="client">The Thirdweb client instance.</param>
    /// <param name="privateKeyHex">The private key in hexadecimal format.</param>
    /// <returns>A new instance of <see cref="PrivateKeyWallet"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the private key is null or empty.</exception>
    public static Task<PrivateKeyWallet> Create(ThirdwebClient client, string privateKeyHex)
    {
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client));
        }

        if (string.IsNullOrEmpty(privateKeyHex))
        {
            throw new ArgumentNullException(nameof(privateKeyHex), "Private key cannot be null or empty.");
        }

        return Task.FromResult(new PrivateKeyWallet(client, new EthECKey(privateKeyHex)));
    }

    #region PrivateKeyWallet Specific

    /// <summary>
    /// Generates a new instance of <see cref="PrivateKeyWallet"/> with a new private key.
    /// </summary>
    /// <param name="client">The Thirdweb client instance.</param>
    /// <returns>A new instance of <see cref="PrivateKeyWallet"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the client is null.</exception>
    public static Task<PrivateKeyWallet> Generate(ThirdwebClient client)
    {
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client));
        }

        return Task.FromResult(new PrivateKeyWallet(client, EthECKey.GenerateKey()));
    }

    /// <summary>
    /// Loads a saved instance of <see cref="PrivateKeyWallet"/> from the local storage or generates an ephemeral one if not found.
    /// </summary>
    /// <param name="client">The Thirdweb client instance.</param>
    /// <returns>A new instance of <see cref="PrivateKeyWallet"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the client is null.</exception>
    public static async Task<PrivateKeyWallet> LoadOrGenerate(ThirdwebClient client)
    {
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client));
        }

        var path = GetSavePath();

        if (File.Exists(path))
        {
            var privateKey = await File.ReadAllTextAsync(path);
            return new PrivateKeyWallet(client, new EthECKey(privateKey));
        }
        else
        {
            return await Generate(client);
        }
    }

    /// <summary>
    /// Gets the path to the file where a PrivateKeyWallet would be saved if PrivateKeyWallet.Save() is called.
    /// </summary>
    /// <returns>The path to the file.</returns>
    public static string GetSavePath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Thirdweb", "PrivateKeyWallet", "private_key_wallet.txt");
    }

    /// <summary>
    /// Saves the private key to the local storage.
    /// </summary>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the wallet does not have a private key.</exception>
    public async Task Save()
    {
        if (this.EcKey == null)
        {
            throw new InvalidOperationException("Cannot save wallet without a private key.");
        }

        var filePath = GetSavePath();
        var directoryPath = Path.GetDirectoryName(filePath);

        if (!Directory.Exists(directoryPath))
        {
            _ = Directory.CreateDirectory(directoryPath);
        }

        await File.WriteAllTextAsync(filePath, this.EcKey.GetPrivateKey());
    }

    /// <summary>
    /// Deletes the saved private key from the local storage if it exists.
    /// </summary>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public static void Delete()
    {
        var path = GetSavePath();

        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Exports the private key as a hexadecimal string.
    /// </summary>
    /// <returns>The private key as a hexadecimal string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the wallet does not have a private key.</exception>
    public async Task<string> Export()
    {
        if (this.EcKey == null)
        {
            throw new InvalidOperationException("Cannot export private key without a private key.");
        }

        return await Task.FromResult(this.EcKey.GetPrivateKey());
    }

    #endregion

    #region IThirdwebWallet

    public virtual Task<string> GetAddress()
    {
        return Task.FromResult(this.EcKey.GetPublicAddress().ToChecksumAddress());
    }

    public virtual Task<string> EthSign(byte[] rawMessage)
    {
        if (rawMessage == null)
        {
            throw new ArgumentNullException(nameof(rawMessage), "Message to sign cannot be null.");
        }

        var signer = new MessageSigner();
        var signature = signer.Sign(rawMessage, this.EcKey);
        return Task.FromResult(signature);
    }

    public virtual Task<string> EthSign(string message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message), "Message to sign cannot be null.");
        }

        var signer = new MessageSigner();
        var signature = signer.Sign(Encoding.UTF8.GetBytes(message), this.EcKey);
        return Task.FromResult(signature);
    }

    public virtual Task<string> RecoverAddressFromEthSign(string message, string signature)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message), "Message to sign cannot be null.");
        }

        if (signature == null)
        {
            throw new ArgumentNullException(nameof(signature), "Signature cannot be null.");
        }

        var signer = new MessageSigner();
        var address = signer.EcRecover(Encoding.UTF8.GetBytes(message), signature);
        return Task.FromResult(address);
    }

    public virtual Task<string> PersonalSign(byte[] rawMessage)
    {
        if (rawMessage == null)
        {
            throw new ArgumentNullException(nameof(rawMessage), "Message to sign cannot be null.");
        }

        var signer = new EthereumMessageSigner();
        var signature = signer.Sign(rawMessage, this.EcKey);
        return Task.FromResult(signature);
    }

    public virtual Task<string> PersonalSign(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentNullException(nameof(message), "Message to sign cannot be null.");
        }

        var signer = new EthereumMessageSigner();
        var signature = signer.EncodeUTF8AndSign(message, this.EcKey);
        return Task.FromResult(signature);
    }

    public virtual Task<string> RecoverAddressFromPersonalSign(string message, string signature)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentNullException(nameof(message), "Message to sign cannot be null.");
        }

        if (string.IsNullOrEmpty(signature))
        {
            throw new ArgumentNullException(nameof(signature), "Signature cannot be null.");
        }

        var signer = new EthereumMessageSigner();
        var address = signer.EncodeUTF8AndEcRecover(message, signature);
        return Task.FromResult(address);
    }

    public virtual Task<string> SignTypedDataV4(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            throw new ArgumentNullException(nameof(json), "Json to sign cannot be null.");
        }

        var signer = new Eip712TypedDataSigner();
        var signature = signer.SignTypedDataV4(json, this.EcKey);
        return Task.FromResult(signature);
    }

    public virtual Task<string> SignTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData)
        where TDomain : IDomain
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data), "Data to sign cannot be null.");
        }

        var signer = new Eip712TypedDataSigner();
        var signature = signer.SignTypedDataV4(data, typedData, this.EcKey);
        return Task.FromResult(signature);
    }

    public virtual Task<string> RecoverAddressFromTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData, string signature)
        where TDomain : IDomain
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data), "Data to sign cannot be null.");
        }

        if (typedData == null)
        {
            throw new ArgumentNullException(nameof(typedData), "Typed data cannot be null.");
        }

        if (signature == null)
        {
            throw new ArgumentNullException(nameof(signature), "Signature cannot be null.");
        }

        var signer = new Eip712TypedDataSigner();
        var address = signer.RecoverFromSignatureV4(data, typedData, signature);
        return Task.FromResult(address);
    }

    public virtual Task<string> SignTransaction(ThirdwebTransactionInput transaction)
    {
        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        if (transaction.Nonce == null)
        {
            throw new ArgumentNullException(nameof(transaction), "Transaction nonce has not been set");
        }

        string signedTransaction;

        if (transaction.GasPrice != null)
        {
            var legacySigner = new LegacyTransactionSigner();
            signedTransaction = legacySigner.SignTransaction(
                this.EcKey.GetPrivateKey(),
                transaction.ChainId.Value,
                transaction.To,
                transaction.Value.Value,
                transaction.Nonce.Value,
                transaction.GasPrice.Value,
                transaction.Gas.Value,
                transaction.Data
            );
        }
        else
        {
            if (transaction.MaxPriorityFeePerGas == null || transaction.MaxFeePerGas == null)
            {
                throw new InvalidOperationException("Transaction MaxPriorityFeePerGas and MaxFeePerGas must be set for EIP-1559 transactions");
            }

            var encodedData = new List<byte[]>
            {
                RLP.EncodeElement(transaction.ChainId.Value.ToByteArrayForRLPEncoding()),
                RLP.EncodeElement(transaction.Nonce.Value.ToByteArrayForRLPEncoding()),
                RLP.EncodeElement(transaction.MaxPriorityFeePerGas.Value.ToByteArrayForRLPEncoding()),
                RLP.EncodeElement(transaction.MaxFeePerGas.Value.ToByteArrayForRLPEncoding()),
                RLP.EncodeElement(transaction.Gas.Value.ToByteArrayForRLPEncoding()),
                RLP.EncodeElement(transaction.To.HexToBytes()),
                RLP.EncodeElement(transaction.Value.Value.ToByteArrayForRLPEncoding()),
                RLP.EncodeElement(transaction.Data == null ? Array.Empty<byte>() : transaction.Data.HexToBytes()),
                new byte[] { 0xc0 }, // AccessList, empty so short list bytes
            };

            if (transaction.AuthorizationList != null)
            {
                var encodedAuthorizationList = new List<byte[]>();
                foreach (var authorizationList in transaction.AuthorizationList)
                {
                    var encodedItem = new List<byte[]>()
                    {
                        RLP.EncodeElement(authorizationList.ChainId.HexToNumber().ToByteArrayForRLPEncoding()),
                        RLP.EncodeElement(authorizationList.Address.HexToBytes()),
                        RLP.EncodeElement(authorizationList.Nonce.HexToNumber().ToByteArrayForRLPEncoding()),
                        RLP.EncodeElement(authorizationList.YParity == "0x00" ? Array.Empty<byte>() : authorizationList.YParity.HexToBytes()),
                        RLP.EncodeElement(authorizationList.R.HexToBytes().TrimZeroes()),
                        RLP.EncodeElement(authorizationList.S.HexToBytes().TrimZeroes())
                    };
                    encodedAuthorizationList.Add(RLP.EncodeList(encodedItem.ToArray()));
                }
                encodedData.Add(RLP.EncodeList(encodedAuthorizationList.ToArray()));
            }

            var encodedBytes = RLP.EncodeList(encodedData.ToArray());
            var returnBytes = new byte[encodedBytes.Length + 1];
            Array.Copy(encodedBytes, 0, returnBytes, 1, encodedBytes.Length);
            returnBytes[0] = transaction.AuthorizationList != null ? (byte)0x04 : (byte)0x02;

            var rawHash = Utils.HashMessage(returnBytes);
            var rawSignature = this.EcKey.SignAndCalculateYParityV(rawHash);

            byte[] v;
            byte[] r;
            byte[] s;
            if (rawSignature.V.Length == 0 || rawSignature.V[0] == 0)
            {
                v = Array.Empty<byte>();
            }
            else
            {
                v = rawSignature.V;
            }
            v = RLP.EncodeElement(v);
            r = RLP.EncodeElement(rawSignature.R.TrimZeroes());
            s = RLP.EncodeElement(rawSignature.S.TrimZeroes());

            encodedData.Add(v);
            encodedData.Add(r);
            encodedData.Add(s);

            encodedBytes = RLP.EncodeList(encodedData.ToArray());
            returnBytes = new byte[encodedBytes.Length + 1];
            Array.Copy(encodedBytes, 0, returnBytes, 1, encodedBytes.Length);
            returnBytes[0] = transaction.AuthorizationList != null ? (byte)0x04 : (byte)0x02;

            // (var tx, var sig) = Utils.DecodeTransaction(returnBytes);

            signedTransaction = returnBytes.ToHex();
            // Console.WriteLine(signedTransaction);

            // (var tx, var sig) = Utils.DecodeTransaction("0x" + signedTransaction);
        }

        return Task.FromResult("0x" + signedTransaction);
    }

    public virtual Task<bool> IsConnected()
    {
        return Task.FromResult(this.EcKey != null);
    }

    public virtual Task Disconnect()
    {
        this.EcKey = null;
        return Task.CompletedTask;
    }

    public virtual Task<string> SendTransaction(ThirdwebTransactionInput transaction)
    {
        throw new InvalidOperationException("SendTransaction is not supported for private key wallets, please use the unified Contract or ThirdwebTransaction APIs.");
    }

    public virtual Task<ThirdwebTransactionReceipt> ExecuteTransaction(ThirdwebTransactionInput transactionInput)
    {
        throw new InvalidOperationException("ExecuteTransaction is not supported for private key wallets, please use the unified Contract or ThirdwebTransaction APIs.");
    }

    public virtual Task<List<LinkedAccount>> LinkAccount(
        IThirdwebWallet walletToLink,
        string otp = null,
        bool? isMobile = null,
        Action<string> browserOpenAction = null,
        string mobileRedirectScheme = "thirdweb://",
        IThirdwebBrowser browser = null,
        BigInteger? chainId = null,
        string jwt = null,
        string payload = null,
        string defaultSessionIdOverride = null,
        List<string> forceWalletIds = null
    )
    {
        throw new InvalidOperationException("LinkAccount is not supported for private key wallets.");
    }

    public virtual Task<List<LinkedAccount>> GetLinkedAccounts()
    {
        throw new InvalidOperationException("GetLinkedAccounts is not supported for private key wallets.");
    }

    public Task<List<LinkedAccount>> UnlinkAccount(LinkedAccount accountToUnlink)
    {
        throw new InvalidOperationException("UnlinkAccount is not supported for private key wallets.");
    }

    public async Task<EIP7702Authorization> SignAuthorization(BigInteger chainId, string contractAddress, bool willSelfExecute)
    {
        var nonce = await this.GetTransactionCount(chainId);
        if (willSelfExecute)
        {
            nonce++;
        }
        var encodedData = new List<byte[]>
        {
            RLP.EncodeElement(chainId.ToByteArrayForRLPEncoding()),
            RLP.EncodeElement(contractAddress.HexToBytes()),
            RLP.EncodeElement(nonce.ToByteArrayForRLPEncoding())
        };
        var encodedBytes = RLP.EncodeList(encodedData.ToArray());
        var returnElements = new byte[encodedBytes.Length + 1];
        Array.Copy(encodedBytes.ToArray(), 0, returnElements, 1, encodedBytes.Length);
        returnElements[0] = 0x05;
        var authorizationHash = Utils.HashMessage(returnElements);
        var authorizationSignature = this.EcKey.SignAndCalculateYParityV(authorizationHash);
        return new EIP7702Authorization(chainId, contractAddress, nonce, authorizationSignature.V, authorizationSignature.R, authorizationSignature.S);
    }

    public Task SwitchNetwork(BigInteger chainId)
    {
        return Task.CompletedTask;
    }

    #endregion
}
