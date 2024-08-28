namespace Thirdweb.Tests.Wallets;

public class SmartWalletTests : BaseTests
{
    public SmartWalletTests(ITestOutputHelper output)
        : base(output) { }

    private async Task<SmartWallet> GetSmartAccount()
    {
        var privateKeyAccount = await PrivateKeyWallet.Generate(this.Client);
        var smartAccount = await SmartWallet.Create(personalWallet: privateKeyAccount, gasless: true, chainId: 421614);
        return smartAccount;
    }

    [Fact(Timeout = 120000)]
    public async Task Initialization_Success()
    {
        var account = await this.GetSmartAccount();
        Assert.NotNull(await account.GetAddress());
    }

    [Fact(Timeout = 120000)]
    public async Task Initialization_WithoutFactory_Success()
    {
        var client = this.Client;
        var privateKeyAccount = await PrivateKeyWallet.Generate(client);
        var smartAccount = await SmartWallet.Create(personalWallet: privateKeyAccount, chainId: 421614);
        Assert.NotNull(await smartAccount.GetAddress());
    }

    [Fact(Timeout = 120000)]
    public async Task Initialization_Fail()
    {
        var client = this.Client;
        var privateKeyAccount = await PrivateKeyWallet.Generate(client);
        await privateKeyAccount.Disconnect();
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await SmartWallet.Create(personalWallet: privateKeyAccount, factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052", gasless: true, chainId: 421614)
        );
        Assert.Equal("SmartAccount.Connect: Personal account must be connected.", ex.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task ForceDeploy_Success()
    {
        var client = this.Client;
        var privateKeyAccount = await PrivateKeyWallet.Generate(client);
        var smartAccount = await SmartWallet.Create(personalWallet: privateKeyAccount, factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052", gasless: true, chainId: 421614);
        await smartAccount.ForceDeploy();
        Assert.True(await smartAccount.IsDeployed());
    }

    [Fact(Timeout = 120000)]
    public async Task IsDeployed_True()
    {
        var account = await this.GetSmartAccount();
        await account.ForceDeploy();
        Assert.True(await account.IsDeployed());
    }

    [Fact(Timeout = 120000)]
    public async Task IsDeployed_False()
    {
        var client = this.Client;
        var privateKeyAccount = await PrivateKeyWallet.Generate(client);
        var smartAccount = await SmartWallet.Create(
            personalWallet: privateKeyAccount,
            factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052",
            gasless: true,
            chainId: 421614,
            accountAddressOverride: "0x75A4e181286F5767c38dFBE65fe1Ad4793aCB642" // vanity
        );
        Assert.False(await smartAccount.IsDeployed());
    }

    [Fact(Timeout = 120000)]
    public async Task ExecuteTransaction_Success()
    {
        var account = await this.GetSmartAccount();
        var tx = await account.ExecuteTransaction(new ThirdwebTransactionInput(421614) { To = await account.GetAddress() });
        Assert.NotNull(tx);
    }

    [Fact(Timeout = 120000)]
    public async Task SendTransaction_Success()
    {
        var account = await this.GetSmartAccount();
        var tx = await account.SendTransaction(new ThirdwebTransactionInput(421614) { To = await account.GetAddress(), });
        Assert.NotNull(tx);
    }

    [Fact(Timeout = 120000)]
    public async Task SendTransaction_ClientBundleId_Success()
    {
        var client = ThirdwebClient.Create(clientId: this.ClientIdBundleIdOnly, bundleId: this.BundleIdBundleIdOnly);
        var privateKeyAccount = await PrivateKeyWallet.Generate(client);
        var smartAccount = await SmartWallet.Create(personalWallet: privateKeyAccount, factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052", gasless: true, chainId: 421614);
        var tx = await smartAccount.SendTransaction(new ThirdwebTransactionInput(421614) { To = await smartAccount.GetAddress(), });
        Assert.NotNull(tx);
    }

    [Fact(Timeout = 120000)]
    public async Task SendTransaction_Fail()
    {
        var account = await this.GetSmartAccount();
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await account.SendTransaction(null));
        Assert.Equal("SmartAccount.SendTransaction: Transaction input is required.", ex.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task GetAddress()
    {
        var account = await this.GetSmartAccount();
        var address = await account.GetAddress();
        Assert.NotNull(address);
    }

    [Fact(Timeout = 120000)]
    public async Task GetPersonalAccount()
    {
        var account = await this.GetSmartAccount();
        var personalAccount = await account.GetPersonalWallet();
        Assert.NotNull(personalAccount);
        _ = Assert.IsType<PrivateKeyWallet>(personalAccount);
    }

    [Fact(Timeout = 120000)]
    public async Task GetAddress_WithOverride()
    {
        var client = this.Client;
        var privateKeyAccount = await PrivateKeyWallet.Generate(client);
        var smartAccount = await SmartWallet.Create(
            personalWallet: privateKeyAccount,
            factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052",
            gasless: true,
            chainId: 421614,
            accountAddressOverride: "0x75A4e181286F5767c38dFBE65fe1Ad4793aCB642" // vanity
        );
        var address = await smartAccount.GetAddress();
        Assert.Equal("0x75A4e181286F5767c38dFBE65fe1Ad4793aCB642", address);
    }

    [Fact(Timeout = 120000)]
    public async Task PersonalSign() // This is the only different signing mechanism for smart wallets, also tests isValidSignature
    {
        var account = await this.GetSmartAccount();
        var sig = await account.PersonalSign("Hello, world!");
        Assert.NotNull(sig);
    }

    [Fact(Timeout = 120000)]
    public async Task IsValidSiganture_Invalid()
    {
        var account = await this.GetSmartAccount();
        var sig = await account.PersonalSign("Hello, world!");
        Assert.NotNull(sig);
        sig += "1";
        var res = await account.IsValidSignature("Hello, world!", sig);
        Assert.False(res);
    }

    [Fact(Timeout = 120000)]
    public async Task CreateSessionKey()
    {
        var account = await this.GetSmartAccount();
        var receipt = await account.CreateSessionKey(
            signerAddress: "0x253d077C45A3868d0527384e0B34e1e3088A3908",
            approvedTargets: new List<string> { Constants.ADDRESS_ZERO },
            nativeTokenLimitPerTransactionInWei: "0",
            permissionStartTimestamp: "0",
            permissionEndTimestamp: (Utils.GetUnixTimeStampNow() + 86400).ToString(),
            reqValidityStartTimestamp: "0",
            reqValidityEndTimestamp: Utils.GetUnixTimeStampIn10Years().ToString()
        );
        Assert.NotNull(receipt);
        Assert.NotNull(receipt.TransactionHash);
    }

    [Fact(Timeout = 120000)]
    public async Task AddAdmin()
    {
        var account = await this.GetSmartAccount();
        var receipt = await account.AddAdmin("0x039d7D195f6f8537003fFC19e86cd91De5e9C431");
        Assert.NotNull(receipt);
        Assert.NotNull(receipt.TransactionHash);
    }

    [Fact(Timeout = 120000)]
    public async Task RemoveAdmin()
    {
        var account = await this.GetSmartAccount();
        var receipt = await account.RemoveAdmin("0x039d7D195f6f8537003fFC19e86cd91De5e9C431");
        Assert.NotNull(receipt);
        Assert.NotNull(receipt.TransactionHash);
    }

    [Fact(Timeout = 120000)]
    public async Task IsConnected()
    {
        var account = await this.GetSmartAccount();
        Assert.True(await account.IsConnected());

        await account.Disconnect();
        Assert.False(await account.IsConnected());
    }

    [Fact(Timeout = 120000)]
    public async Task Disconnect()
    {
        var account = await this.GetSmartAccount();
        await account.Disconnect();
        Assert.False(await account.IsConnected());
    }

    [Fact(Timeout = 120000)]
    public async Task GetAllActiveSigners()
    {
        var account = await this.GetSmartAccount();
        var signers = await account.GetAllActiveSigners();
        Assert.NotNull(signers);
        var count = signers.Count;

        // add signer
        var randomSigner = await (await PrivateKeyWallet.Generate(this.Client)).GetAddress();
        _ = await account.CreateSessionKey(
            signerAddress: randomSigner,
            approvedTargets: new List<string>() { Constants.ADDRESS_ZERO },
            nativeTokenLimitPerTransactionInWei: "0",
            permissionStartTimestamp: "0",
            permissionEndTimestamp: (Utils.GetUnixTimeStampNow() + 86400).ToString(),
            reqValidityStartTimestamp: "0",
            reqValidityEndTimestamp: Utils.GetUnixTimeStampIn10Years().ToString()
        );

        signers = await account.GetAllActiveSigners();

        Assert.Equal(count + 1, signers.Count);

        // remove signer
        _ = await account.RevokeSessionKey(signerAddress: randomSigner);

        signers = await account.GetAllActiveSigners();

        Assert.Equal(count, signers.Count);
    }

    [Fact(Timeout = 120000)]
    public async Task GetAllAdmins()
    {
        var account = await this.GetSmartAccount();
        await account.ForceDeploy();
        var admins = await account.GetAllAdmins();
        Assert.NotNull(admins);
        var count = admins.Count;

        // add admin
        var randomAdmin = await (await PrivateKeyWallet.Generate(this.Client)).GetAddress();
        _ = await account.AddAdmin(randomAdmin);

        admins = await account.GetAllAdmins();

        Assert.Equal(count + 1, admins.Count);

        // remove admin
        _ = await account.RemoveAdmin(randomAdmin);

        admins = await account.GetAllAdmins();

        Assert.Equal(count, admins.Count);
    }

    [Fact(Timeout = 120000)]
    public async Task SendTransaction_07_Success()
    {
        var smartWallet07 = await SmartWallet.Create(
            personalWallet: await PrivateKeyWallet.Generate(this.Client),
            chainId: 11155111,
            gasless: true,
            factoryAddress: "0xc5A43D081Dc10316EE640504Ea1cBc74666F3874",
            entryPoint: Constants.ENTRYPOINT_ADDRESS_V07
        );

        var hash07 = await smartWallet07.SendTransaction(new ThirdwebTransactionInput(11155111) { To = await smartWallet07.GetAddress(), });

        Assert.NotNull(hash07);
        Assert.True(hash07.Length == 66);
    }

    [Fact(Timeout = 120000)]
    public async Task MultiChainTransaction_Success()
    {
        var chainId1 = 11155111;
        var chainId2 = 421614;

        var smartWallet = await SmartWallet.Create(
            personalWallet: await PrivateKeyWallet.Generate(this.Client),
            chainId: chainId1,
            gasless: true,
            factoryAddress: Constants.DEFAULT_FACTORY_ADDRESS_V06,
            entryPoint: Constants.ENTRYPOINT_ADDRESS_V06
        );

        var address1 = await smartWallet.GetAddress();
        var receipt1 = await smartWallet.ExecuteTransaction(new ThirdwebTransactionInput(chainId1) { To = address1, });
        var nonce1 = await smartWallet.GetTransactionCount(chainId: chainId1, blocktag: "latest");

        var address2 = await smartWallet.GetAddress();
        var receipt2 = await smartWallet.ExecuteTransaction(new ThirdwebTransactionInput(chainId2) { To = address2, });
        var nonce2 = await smartWallet.GetTransactionCount(chainId: chainId2, blocktag: "latest");

        Assert.NotNull(address1);
        Assert.NotNull(address2);
        Assert.Equal(address1, address2);

        Assert.NotNull(receipt1);
        Assert.NotNull(receipt2);

        Assert.True(receipt1.TransactionHash.Length == 66);
        Assert.True(receipt2.TransactionHash.Length == 66);

        Assert.Equal(receipt1.To, receipt2.To);

        Assert.Equal(nonce1, 1);
        Assert.Equal(nonce2, 1);
    }
}
