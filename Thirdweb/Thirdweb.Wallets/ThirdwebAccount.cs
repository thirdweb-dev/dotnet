using System.Numerics;
using Nethereum.ABI.EIP712;
using Nethereum.RPC.Eth.DTOs;

namespace Thirdweb
{
    public class ThirdwebAccount
    {
        internal ThirdwebAccountOptions Options { get; private set; }

        public IWallet Wallet { get; private set; }

        public ThirdwebAccount(ThirdwebAccountOptions options)
        {
            if (options.Client == null)
            {
                throw new ArgumentException("Client must be provided");
            }

            Options = options;

            Wallet = Options.Type switch
            {
                WalletType.PrivateKey => new PrivateKey(Options.PrivateKey),
                WalletType.Embedded => new Embedded(Options.Client, Options.Email),
                _ => throw new ArgumentException("Invalid wallet type"),
            };
        }

        public async Task Initialize()
        {
            await Wallet.Initialize();
        }

        public string GetAddress()
        {
            return Wallet.GetAddress();
        }

        public string EthSign(string message)
        {
            return Wallet.EthSign(message);
        }

        public string PersonalSign(string message)
        {
            return Wallet.PersonalSign(message);
        }

        public string SignTypedDataV4(string json)
        {
            return Wallet.SignTypedDataV4(json);
        }

        public string SignTypedDataV4<T>(T data, TypedData<T> typedData)
        {
            return Wallet.SignTypedDataV4(data, typedData);
        }

        public string SignTransaction(TransactionInput transaction, BigInteger chainId)
        {
            return Wallet.SignTransaction(transaction, chainId);
        }

        public bool IsConnected()
        {
            return Wallet.IsConnected();
        }

        public async Task Disconnect()
        {
            await Wallet.Disconnect();
        }
    }
}
