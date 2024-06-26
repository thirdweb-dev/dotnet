using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;

namespace Thirdweb.Tests;

public class SmartWalletTests : BaseTests
{
    public SmartWalletTests(ITestOutputHelper output)
        : base(output) { }

    private async Task<SmartWallet> GetSmartAccount()
    {
        var client = ThirdwebClient.Create(secretKey: _secretKey);
        var privateKeyAccount = await PrivateKeyWallet.Generate(client);
        var smartAccount = await SmartWallet.Create(client, personalWallet: privateKeyAccount, factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052", gasless: true, chainId: 421614);
        return smartAccount;
    }

    [Fact]
    public async Task Initialization_Success()
    {
        var account = await GetSmartAccount();
        Assert.NotNull(await account.GetAddress());
    }

    [Fact]
    public async Task Initialization_WithoutFactory_Success()
    {
        var client = ThirdwebClient.Create(secretKey: _secretKey);
        var privateKeyAccount = await PrivateKeyWallet.Generate(client);
        var smartAccount = await SmartWallet.Create(client, personalWallet: privateKeyAccount, chainId: 421614);
        Assert.NotNull(await smartAccount.GetAddress());
    }

    [Fact]
    public async Task Initialization_Fail()
    {
        var client = ThirdwebClient.Create(secretKey: _secretKey);
        var privateKeyAccount = await PrivateKeyWallet.Generate(client);
        await privateKeyAccount.Disconnect();
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await SmartWallet.Create(client, personalWallet: privateKeyAccount, factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052", gasless: true, chainId: 421614)
        );
        Assert.Equal("SmartAccount.Connect: Personal account must be connected.", ex.Message);
    }

    [Fact]
    public async Task ForceDeploy_Success()
    {
        var client = ThirdwebClient.Create(secretKey: _secretKey);
        var privateKeyAccount = await PrivateKeyWallet.Generate(client);
        var smartAccount = await SmartWallet.Create(client, personalWallet: privateKeyAccount, factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052", gasless: true, chainId: 421614);
        await smartAccount.ForceDeploy();
        Assert.True(await smartAccount.IsDeployed());
    }

    [Fact]
    public async Task IsDeployed_True()
    {
        var account = await GetSmartAccount();
        await account.ForceDeploy();
        Assert.True(await account.IsDeployed());
    }

    [Fact]
    public async Task IsDeployed_False()
    {
        var client = ThirdwebClient.Create(secretKey: _secretKey);
        var privateKeyAccount = await PrivateKeyWallet.Generate(client);
        var smartAccount = await SmartWallet.Create(
            client,
            personalWallet: privateKeyAccount,
            factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052",
            gasless: true,
            chainId: 421614,
            accountAddressOverride: "0x75A4e181286F5767c38dFBE65fe1Ad4793aCB642" // vanity
        );
        Assert.False(await smartAccount.IsDeployed());
    }

    [Fact]
    public async Task SendTransaction_Success()
    {
        var account = await GetSmartAccount();
        var tx = await account.SendTransaction(
            new ThirdwebTransactionInput()
            {
                From = await account.GetAddress(),
                To = await account.GetAddress(),
                Value = new HexBigInteger(BigInteger.Parse("0")),
            }
        );
        Assert.NotNull(tx);
    }

    [Fact]
    public async Task SendTransaction_ClientBundleId_Success()
    {
        var client = ThirdwebClient.Create(clientId: _clientIdBundleIdOnly, bundleId: _bundleIdBundleIdOnly);
        var privateKeyAccount = await PrivateKeyWallet.Generate(client);
        var smartAccount = await SmartWallet.Create(client, personalWallet: privateKeyAccount, factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052", gasless: true, chainId: 421614);
        var tx = await smartAccount.SendTransaction(
            new ThirdwebTransactionInput()
            {
                From = await smartAccount.GetAddress(),
                To = await smartAccount.GetAddress(),
                Value = new HexBigInteger(BigInteger.Parse("0")),
            }
        );
        Assert.NotNull(tx);
    }

    [Fact]
    public async Task SendTransaction_Fail()
    {
        var account = await GetSmartAccount();
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await account.SendTransaction(null));
        Assert.Equal("SmartAccount.SendTransaction: Transaction input is required.", ex.Message);
    }

    [Fact]
    public async Task GetAddress()
    {
        var account = await GetSmartAccount();
        var address = await account.GetAddress();
        Assert.NotNull(address);
    }

    [Fact]
    public async Task GetPersonalAccount()
    {
        var account = await GetSmartAccount();
        var personalAccount = await account.GetPersonalAccount();
        Assert.NotNull(personalAccount);
        _ = Assert.IsType<PrivateKeyWallet>(personalAccount);
    }

    [Fact]
    public async Task GetAddress_WithOverride()
    {
        var client = ThirdwebClient.Create(secretKey: _secretKey);
        var privateKeyAccount = await PrivateKeyWallet.Generate(client);
        var smartAccount = await SmartWallet.Create(
            client,
            personalWallet: privateKeyAccount,
            factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052",
            gasless: true,
            chainId: 421614,
            accountAddressOverride: "0x75A4e181286F5767c38dFBE65fe1Ad4793aCB642" // vanity
        );
        var address = await smartAccount.GetAddress();
        Assert.Equal("0x75A4e181286F5767c38dFBE65fe1Ad4793aCB642", address);
    }

    [Fact]
    public async Task PersonalSign() // This is the only different signing mechanism for smart wallets, also tests isValidSignature
    {
        var account = await GetSmartAccount();
        var sig = await account.PersonalSign("Hello, world!");
        Assert.NotNull(sig);
    }

    [Fact]
    public async Task IsValidSiganture_Invalid()
    {
        var account = await GetSmartAccount();
        var sig = await account.PersonalSign("Hello, world!");
        Assert.NotNull(sig);
        sig += "1";
        var res = await account.IsValidSignature("Hello, world!", sig);
        Assert.False(res);
    }

    [Fact]
    public async Task CreateSessionKey()
    {
        var account = await GetSmartAccount();
        var receipt = await account.CreateSessionKey(
            signerAddress: "0x253d077C45A3868d0527384e0B34e1e3088A3908",
            approvedTargets: new List<string>() { Constants.ADDRESS_ZERO },
            nativeTokenLimitPerTransactionInWei: "0",
            permissionStartTimestamp: "0",
            permissionEndTimestamp: (Utils.GetUnixTimeStampNow() + 86400).ToString(),
            reqValidityStartTimestamp: "0",
            reqValidityEndTimestamp: Utils.GetUnixTimeStampIn10Years().ToString()
        );
        Assert.NotNull(receipt);
        Assert.NotNull(receipt.TransactionHash);
    }

    [Fact]
    public async Task AddAdmin()
    {
        var account = await GetSmartAccount();
        var receipt = await account.AddAdmin("0x039d7D195f6f8537003fFC19e86cd91De5e9C431");
        Assert.NotNull(receipt);
        Assert.NotNull(receipt.TransactionHash);
    }

    [Fact]
    public async Task RemoveAdmin()
    {
        var account = await GetSmartAccount();
        var receipt = await account.RemoveAdmin("0x039d7D195f6f8537003fFC19e86cd91De5e9C431");
        Assert.NotNull(receipt);
        Assert.NotNull(receipt.TransactionHash);
    }

    [Fact]
    public async Task IsConnected()
    {
        var account = await GetSmartAccount();
        Assert.True(await account.IsConnected());

        await account.Disconnect();
        Assert.False(await account.IsConnected());
    }

    [Fact]
    public async Task Disconnect()
    {
        var account = await GetSmartAccount();
        await account.Disconnect();
        Assert.False(await account.IsConnected());
    }
}
