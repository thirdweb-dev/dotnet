using System.Numerics;
using Nethereum.ABI.EIP712;
using Nethereum.RPC.Eth.DTOs;

namespace Thirdweb
{
    public class ThirdwebWallet
    {
        public Dictionary<string, IThirdwebAccount> Accounts { get; }
        public IThirdwebAccount ActiveAccount { get; private set; }

        public ThirdwebWallet()
        {
            Accounts = new Dictionary<string, IThirdwebAccount>();
            ActiveAccount = null;
        }

        public async Task Initialize(List<IThirdwebAccount> accounts)
        {
            if (accounts.Count == 0)
            {
                throw new ArgumentException("At least one account must be provided.");
            }

            for (var i = 0; i < accounts.Count; i++)
            {
                if (!await accounts[i].IsConnected())
                {
                    throw new InvalidOperationException($"Account at index {i} is not connected.");
                }
            }

            foreach (var account in accounts)
            {
                Accounts.Add(await account.GetAddress(), account);
            }

            SetActive(Accounts.Keys.First());
        }

        public void SetActive(string address)
        {
            if (!Accounts.ContainsKey(address))
            {
                throw new ArgumentException($"Account with address {address} not found.");
            }

            ActiveAccount = Accounts[address];
        }

        public async Task<string> GetAddress()
        {
            return await ActiveAccount.GetAddress();
        }

        public async Task<string> EthSign(string message)
        {
            return await ActiveAccount.EthSign(message);
        }

        public async Task<string> PersonalSign(string message)
        {
            return await ActiveAccount.PersonalSign(message);
        }

        public async Task<string> SignTypedDataV4(string json)
        {
            return await ActiveAccount.SignTypedDataV4(json);
        }

        public async Task<string> SignTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData)
            where TDomain : IDomain
        {
            return await ActiveAccount.SignTypedDataV4(data, typedData);
        }

        public async Task<string> SignTransaction(TransactionInput transaction, BigInteger chainId)
        {
            return await ActiveAccount.SignTransaction(transaction, chainId);
        }

        public async Task<bool> IsConnected()
        {
            return await ActiveAccount.IsConnected();
        }

        public async Task Disconnect()
        {
            foreach (var account in Accounts.Values)
            {
                await account.Disconnect();
            }

            ActiveAccount = null;
        }
    }
}
