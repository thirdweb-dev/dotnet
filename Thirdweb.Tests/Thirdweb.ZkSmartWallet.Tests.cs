namespace Thirdweb.Tests;

public class ZkSmartWalletTests : BaseTests
{
    private readonly ThirdwebClient _zkClient;

    public ZkSmartWalletTests(ITestOutputHelper output)
        : base(output)
    {
        _zkClient = ThirdwebClient.Create(secretKey: _secretKey);
    }

    private async Task<SmartWallet> GetSmartAccount(int zkChainId = 300, bool gasless = true)
    {
        var privateKeyAccount = await PrivateKeyWallet.Create(_zkClient, _testPrivateKey);
        var smartAccount = await SmartWallet.Create(_zkClient, personalWallet: privateKeyAccount, gasless: gasless, chainId: zkChainId);
        return smartAccount;
    }

    [Fact]
    public async Task GetAddress_Success()
    {
        var account = await GetSmartAccount();
        Assert.NotNull(await account.GetAddress());
    }

    [Fact]
    public async Task IsDeployed_ReturnsTrue()
    {
        var account = await GetSmartAccount();
        Assert.True(await account.IsDeployed());
    }

    [Fact]
    public async Task SendGaslessZkTx_Success()
    {
        var account = await GetSmartAccount();
        var hash = await account.SendTransaction(
            new ThirdwebTransactionInput()
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

    [Fact]
    public async Task SendGaslessZkTx_ZkCandy_Success()
    {
        var account = await GetSmartAccount(zkChainId: 302);
        var hash = await account.SendTransaction(
            new ThirdwebTransactionInput()
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
}
