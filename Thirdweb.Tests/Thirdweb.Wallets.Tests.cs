using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;

namespace Thirdweb.Tests;

public class WalletTests : BaseTests
{
    public WalletTests(ITestOutputHelper output)
        : base(output) { }

    private async Task<ThirdwebWallet> GetWallet()
    {
        var client = new ThirdwebClient(secretKey: _secretKey);
        var privateKeyAccount = new PrivateKeyAccount(client, _testPrivateKey);
        var smartAccount = new SmartAccount(client, personalAccount: privateKeyAccount, factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052", gasless: true, chainId: 421614);
        await privateKeyAccount.Connect();
        await smartAccount.Connect();
        var wallet = new ThirdwebWallet();
        await wallet.Initialize(new List<IThirdwebAccount> { privateKeyAccount, smartAccount });
        wallet.SetActive(await smartAccount.GetAddress());
        return wallet;
    }

    [Fact]
    public async Task Initialization_Success()
    {
        var wallet = await GetWallet();
        Assert.NotNull(wallet.ActiveAccount);
        Assert.Equal(2, wallet.Accounts.Count);
    }

    [Fact]
    public async Task Initialization_NoAccounts()
    {
        var wallet = new ThirdwebWallet();
        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await wallet.Initialize(new List<IThirdwebAccount>()));
        Assert.Equal("At least one account must be provided.", ex.Message);
    }

    [Fact]
    public async Task Initialization_OneDisconnectedAccount()
    {
        var client = new ThirdwebClient(secretKey: _secretKey);
        var privateKeyAccount = new PrivateKeyAccount(client, _testPrivateKey);
        var smartAccount = new SmartAccount(client, personalAccount: privateKeyAccount, factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052", gasless: true, chainId: 421614);
        await privateKeyAccount.Connect();
        var wallet = new ThirdwebWallet();
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await wallet.Initialize(new List<IThirdwebAccount> { privateKeyAccount, smartAccount }));
        Assert.Equal("Account at index 1 is not connected.", ex.Message);
    }

    [Fact]
    public async Task SetActive_Success()
    {
        var wallet = await GetWallet();
        wallet.SetActive(wallet.Accounts.Keys.Last());
        Assert.Equal(wallet.Accounts.Keys.Last(), await wallet.GetAddress());
    }

    [Fact]
    public async Task SetActive_NotFound()
    {
        var wallet = await GetWallet();
        var ex = Assert.Throws<ArgumentException>(() => wallet.SetActive(Constants.ADDRESS_ZERO));
        Assert.Equal($"Account with address {Constants.ADDRESS_ZERO} not found.", ex.Message);
    }

    [Fact]
    public async Task GetAddress()
    {
        var wallet = await GetWallet();
        Assert.Equal(await wallet.ActiveAccount.GetAddress(), await wallet.GetAddress());
    }

    [Fact]
    public async Task EthSign()
    {
        var wallet = await GetWallet();
        var message = "Hello, world!";
        var signature = await wallet.EthSign(message);
        Assert.NotNull(signature);
    }

    [Fact]
    public async Task PersonalSign()
    {
        var wallet = await GetWallet();
        var message = "Hello, world!";
        var signature = await wallet.PersonalSign(message);
        Assert.NotNull(signature);
    }

