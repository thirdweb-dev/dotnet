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

internal class PrivateKeyWallet : IWallet
{
    private readonly EthECKey _ecKey;

    internal PrivateKeyWallet(string privateKeyHex)
    {
        if (string.IsNullOrEmpty(privateKeyHex))
        {
            throw new ArgumentNullException(nameof(privateKeyHex), "Private key cannot be null or empty.");
        }

        _ecKey = new EthECKey(privateKeyHex);
    }

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
}
