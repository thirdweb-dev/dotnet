using System.Numerics;
using Nethereum.ABI.EIP712;
using Nethereum.RPC.Eth.DTOs;

namespace Thirdweb
{
    public interface IWallet
    {
        internal Task Initialize();
        internal string GetAddress();
        internal string EthSign(string message);
        internal string PersonalSign(string message);
        internal string SignTypedDataV4(string json);
        internal string SignTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData);
        internal string SignTransaction(TransactionInput transaction, BigInteger chainId);
        internal bool IsConnected();
        internal Task Disconnect();
    }
}