    [Fact]
    public async Task SignTypedDataV4()
    {
        var wallet = await GetWallet();
        var json =
            "{\"types\":{\"EIP712Domain\":[{\"name\":\"name\",\"type\":\"string\"},{\"name\":\"version\",\"type\":\"string\"},{\"name\":\"chainId\",\"type\":\"uint256\"},{\"name\":\"verifyingContract\",\"type\":\"address\"}],\"Person\":[{\"name\":\"name\",\"type\":\"string\"},{\"name\":\"wallet\",\"type\":\"address\"}],\"Mail\":[{\"name\":\"from\",\"type\":\"Person\"},{\"name\":\"to\",\"type\":\"Person\"},{\"name\":\"contents\",\"type\":\"string\"}]},\"primaryType\":\"Mail\",\"domain\":{\"name\":\"Ether Mail\",\"version\":\"1\",\"chainId\":1,\"verifyingContract\":\"0xCcCCccccCCCCcCCCCCCcCcCccCcCCCcCcccccccC\"},\"message\":{\"from\":{\"name\":\"Cow\",\"wallet\":\"0xCD2a3d9F938E13CD947Ec05AbC7FE734Df8DD826\"},\"to\":{\"name\":\"Bob\",\"wallet\":\"0xbBbBBBBbbBBBbbbBbbBbbBBbBbbBbBbBbBbbBBbB\"},\"contents\":\"Hello, Bob!\"}}";
        var signature = await wallet.SignTypedDataV4(json);
        Assert.NotNull(signature);
    }

    [Fact]
    public async Task SignTypedDataV4_Typed()
    {
        var wallet = await GetWallet();
        var typedData = EIP712.GetTypedDefinition_SmartAccount_AccountMessage("Account", "1", 421614, await wallet.GetAddress());
        var accountMessage = new AccountAbstraction.AccountMessage { Message = System.Text.Encoding.UTF8.GetBytes("Hello, world!").HashPrefixedMessage() };
        var signature = await wallet.SignTypedDataV4(accountMessage, typedData);
        Assert.NotNull(signature);

        var signerAcc = await ((SmartAccount)wallet.ActiveAccount).GetPersonalAccount();
        var gen1 = await EIP712.GenerateSignature_SmartAccount_AccountMessage(
            "Account",
            "1",
            421614,
            await wallet.GetAddress(),
            System.Text.Encoding.UTF8.GetBytes("Hello, world!").HashPrefixedMessage(),
            signerAcc
        );
        Assert.Equal(gen1, signature);

        var req = new AccountAbstraction.SignerPermissionRequest()
        {
            Signer = await wallet.GetAddress(),
            IsAdmin = 0,
            ApprovedTargets = new List<string>() { Constants.ADDRESS_ZERO },
            NativeTokenLimitPerTransaction = 0,
            PermissionStartTimestamp = 0,
            ReqValidityStartTimestamp = 0,
            PermissionEndTimestamp = 0,
            Uid = new byte[32]
        };

        var typedData2 = EIP712.GetTypedDefinition_SmartAccount("Account", "1", 421614, await wallet.GetAddress());
        var signature2 = await wallet.SignTypedDataV4(req, typedData2);
        Assert.NotNull(signature2);

        var gen2 = await EIP712.GenerateSignature_SmartAccount("Account", "1", 421614, await wallet.GetAddress(), req, signerAcc);
        Assert.Equal(gen2, signature2);
    }

    [Fact]
    public async Task SignTransaction()
    {
        var wallet = await GetWallet();
        var transaction = new TransactionInput
        {
            To = await wallet.GetAddress(),
            Data = "0x",
            Value = new HexBigInteger(0),
            Gas = new HexBigInteger(21000),
            GasPrice = new HexBigInteger(10000000000),
            Nonce = new HexBigInteger(9999999999999),
        };
        var rpc = ThirdwebRPC.GetRpcInstance(new ThirdwebClient(secretKey: _secretKey), 421614);
        var signature = await wallet.SignTransaction(transaction, 421614);
        Assert.NotNull(signature);
    }

    [Fact]
    public async Task IsConnected()
    {
        var wallet = await GetWallet();
        Assert.True(await wallet.IsConnected());
        foreach (var account in wallet.Accounts.Values)
        {
            Assert.True(await account.IsConnected());
        }

        await wallet.DisconnectAll();
        Assert.False(await wallet.IsConnected());
        foreach (var account in wallet.Accounts.Values)
        {
            Assert.False(await account.IsConnected());
        }
    }
}
