using System.Numerics;
using Nethereum.ABI.EIP712;
using Nethereum.Util;
using Thirdweb.AccountAbstraction;

namespace Thirdweb;

public enum ExecutionMode
{
    EOA,
    EIP7702,
}

/// <summary>
/// Represents a 7702 delegated wallet with granular session key permissions and automatic session key execution.
/// </summary>
public class SmarterWallet : IThirdwebWallet
{
    public string WalletId => "smarter";

    public ThirdwebClient Client { get; }
    public ThirdwebAccountType AccountType => ThirdwebAccountType.ExternalAccount;

    internal IThirdwebWallet UserWallet { get; }
    internal ThirdwebContract UserContract { get; }
    internal BigInteger ChainId { get; }
    internal ExecutionMode ExecutionMode { get; }

    private EIP7702Authorization? Authorization { get; set; }

    internal SmarterWallet(ThirdwebClient client, BigInteger chainId, IThirdwebWallet userWallet, ThirdwebContract userContract, EIP7702Authorization? authorization, ExecutionMode executionMode)
    {
        this.Client = client;
        this.ChainId = chainId;
        this.UserWallet = userWallet;
        this.UserContract = userContract;
        this.Authorization = authorization;
        this.ExecutionMode = executionMode;
    }

    public static async Task<SmarterWallet> Create(ThirdwebClient client, BigInteger chainId, IThirdwebWallet userWallet, ExecutionMode executionMode)
    {
        var userWalletAddress = await userWallet.GetAddress();
        var userContract = await ThirdwebContract.Create(client, userWalletAddress, chainId, Constants.MINIMAL_ACCOUNT_7702_ABI);
        var needsDelegation = !await Utils.IsDelegatedAccount(client, chainId, userWalletAddress);
        EIP7702Authorization? authorization = needsDelegation ? await userWallet.SignAuthorization(chainId, Constants.MINIMAL_ACCOUNT_7702, willSelfExecute: executionMode == ExecutionMode.EOA) : null;
        var wallet = new SmarterWallet(client, chainId, userWallet, userContract, authorization, executionMode);
        Utils.TrackConnection(wallet);
        return wallet;
    }

    #region Wallet Specific

    public async Task<ThirdwebTransactionReceipt> CreateSessionKey(SessionSpec sessionKeyParams)
    {
        var userWalletAddress = await this.UserWallet.GetAddress();
        var sessionKeySig = await EIP712.GenerateSignature_SmartAccount_7702("MinimalAccount", "1", this.ChainId, userWalletAddress, sessionKeyParams, this.UserWallet);
        var sessionKeyCallData = this.UserContract.CreateCallData("createSessionWithSig", sessionKeyParams, sessionKeySig.HexToBytes());
        var sessionKeyTx = await ThirdwebTransaction.Create(this, new ThirdwebTransactionInput(chainId: this.ChainId, to: userWalletAddress, data: sessionKeyCallData));
        return await ThirdwebTransaction.SendAndWaitForTransactionReceipt(sessionKeyTx);
    }

    #endregion

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
        var userWalletAddress = await this.UserWallet.GetAddress();

        if (this.Authorization != null && await Utils.IsDelegatedAccount(this.Client, this.ChainId, userWalletAddress))
        {
            this.Authorization = null;
        }

        var calls = new List<Call>
        {
            new()
            {
                Target = transaction.To,
                Value = transaction.Value?.Value ?? BigInteger.Zero,
                Data = transaction.Data.HexToBytes()
            }
        };

        switch (this.ExecutionMode)
        {
            case ExecutionMode.EIP7702:
                var wrappedCalls = new WrappedCalls() { Calls = calls, Uid = Guid.NewGuid().ToByteArray().PadTo32Bytes() };
                var signature = await EIP712.GenerateSignature_SmartAccount_7702_WrappedCalls("MinimalAccount", "1", this.ChainId, userWalletAddress, wrappedCalls, this.UserWallet);
                var response = await BundlerClient.TwExecute(
                    client: this.Client,
                    // url: $"{this.ChainId}.bundler.thirdweb.com",
                    url: "http://localhost:8787?chain=11155111",
                    requestId: 7702,
                    eoaAddress: userWalletAddress,
                    wrappedCalls: wrappedCalls,
                    signature: signature,
                    authorization: this.Authorization != null && !await Utils.IsDelegatedAccount(this.Client, this.ChainId, userWalletAddress) ? this.Authorization : null
                );
                var queueId = response?.QueueId;
                string txHash = null;
                var ct = new CancellationTokenSource(this.Client.FetchTimeoutOptions.GetTimeout(TimeoutType.Other));
                try
                {
                    while (txHash == null)
                    {
                        ct.Token.ThrowIfCancellationRequested();

                        var hashResponse = await BundlerClient
                            .TwGetTransactionHash(
                                client: this.Client,
                                // url: $"{this.ChainId}.bundler.thirdweb.com",
                                url: "http://localhost:8787?chain=11155111",
                                requestId: 7702,
                                queueId
                            )
                            .ConfigureAwait(false);

                        txHash = hashResponse?.TransactionHash;
                        await ThirdwebTask.Delay(100, ct.Token).ConfigureAwait(false);
                    }
                    return txHash;
                }
                catch (OperationCanceledException)
                {
                    throw new Exception($"EIP-7702 sponsored transaction timed out with queue id: {queueId}");
                }
            case ExecutionMode.EOA:
                // Add up values of all calls
                BigInteger totalValue = 0;
                foreach (var call in calls)
                {
                    totalValue += call.Value;
                }
                // Prepare a tx using the user wallet as the executor
                var finalTx = await this.UserContract.Prepare(wallet: this.UserWallet, method: "execute", weiValue: totalValue, parameters: new object[] { calls });
                finalTx.Input.AuthorizationList = this.Authorization != null ? new List<EIP7702Authorization>() { this.Authorization.Value } : null;

                // Append authorization if not delegated yet

                // Send the transaction and return the
                return await ThirdwebTransaction.Send(finalTx);
            default:
                throw new NotImplementedException($"Execution mode {this.ExecutionMode} is not supported.");
        }
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
