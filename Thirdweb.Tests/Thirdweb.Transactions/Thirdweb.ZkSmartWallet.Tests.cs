namespace Thirdweb.Tests.Wallets;

public class ZkSmartWalletTests : BaseTests
{
    private readonly ThirdwebClient _zkClient;

    public ZkSmartWalletTests(ITestOutputHelper output)
        : base(output)
    {
        this._zkClient = ThirdwebClient.Create(secretKey: this.SecretKey);
    }

    private async Task<SmartWallet> GetSmartAccount(int zkChainId = 300, bool gasless = true)
    {
        var privateKeyAccount = await PrivateKeyWallet.Generate(this._zkClient);
        var smartAccount = await SmartWallet.Create(personalWallet: privateKeyAccount, gasless: gasless, chainId: zkChainId);
        return smartAccount;
    }

    [Fact(Timeout = 120000)]
    public async Task GetAddress_Success()
    {
        var account = await this.GetSmartAccount();
        Assert.NotNull(await account.GetAddress());
    }

    [Fact(Timeout = 120000)]
    public async Task PersonalSign_Success()
    {
        var account = await this.GetSmartAccount(zkChainId: 302);
        var message = "Hello, World!";
        var signature = await account.PersonalSign(message);
        Assert.NotNull(signature);
        Assert.True(signature.Length > 0);
    }

    [Fact(Timeout = 120000)]
    public async Task CreateSessionKey_Throws()
    {
        var account = await this.GetSmartAccount();
        _ = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await account.CreateSessionKey(
                    signerAddress: await account.GetAddress(),
                    approvedTargets: [Constants.ADDRESS_ZERO],
                    nativeTokenLimitPerTransactionInWei: "0",
                    permissionStartTimestamp: "0",
                    permissionEndTimestamp: (Utils.GetUnixTimeStampNow() + 86400).ToString(),
                    reqValidityStartTimestamp: "0",
                    reqValidityEndTimestamp: Utils.GetUnixTimeStampIn10Years().ToString()
                )
        );
    }

    [Fact(Timeout = 120000)]
    public async Task AddAdmin_Throws()
    {
        var account = await this.GetSmartAccount();
        _ = await Assert.ThrowsAsync<InvalidOperationException>(async () => await account.AddAdmin(Constants.ADDRESS_ZERO));
    }

    [Fact(Timeout = 120000)]
    public async Task RemoveAdmin_Throws()
    {
        var account = await this.GetSmartAccount();
        _ = await Assert.ThrowsAsync<InvalidOperationException>(async () => await account.RemoveAdmin(Constants.ADDRESS_ZERO));
    }

    [Fact(Timeout = 120000)]
    public async Task IsDeployed_ReturnsTrue()
    {
        var account = await this.GetSmartAccount();
        Assert.True(await account.IsDeployed());
    }

    [Fact(Timeout = 120000)]
    public async Task SendGaslessZkTx_Success()
    {
        var account = await this.GetSmartAccount();
        var hash = await account.SendTransaction(
            new ThirdwebTransactionInput(300)
            {
                From = await account.GetAddress(),
                To = await account.GetAddress(),
                Value = new Nethereum.Hex.HexTypes.HexBigInteger(0),
                Data = "0x"
            }
        );
        Assert.NotNull(hash);
        Assert.True(hash.Length == 66);
    }

    // [Fact(Timeout = 120000)]
    // public async Task SendGaslessZkTx_ZkCandy_Success()
    // {
    //     var account = await this.GetSmartAccount(zkChainId: 302);
    //     var hash = await account.SendTransaction(
    //         new ThirdwebTransactionInput(302)
    //         {
    //             From = await account.GetAddress(),
    //             To = await account.GetAddress(),
    //             Value = new Nethereum.Hex.HexTypes.HexBigInteger(0),
    //             Data = "0x"
    //         }
    //     );
    //     Assert.NotNull(hash);
    //     Assert.True(hash.Length == 66);
    // }

    [Fact(Timeout = 120000)]
    public async Task SendGaslessZkTx_Abstract_Success()
    {
        var account = await this.GetSmartAccount(zkChainId: 11124);
        var hash = await account.SendTransaction(
            new ThirdwebTransactionInput(11124)
            {
                From = await account.GetAddress(),
                To = await account.GetAddress(),
                Value = new Nethereum.Hex.HexTypes.HexBigInteger(0),
                Data = "0x"
            }
        );
        Assert.NotNull(hash);
        Assert.True(hash.Length == 66);
    }

    [Fact(Timeout = 120000)]
    public async Task ZkSync_Switch()
    {
        var account = await this.GetSmartAccount(zkChainId: 300);
        _ = await account.SendTransaction(
            new ThirdwebTransactionInput(302)
            {
                From = await account.GetAddress(),
                To = await account.GetAddress(),
                Value = new Nethereum.Hex.HexTypes.HexBigInteger(0),
                Data = "0x"
            }
        );
    }
}
