using System.Numerics;
using System.Text;
using Nethereum.ABI.EIP712;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.Model;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Eth.Mappers;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;
using Newtonsoft.Json;

namespace Thirdweb
{
    public class PrivateKeyWallet : IThirdwebWallet
    {
        public ThirdwebAccountType AccountType => ThirdwebAccountType.PrivateKeyAccount;

        protected ThirdwebClient _client;
        protected EthECKey _ecKey;

        protected PrivateKeyWallet(ThirdwebClient client, EthECKey key)
        {
            _client = client;
            _ecKey = key;
        }

        public static Task<PrivateKeyWallet> Create(ThirdwebClient client, string privateKeyHex)
        {
            return string.IsNullOrEmpty(privateKeyHex)
                ? throw new ArgumentNullException(nameof(privateKeyHex), "Private key cannot be null or empty.")
                : Task.FromResult(new PrivateKeyWallet(client, new EthECKey(privateKeyHex)));
        }

        public static Task<PrivateKeyWallet> Generate(ThirdwebClient client)
        {
            return Task.FromResult(new PrivateKeyWallet(client, EthECKey.GenerateKey()));
        }

        public virtual Task<string> GetAddress()
        {
            return Task.FromResult(_ecKey.GetPublicAddress().ToChecksumAddress());
        }

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

        public virtual Task<bool> IsConnected()
        {
            return Task.FromResult(_ecKey != null);
        }

        public virtual Task Disconnect()
        {
            _ecKey = null;
            return Task.CompletedTask;
        }

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

            using var httpClient = httpClientOverride ?? _client.HttpClient;

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

        public Task<string> SendTransaction(ThirdwebTransactionInput transaction)
        {
            throw new InvalidOperationException("SendTransaction is not supported for private key wallets, please use the unified Contract or ThirdwebTransaction APIs.");
        }
    }
}
