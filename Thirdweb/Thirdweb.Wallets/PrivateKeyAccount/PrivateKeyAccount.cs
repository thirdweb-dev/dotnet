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

namespace Thirdweb
{
    public class PrivateKeyAccount : IThirdwebAccount
    {
        public ThirdwebAccountType AccountType => ThirdwebAccountType.PrivateKeyAccount;

        protected ThirdwebClient _client;
        protected EthECKey _ecKey;

        protected PrivateKeyAccount(ThirdwebClient client, EthECKey key)
        {
            _client = client;
            _ecKey = key;
        }

        public static Task<PrivateKeyAccount> Create(ThirdwebClient client, string privateKeyHex)
        {
            return string.IsNullOrEmpty(privateKeyHex)
                ? throw new ArgumentNullException(nameof(privateKeyHex), "Private key cannot be null or empty.")
                : Task.FromResult(new PrivateKeyAccount(client, new EthECKey(privateKeyHex)));
        }

        public virtual Task<string> GetAddress()
        {
            return Task.FromResult(_ecKey.GetPublicAddress());
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

        public virtual async Task<string> SignTransaction(TransactionInput transaction, BigInteger chainId)
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
            if (transaction.Type != null && transaction.Type.Value == TransactionType.EIP1559.AsByte())
            {
                if (transaction.MaxPriorityFeePerGas == null || transaction.MaxFeePerGas == null)
                {
                    throw new InvalidOperationException("Transaction MaxPriorityFeePerGas and MaxFeePerGas must be set for EIP-1559 transactions");
                }
                var maxPriorityFeePerGas = transaction.MaxPriorityFeePerGas.Value;
                var maxFeePerGas = transaction.MaxFeePerGas.Value;
                var transaction1559 = new Transaction1559(
                    chainId,
                    nonce,
                    maxPriorityFeePerGas,
                    maxFeePerGas,
                    gasLimit,
                    transaction.To,
                    value,
                    transaction.Data,
                    transaction.AccessList.ToSignerAccessListItemArray()
                );

                var signer = new Transaction1559Signer();
                signer.SignTransaction(_ecKey, transaction1559);
                signedTransaction = transaction1559.GetRLPEncoded().ToHex();
            }
            else
            {
                if (transaction.GasPrice == null)
                {
                    throw new InvalidOperationException("Transaction gas price must be set for legacy transactions");
                }
                var gasPrice = transaction.GasPrice;
                var legacySigner = new LegacyTransactionSigner();
                signedTransaction = legacySigner.SignTransaction(_ecKey.GetPrivateKey(), chainId, transaction.To, value.Value, nonce, gasPrice.Value, gasLimit.Value, transaction.Data);
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
    }
}
