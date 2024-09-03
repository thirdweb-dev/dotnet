using System.Numerics;
using System.Text;
using Nethereum.ABI.EIP712;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.Model;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;
using Newtonsoft.Json;

namespace Thirdweb
{
    /// <summary>
    /// Represents a wallet that uses a private key for signing transactions and messages.
    /// </summary>
    public class PrivateKeyWallet : IThirdwebWallet
    {
        /// <summary>
        /// Gets the Thirdweb client associated with the wallet.
        /// </summary>
        public ThirdwebClient Client { get; }

        /// <summary>
        /// Gets the account type of the wallet.
        /// </summary>
        public ThirdwebAccountType AccountType => ThirdwebAccountType.PrivateKeyAccount;

        /// <summary>
        /// The Ethereum EC key used by the wallet.
        /// </summary>
        protected EthECKey _ecKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyWallet"/> class.
        /// </summary>
        /// <param name="client">The Thirdweb client.</param>
        /// <param name="key">The Ethereum EC key.</param>
        protected PrivateKeyWallet(ThirdwebClient client, EthECKey key)
        {
            Client = client;
            _ecKey = key;
        }

        /// <summary>
        /// Creates a new instance of <see cref="PrivateKeyWallet"/> using the specified private key.
        /// </summary>
        /// <param name="client">The Thirdweb client.</param>
        /// <param name="privateKeyHex">The private key in hexadecimal format.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="PrivateKeyWallet"/>.</returns>
        public static Task<PrivateKeyWallet> Create(ThirdwebClient client, string privateKeyHex)
        {
            return string.IsNullOrEmpty(privateKeyHex)
                ? throw new ArgumentNullException(nameof(privateKeyHex), "Private key cannot be null or empty.")
                : Task.FromResult(new PrivateKeyWallet(client, new EthECKey(privateKeyHex)));
        }

        /// <summary>
        /// Generates a new instance of <see cref="PrivateKeyWallet"/> with a random private key.
        /// </summary>
        /// <param name="client">The Thirdweb client.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="PrivateKeyWallet"/>.</returns>
        public static Task<PrivateKeyWallet> Generate(ThirdwebClient client)
        {
            return Task.FromResult(new PrivateKeyWallet(client, EthECKey.GenerateKey()));
        }

        /// <summary>
        /// Gets the address of the wallet.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the address of the wallet.</returns>
        public virtual Task<string> GetAddress()
        {
            return Task.FromResult(_ecKey.GetPublicAddress().ToChecksumAddress());
        }

        /// <summary>
        /// Signs a message using the wallet's private key.
        /// </summary>
        /// <param name="rawMessage">The message to sign.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the signed message.</returns>
        public virtual Task<string> EthSign(byte[] rawMessage)
        {
            if (rawMessage == null)
            {
                throw new ArgumentNullException(nameof(rawMessage), "Message to sign cannot be null.");
            }

            var signer = new MessageSigner();
            var signature = signer.Sign(rawMessage, _ecKey);
            return Task.FromResult(signature);
        }

