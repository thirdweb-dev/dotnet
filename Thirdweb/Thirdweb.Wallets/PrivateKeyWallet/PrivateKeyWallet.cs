using System.Text;
using Nethereum.ABI.EIP712;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.Model;
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

        var nonce = transaction.Nonce ?? throw new ArgumentNullException(nameof(transaction), "Transaction nonce has not been set");

        var gasLimit = transaction.Gas;
        var value = transaction.Value ?? new HexBigInteger(0);

        string signedTransaction;

        if (transaction.GasPrice != null)
        {
            var gasPrice = transaction.GasPrice;
            var legacySigner = new LegacyTransactionSigner();
            signedTransaction = legacySigner.SignTransaction(
                this.EcKey.GetPrivateKey(),
                transaction.ChainId.Value,
                transaction.To,
                value.Value,
                nonce,
                gasPrice.Value,
                gasLimit.Value,
                transaction.Data
            );
        }
        else
        {
            if (transaction.MaxPriorityFeePerGas == null || transaction.MaxFeePerGas == null)
            {
                throw new InvalidOperationException("Transaction MaxPriorityFeePerGas and MaxFeePerGas must be set for EIP-1559 transactions");
            }
            var maxPriorityFeePerGas = transaction.MaxPriorityFeePerGas.Value;
            var maxFeePerGas = transaction.MaxFeePerGas.Value;
            var transaction1559 = new Transaction1559(transaction.ChainId.Value, nonce, maxPriorityFeePerGas, maxFeePerGas, gasLimit, transaction.To, value, transaction.Data, null);

            var signer = new Transaction1559Signer();
            _ = signer.SignTransaction(this.EcKey, transaction1559);
            signedTransaction = transaction1559.GetRLPEncoded().ToHex();
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

    public Task<string> SendTransaction(ThirdwebTransactionInput transaction)
    {
        throw new InvalidOperationException("SendTransaction is not supported for private key wallets, please use the unified Contract or ThirdwebTransaction APIs.");
    }

    public virtual Task<ThirdwebTransactionReceipt> ExecuteTransaction(ThirdwebTransactionInput transactionInput)
    {
        throw new InvalidOperationException("ExecuteTransaction is not supported for private key wallets, please use the unified Contract or ThirdwebTransaction APIs.");
    }

    #endregion
}
