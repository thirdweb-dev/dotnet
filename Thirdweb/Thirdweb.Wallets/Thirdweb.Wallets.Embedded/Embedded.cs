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
using Thirdweb;
using Thirdweb.EWS;

public class Embedded : IWallet
{
    EmbeddedWallet _embeddedWallet;
    User _user;
    EthECKey _ecKey;
    string _email;

    internal Embedded(ThirdwebClient client, string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            throw new ArgumentException("Email must be provided to use Embedded Wallets.");
        }

        _embeddedWallet = new EmbeddedWallet(client);
        _email = email;
    }

    public async Task Initialize()
    {
        try
        {
            _user = await _embeddedWallet.GetUserAsync(_email, "EmailOTP");
            _ecKey = new EthECKey(_user.Account.PrivateKey);
        }
        catch
        {
            Console.WriteLine("User not found. Please call Embedded.LoginWithOTP() to create a new user.");
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
            Console.WriteLine("OTP sent to email. Please call SubmitOTP to login.");
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
            bool canRetry = res.CanRetry;
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
            return (GetAddress(), false);
        }
    }

    #endregion

    public string GetAddress()
    {
        return _ecKey.GetPublicAddress();
    }

    public string EthSign(string message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message), "Message to sign cannot be null.");
        }

        var signer = new MessageSigner();
        var signature = signer.Sign(Encoding.UTF8.GetBytes(message), _ecKey);
        return signature;
    }

    public string PersonalSign(string message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message), "Message to sign cannot be null.");
        }

        var signer = new EthereumMessageSigner();
        var signature = signer.EncodeUTF8AndSign(message, _ecKey);
        return signature;
    }

    public string SignTypedDataV4(string json)
    {
        if (json == null)
        {
            throw new ArgumentNullException(nameof(json), "Json to sign cannot be null.");
        }

        var signer = new Eip712TypedDataSigner();
        var signature = signer.SignTypedDataV4(json, _ecKey);
        return signature;
    }

    public string SignTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data), "Data to sign cannot be null.");
        }

        var signer = new Eip712TypedDataSigner();
        var signature = signer.SignTypedDataV4(data, typedData, _ecKey);
        return signature;
    }

    public string SignTransaction(TransactionInput transaction, BigInteger chainId)
    {
        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        if (string.IsNullOrWhiteSpace(transaction.From))
        {
            transaction.From = GetAddress();
        }
        else if (transaction.From != GetAddress())
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

    public bool IsConnected()
    {
        return _ecKey != null;
    }

    public async Task Disconnect()
    {
        await _embeddedWallet.SignOutAsync();
        _user = null;
        _ecKey = null;
    }
}
