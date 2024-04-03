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
using Thirdweb.EWS;

namespace Thirdweb
{
    public class EmbeddedAccount : IThirdwebAccount
    {
        public ThirdwebAccountType AccountType => ThirdwebAccountType.PrivateKeyAccount;

        private ThirdwebClient _client;
        private EmbeddedWallet _embeddedWallet;
        private User _user;
        private EthECKey _ecKey;
        private string _email;

        public EmbeddedAccount(ThirdwebClient client, string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email must be provided to use Embedded Wallets.");
            }

            _embeddedWallet = new EmbeddedWallet(client);
            _email = email;
            _client = client;
        }

        public async Task Connect()
        {
            try
            {
                _user = await _embeddedWallet.GetUserAsync(_email, "EmailOTP");
                _ecKey = new EthECKey(_user.Account.PrivateKey);
            }
            catch
            {
                Console.WriteLine("User not found. Please call EmbeddedAccount.SendOTP() to initialize the login process.");
                _user = null;
                _ecKey = null;
            }
        }

        #region Email OTP Flow

        public async Task SendOTP()
        {
            if (string.IsNullOrEmpty(_email))
            {
                throw new Exception("Email is required for OTP login");
            }

            try
            {
                (bool isNewUser, bool isNewDevice, bool needsRecoveryCode) = await _embeddedWallet.SendOtpEmailAsync(_email);
                Console.WriteLine("OTP sent to email. Please call EmbeddedAccount.SubmitOTP to login.");
            }
            catch (Exception e)
            {
                throw new Exception("Failed to send OTP email", e);
            }
        }

        public async Task<(string, bool)> SubmitOTP(string otp)
        {
            var res = await _embeddedWallet.VerifyOtpAsync(_email, otp, null);
            if (res.User == null)
            {
                var canRetry = res.CanRetry;
                if (canRetry)
                {
                    Console.WriteLine("Invalid OTP. Please try again.");
                }
                else
                {
                    Console.WriteLine("Invalid OTP. Please request a new OTP.");
                }
                return (null, canRetry);
            }
            else
            {
                _user = res.User;
                _ecKey = new EthECKey(_user.Account.PrivateKey);
                return (await GetAddress(), false);
            }
        }

        #endregion

        public Task<string> GetAddress()
        {
            return Task.FromResult(_ecKey.GetPublicAddress());
        }

        public Task<string> EthSign(string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message), "Message to sign cannot be null.");
            }

            var signer = new MessageSigner();
            var signature = signer.Sign(Encoding.UTF8.GetBytes(message), _ecKey);
            return Task.FromResult(signature);
        }

        public Task<string> PersonalSign(byte[] rawMessage)
        {
            if (rawMessage == null)
            {
                throw new ArgumentNullException(nameof(rawMessage), "Message to sign cannot be null.");
            }

            var signer = new EthereumMessageSigner();
            var signature = signer.Sign(rawMessage, _ecKey);
            return Task.FromResult(signature);
        }

        public Task<string> PersonalSign(string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message), "Message to sign cannot be null.");
            }

            var signer = new EthereumMessageSigner();
            var signature = signer.EncodeUTF8AndSign(message, _ecKey);
            return Task.FromResult(signature);
        }

        public Task<string> SignTypedDataV4(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json), "Json to sign cannot be null.");
            }

            var signer = new Eip712TypedDataSigner();
            var signature = signer.SignTypedDataV4(json, _ecKey);
            return Task.FromResult(signature);
        }

        public Task<string> SignTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "Data to sign cannot be null.");
            }

            var signer = new Eip712TypedDataSigner();
            var signature = signer.SignTypedDataV4(data, typedData, _ecKey);
            return Task.FromResult(signature);
        }

        public async Task<string> SignTransaction(TransactionInput transaction, BigInteger chainId)
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
                var gasPrice = transaction.GasPrice;
                var legacySigner = new LegacyTransactionSigner();
                signedTransaction = legacySigner.SignTransaction(_ecKey.GetPrivateKey(), chainId, transaction.To, value.Value, nonce, gasPrice.Value, gasLimit.Value, transaction.Data);
            }

            return "0x" + signedTransaction;
        }

        public Task<bool> IsConnected()
        {
            return Task.FromResult(_ecKey != null);
        }

        public async Task Disconnect()
        {
            try
            {
                await _embeddedWallet.SignOutAsync();
            }
            catch
            {
                Console.WriteLine("Failed to sign out user. Proceeding anyway.");
            }
            _user = null;
            _ecKey = null;
        }
    }
}
