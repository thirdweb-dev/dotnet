using System.Numerics;
using Nethereum.ABI.EIP712;
using Nethereum.RPC.Eth.DTOs;

namespace Thirdweb
{
    public class ThirdwebAccount
    {
        internal ThirdwebAccountOptions Options { get; private set; }

        private IWallet _wallet;

        public ThirdwebAccount(ThirdwebAccountOptions options)
        {
            if (options.Client == null)
            {
                throw new ArgumentException("Client must be provided");
            }

            Options = options;

            InitializeWallet();
        }

        private void InitializeWallet()
        {
            _wallet = Options.Type switch
            {
                WalletType.PrivateKey => new PrivateKeyWallet(Options.PrivateKey),
                _ => throw new ArgumentException("Invalid wallet type"),
            };
        }

        public string GetAddress()
        {
            return _wallet.GetAddress();
        }

        public string EthSign(string message)
        {
            return _wallet.EthSign(message);
        }

        public string PersonalSign(string message)
        {
            return _wallet.PersonalSign(message);
        }

        public string SignTypedDataV4(string json)
        {
            return _wallet.SignTypedDataV4(json);
        }

        public string SignTypedDataV4<T>(T data, TypedData<T> typedData)
        {
            return _wallet.SignTypedDataV4(data, typedData);
        }

        public string SignTransaction(TransactionInput transaction, BigInteger chainId)
        {
            return _wallet.SignTransaction(transaction, chainId);
        }
    }
}
