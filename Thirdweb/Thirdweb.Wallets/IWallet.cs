using System.Numerics;
using Nethereum.ABI.EIP712;
using Nethereum.RPC.Eth.DTOs;

namespace Thirdweb
{
    internal interface IWallet
    {
        string GetAddress();
        string EthSign(string message);
        string PersonalSign(string message);
        string SignTypedDataV4(string json);
        string SignTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData);
        string SignTransaction(TransactionInput transaction, BigInteger chainId);
    }
}
