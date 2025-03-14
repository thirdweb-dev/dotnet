using System.Numerics;
using Nethereum.ABI.EIP712;
using Thirdweb.AccountAbstraction;

namespace Thirdweb;

/// <summary>
/// Represents a 7702 delegated wallet with granular session key permissions and automatic session key execution.
/// </summary>
public class ThirdwebWallet : IThirdwebWallet
{
    public string WalletId => "thirdweb";

    public ThirdwebClient Client { get; }
    public ThirdwebAccountType AccountType => ThirdwebAccountType.ExternalAccount;

    internal IThirdwebWallet UserWallet { get; }
    internal IThirdwebWallet ExecutorWallet { get; }
    internal ThirdwebContract UserContract { get; }

    internal ThirdwebWallet(ThirdwebClient client, IThirdwebWallet userWallet, IThirdwebWallet executorWallet, ThirdwebContract userContract)
    {
        this.Client = client;
        this.UserWallet = userWallet;
        this.ExecutorWallet = executorWallet;
        this.UserContract = userContract;
    }

    public static async Task<ThirdwebWallet> Create(ThirdwebClient client, BigInteger chainId, IThirdwebWallet userWallet, IThirdwebWallet executorWallet, SessionSpec sessionKeyParams)
    {
        var userWalletAddress = await userWallet.GetAddress();
        var executorWalletAddress = await executorWallet.GetAddress();
        if (sessionKeyParams != null && sessionKeyParams.Signer != executorWalletAddress)
        {
            throw new Exception("Session key signer must be the executor wallet");
        }
        var delegationContract = await ThirdwebContract.Create(client, Constants.MINIMAL_ACCOUNT_7702, chainId);

        var rpc = ThirdwebRPC.GetRpcInstance(client, chainId);
        var code = await rpc.SendRequestAsync<string>("eth_getCode", userWalletAddress, "latest");
        var needsDelegation = !Utils.IsDelegatedAccount(code);

        // Sign authorization if needed
        EIP7702Authorization? authorization = needsDelegation ? await userWallet.SignAuthorization(chainId, Constants.MINIMAL_ACCOUNT_7702, willSelfExecute: false) : null;

        // TODO: We don't always need to create a session key when creating this wallet, handle with a flag or null check

        // Sign message for session key
        var sessionKeySig = await EIP712.GenerateSignature_SmartAccount_7702("MinimalAccount", "1", chainId, userWalletAddress, sessionKeyParams, userWallet);

        // Create call data for the session
        var sessionKeyCallData = delegationContract.CreateCallData("createSessionWithSig", sessionKeyParams, sessionKeySig.HexToBytes());

        // Execute the delegation & session creation in one go
        var delegationTx = await ThirdwebTransaction.Create(
            executorWallet,
            new ThirdwebTransactionInput(chainId: chainId, to: userWalletAddress, data: sessionKeyCallData, authorization: authorization)
        );
        _ = await ThirdwebTransaction.SendAndWaitForTransactionReceipt(delegationTx);

        var newCode = await rpc.SendRequestAsync<string>("eth_getCode", userWalletAddress, "latest");
        if (!Utils.IsDelegatedAccount(newCode))
        {
            throw new Exception("Delegation failed, code was not set.");
        }

        var userContract = await ThirdwebContract.Create(client, userWalletAddress, chainId, delegationContract.Abi);
        var wallet = new ThirdwebWallet(client, userWallet, executorWallet, userContract);
        Utils.TrackConnection(wallet);
        return wallet;
    }

    #region IThirdwebWallet

    public Task<string> GetAddress()
    {
        return this.UserWallet.GetAddress();
    }

    public Task<string> EthSign(byte[] rawMessage)
    {
        return this.UserWallet.EthSign(rawMessage);
    }

    public Task<string> EthSign(string message)
    {
        return this.UserWallet.EthSign(message);
    }

    public Task<string> RecoverAddressFromEthSign(string message, string signature)
    {
        return this.UserWallet.RecoverAddressFromEthSign(message, signature);
    }

    public Task<string> PersonalSign(byte[] rawMessage)
    {
        return this.UserWallet.PersonalSign(rawMessage);
    }

    public Task<string> PersonalSign(string message)
    {
        return this.UserWallet.PersonalSign(message);
    }

    public Task<string> RecoverAddressFromPersonalSign(string message, string signature)
    {
        return this.UserWallet.RecoverAddressFromPersonalSign(message, signature);
    }

    public Task<string> SignTypedDataV4(string json)
    {
        return this.UserWallet.SignTypedDataV4(json);
    }

    public Task<string> SignTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData)
        where TDomain : IDomain
    {
        return this.UserWallet.SignTypedDataV4(data, typedData);
    }

    public Task<string> RecoverAddressFromTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData, string signature)
        where TDomain : IDomain
    {
        return this.UserWallet.RecoverAddressFromTypedDataV4(data, typedData, signature);
    }

    public Task<bool> IsConnected()
    {
        return this.UserWallet.IsConnected();
    }

    public Task<string> SignTransaction(ThirdwebTransactionInput transaction)
    {
        return this.UserWallet.SignTransaction(transaction);
    }

    public async Task<string> SendTransaction(ThirdwebTransactionInput transaction)
    {
        var calls = new List<Call>
        {
            new()
            {
                Target = transaction.To,
                Value = transaction.Value?.Value ?? BigInteger.Zero,
                Data = transaction.Data.HexToBytes()
            }
        };
        var tx = await this.UserContract.Prepare(this.ExecutorWallet, "execute", calls[0].Value, calls);
        return await ThirdwebTransaction.Send(tx);
    }

    public async Task<ThirdwebTransactionReceipt> ExecuteTransaction(ThirdwebTransactionInput transaction)
    {
        var hash = await this.SendTransaction(transaction);
        return await Utils.WaitForTransactionReceipt(this.Client, transaction.ChainId, hash);
    }

    public Task Disconnect()
    {
        return this.UserWallet.Disconnect();
    }

    public Task<List<LinkedAccount>> LinkAccount(
        IThirdwebWallet walletToLink,
        string otp = null,
        bool? isMobile = null,
        Action<string> browserOpenAction = null,
        string mobileRedirectScheme = "thirdweb://",
        IThirdwebBrowser browser = null,
        BigInteger? chainId = null,
        string jwt = null,
        string payload = null,
        string defaultSessionIdOverride = null,
        List<string> forceWalletIds = null
    )
    {
        return this.UserWallet.LinkAccount(walletToLink, otp, isMobile, browserOpenAction, mobileRedirectScheme, browser, chainId, jwt, payload, defaultSessionIdOverride, forceWalletIds);
    }

    public Task<List<LinkedAccount>> UnlinkAccount(LinkedAccount accountToUnlink)
    {
        return this.UserWallet.UnlinkAccount(accountToUnlink);
    }

    public Task<List<LinkedAccount>> GetLinkedAccounts()
    {
        return this.UserWallet.GetLinkedAccounts();
    }

    public Task<EIP7702Authorization> SignAuthorization(BigInteger chainId, string contractAddress, bool willSelfExecute)
    {
        return this.UserWallet.SignAuthorization(chainId, contractAddress, willSelfExecute);
    }

    public Task SwitchNetwork(BigInteger chainId)
    {
        return this.UserWallet.SwitchNetwork(chainId);
    }

    #endregion
}