        /// <summary>
        /// Signs a message using the wallet's private key.
        /// </summary>
        /// <param name="message">The message to sign.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the signed message.</returns>
        public virtual Task<string> EthSign(string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message), "Message to sign cannot be null.");
            }

            var signer = new MessageSigner();
            var signature = signer.Sign(Encoding.UTF8.GetBytes(message), _ecKey);
            return Task.FromResult(signature);
        }

        /// <summary>
        /// Recovers the address from a signed message using Ethereum's signing method.
        /// </summary>
        /// <param name="message">The UTF-8 encoded message.</param>
        /// <param name="signature">The signature.</param>
        /// <returns>The recovered address.</returns>
        /// <exception cref="ArgumentNullException"></exception>
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

        /// <summary>
        /// Signs a message using the wallet's private key with personal sign.
        /// </summary>
        /// <param name="rawMessage">The message to sign.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the signed message.</returns>
        public virtual Task<string> PersonalSign(byte[] rawMessage)
        {
            if (rawMessage == null)
            {
                throw new ArgumentNullException(nameof(rawMessage), "Message to sign cannot be null.");
            }

            var signer = new EthereumMessageSigner();
            var signature = signer.Sign(rawMessage, _ecKey);
            return Task.FromResult(signature);
        }

        /// <summary>
        /// Signs a message using the wallet's private key with personal sign.
        /// </summary>
        /// <param name="message">The message to sign.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the signed message.</returns>
        public virtual Task<string> PersonalSign(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message), "Message to sign cannot be null.");
            }

            var signer = new EthereumMessageSigner();
            var signature = signer.EncodeUTF8AndSign(message, _ecKey);
            return Task.FromResult(signature);
        }

        /// <summary>
        /// Recovers the address from a signed message using personal signing.
        /// </summary>
        /// <param name="message">The UTF-8 encoded and prefixed message.</param>
        /// <param name="signature">The signature.</param>
        /// <returns>The recovered address.</returns>
        /// <exception cref="ArgumentNullException"></exception>
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

        /// <summary>
        /// Signs typed data (EIP-712) using the wallet's private key.
        /// </summary>
        /// <param name="json">The JSON string representing the typed data.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the signed data.</returns>
        public virtual Task<string> SignTypedDataV4(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException(nameof(json), "Json to sign cannot be null.");
            }

            var signer = new Eip712TypedDataSigner();
            var signature = signer.SignTypedDataV4(json, _ecKey);
            return Task.FromResult(signature);
        }

        /// <summary>
        /// Signs typed data (EIP-712) using the wallet's private key.
        /// </summary>
        /// <typeparam name="T">The type of the data to sign.</typeparam>
        /// <typeparam name="TDomain">The type of the domain.</typeparam>
        /// <param name="data">The data to sign.</param>
        /// <param name="typedData">The typed data.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the signed data.</returns>
        public virtual Task<string> SignTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData)
            where TDomain : IDomain
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "Data to sign cannot be null.");
            }

            var signer = new Eip712TypedDataSigner();
            var signature = signer.SignTypedDataV4(data, typedData, _ecKey);
            return Task.FromResult(signature);
        }

        /// <summary>
        /// Recovers the address from a signed message using typed data (version 4).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TDomain"></typeparam>
        /// <param name="data">The data to sign.</param>
        /// <param name="typedData">The typed data.</param>
        /// <param name="signature">The signature.</param>
        /// <returns>The recovered address.</returns>
        /// <exception cref="ArgumentNullException"></exception>
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

        public string SignTransactionLegacy(string to, BigInteger value, BigInteger nonce, BigInteger gasPrice, BigInteger gas, string data, BigInteger? chainId)
        {
            var rawSigner = new LegacyTransactionSigner();
            if (chainId == null)
            {
                var signedTx = rawSigner.SignTransaction(privateKey: _ecKey.GetPrivateKey(), to: to, amount: value, nonce: nonce, gasPrice: gasPrice, gasLimit: gas, data: data);
                return "0x" + signedTx;
            }
            else
            {
                var signedTx = rawSigner.SignTransaction(
                    privateKey: _ecKey.GetPrivateKeyAsBytes(),
                    chainId: chainId.Value,
                    to: to,
                    amount: value,
                    nonce: nonce,
                    gasPrice: gasPrice,
                    gasLimit: gas,
                    data: data
                );
                return "0x" + signedTx;
            }
        }

        /// <summary>
        /// Signs a transaction using the wallet's private key.
        /// </summary>
        /// <param name="transaction">The transaction to sign.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the signed transaction.</returns>
        public virtual async Task<string> SignTransaction(ThirdwebTransactionInput transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            if (string.IsNullOrWhiteSpace(transaction.From))
            {
                transaction.From = await GetAddress();
            }
            else if (transaction.From != await GetAddress())
            {
                throw new Exception("Transaction 'From' address does not match the wallet address");
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
                    _ecKey.GetPrivateKey(),
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
                signer.SignTransaction(_ecKey, transaction1559);
                signedTransaction = transaction1559.GetRLPEncoded().ToHex();
            }

            return "0x" + signedTransaction;
        }

        /// <summary>
        /// Checks if the wallet is connected.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the wallet is connected.</returns>
        public virtual Task<bool> IsConnected()
        {
            return Task.FromResult(_ecKey != null);
        }

        /// <summary>
        /// Disconnects the wallet.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public virtual Task Disconnect()
        {
            _ecKey = null;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Authenticates the user by signing a message with the wallet's private key.
        /// </summary>
        /// <param name="domain">The domain for authentication.</param>
        /// <param name="chainId">The chain ID.</param>
        /// <param name="authPayloadPath">The authentication payload path.</param>
        /// <param name="authLoginPath">The authentication login path.</param>
        /// <param name="httpClientOverride">Optional HTTP client override.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the authentication response.</returns>
        public virtual async Task<string> Authenticate(
            string domain,
            BigInteger chainId,
            string authPayloadPath = "/auth/payload",
            string authLoginPath = "/auth/login",
            IThirdwebHttpClient httpClientOverride = null
        )
        {
            var payloadURL = domain + authPayloadPath;
            var loginURL = domain + authLoginPath;

            var payloadBodyRaw = new { address = await GetAddress(), chainId = chainId.ToString() };
            var payloadBody = JsonConvert.SerializeObject(payloadBodyRaw);

            var httpClient = httpClientOverride ?? Client.HttpClient;

            var payloadContent = new StringContent(payloadBody, Encoding.UTF8, "application/json");
            var payloadResponse = await httpClient.PostAsync(payloadURL, payloadContent);
            _ = payloadResponse.EnsureSuccessStatusCode();
            var payloadString = await payloadResponse.Content.ReadAsStringAsync();

            var loginBodyRaw = JsonConvert.DeserializeObject<LoginPayload>(payloadString);
            var payloadToSign = Utils.GenerateSIWE(loginBodyRaw.payload);

            loginBodyRaw.signature = await PersonalSign(payloadToSign);
            var loginBody = JsonConvert.SerializeObject(new { payload = loginBodyRaw });

            var loginContent = new StringContent(loginBody, Encoding.UTF8, "application/json");
            var loginResponse = await httpClient.PostAsync(loginURL, loginContent);
            _ = loginResponse.EnsureSuccessStatusCode();
            var responseString = await loginResponse.Content.ReadAsStringAsync();
            return responseString;
        }

        /// <summary>
        /// Throws an exception because sending transactions is not supported for private key wallets.
        /// </summary>
        /// <param name="transaction">The transaction to send.</param>
        /// <returns>Throws an InvalidOperationException.</returns>
        public Task<string> SendTransaction(ThirdwebTransactionInput transaction)
        {
            throw new InvalidOperationException("SendTransaction is not supported for private key wallets, please use the unified Contract or ThirdwebTransaction APIs.");
        }
    }
}
