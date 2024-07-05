using Nethereum.Hex.HexTypes;

namespace Thirdweb.Tests.Wallets;

public class WalletTests : BaseTests
{
    private ThirdwebClient _client;

    public WalletTests(ITestOutputHelper output)
        : base(output)
    {
        _client = ThirdwebClient.Create(secretKey: _secretKey);
    }

    private async Task<SmartWallet> GetAccount()
    {
        var privateKeyAccount = await PrivateKeyWallet.Generate(_client);
        var smartAccount = await SmartWallet.Create(personalWallet: privateKeyAccount, factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052", gasless: true, chainId: 421614);
        return smartAccount;
    }

    [Fact(Timeout = 120000)]
    public async Task GetAddress()
    {
        var wallet = await GetAccount();
        Assert.Equal(await wallet.GetAddress(), await wallet.GetAddress());
    }

    [Fact(Timeout = 120000)]
    public async Task EthSignRaw()
    {
        var wallet = await GetAccount();
        var message = "Hello, world!";
        var signature = await wallet.EthSign(System.Text.Encoding.UTF8.GetBytes(message));
        Assert.NotNull(signature);
    }

    [Fact(Timeout = 120000)]
    public async Task EthSign()
    {
        var wallet = await GetAccount();
        var message = "Hello, world!";
        var signature = await wallet.EthSign(message);
        Assert.NotNull(signature);
    }

    [Fact(Timeout = 120000)]
    public async Task PersonalSignRaw()
    {
        var wallet = await GetAccount();
        var message = "Hello, world!";
        var signature = await wallet.PersonalSign(System.Text.Encoding.UTF8.GetBytes(message));
        Assert.NotNull(signature);
    }

    [Fact(Timeout = 120000)]
    public async Task PersonalSign()
    {
        var wallet = await GetAccount();
        var message = "Hello, world!";
        var signature = await wallet.PersonalSign(message);
        Assert.NotNull(signature);
    }

    [Fact(Timeout = 120000)]
    public async Task SignTypedDataV4()
    {
        var wallet = await GetAccount();
        var json =
            "{\"types\":{\"EIP712Domain\":[{\"name\":\"name\",\"type\":\"string\"},{\"name\":\"version\",\"type\":\"string\"},{\"name\":\"chainId\",\"type\":\"uint256\"},{\"name\":\"verifyingContract\",\"type\":\"address\"}],\"Person\":[{\"name\":\"name\",\"type\":\"string\"},{\"name\":\"wallet\",\"type\":\"address\"}],\"Mail\":[{\"name\":\"from\",\"type\":\"Person\"},{\"name\":\"to\",\"type\":\"Person\"},{\"name\":\"contents\",\"type\":\"string\"}]},\"primaryType\":\"Mail\",\"domain\":{\"name\":\"Ether Mail\",\"version\":\"1\",\"chainId\":1,\"verifyingContract\":\"0xCcCCccccCCCCcCCCCCCcCcCccCcCCCcCcccccccC\"},\"message\":{\"from\":{\"name\":\"Cow\",\"wallet\":\"0xCD2a3d9F938E13CD947Ec05AbC7FE734Df8DD826\"},\"to\":{\"name\":\"Bob\",\"wallet\":\"0xbBbBBBBbbBBBbbbBbbBbbBBbBbbBbBbBbBbbBBbB\"},\"contents\":\"Hello, Bob!\"}}";
        var signature = await wallet.SignTypedDataV4(json);
        Assert.NotNull(signature);
    }

    [Fact(Timeout = 120000)]
    public async Task SignTypedDataV4_Typed()
    {
        var wallet = await GetAccount();
        var typedData = EIP712.GetTypedDefinition_SmartAccount_AccountMessage("Account", "1", 421614, await wallet.GetAddress());
        var accountMessage = new AccountAbstraction.AccountMessage { Message = System.Text.Encoding.UTF8.GetBytes("Hello, world!").HashPrefixedMessage() };
        var signature = await wallet.SignTypedDataV4(accountMessage, typedData);
        Assert.NotNull(signature);

        var signerAcc = await (wallet).GetPersonalAccount();
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

        // Recover address
        var recoveredAddress = await wallet.RecoverAddressFromTypedDataV4(req, typedData2, signature2);
        Assert.Equal(await wallet.GetAddress(), recoveredAddress);

        // Recover address invalid
        var recoveredAddress2 = await wallet.RecoverAddressFromTypedDataV4(req, typedData2, signature2 + "00");
        Assert.NotEqual(await wallet.GetAddress(), recoveredAddress2);
    }

    [Fact(Timeout = 120000)]
    public async Task SignTransaction()
    {
        var wallet = await GetAccount();
        var transaction = new ThirdwebTransactionInput
        {
            To = await wallet.GetAddress(),
            Data = "0x",
            Value = new HexBigInteger(0),
            Gas = new HexBigInteger(21000),
            GasPrice = new HexBigInteger(10000000000),
            Nonce = new HexBigInteger(9999999999999),
            ChainId = new HexBigInteger(421614),
        };
        var rpc = ThirdwebRPC.GetRpcInstance(ThirdwebClient.Create(secretKey: _secretKey), 421614);
        var signature = await wallet.SignTransaction(transaction);
        Assert.NotNull(signature);
    }

    [Fact(Timeout = 120000)]
    public async Task RecoverAddressFromEthSign_ReturnsSameAddress()
    {
        var wallet = await PrivateKeyWallet.Generate(_client);
        var message = "Hello, world!";
        var signature = await wallet.EthSign(message);
        var recoveredAddress = await wallet.RecoverAddressFromEthSign(message, signature);
        Assert.Equal(await wallet.GetAddress(), recoveredAddress);
    }

    [Fact(Timeout = 120000)]
    public async Task RecoverAddressFromPersonalSign_ReturnsSameAddress()
    {
        var wallet = await PrivateKeyWallet.Generate(_client);
        var message = "Hello, world!";
        var signature = await wallet.PersonalSign(message);
        var recoveredAddress = await wallet.RecoverAddressFromPersonalSign(message, signature);
        Assert.Equal(await wallet.GetAddress(), recoveredAddress);
    }

    [Fact(Timeout = 120000)]
    public async Task RecoverAddressFromPersonalSign_ReturnsSameAddress_SmartWallet()
    {
        var wallet = await GetAccount();
        var message = "Hello, world!";
        var signature = await wallet.PersonalSign(message);
        var recoveredAddress = await wallet.RecoverAddressFromPersonalSign(message, signature);
        Assert.Equal(await wallet.GetAddress(), recoveredAddress);
    }

    [Fact(Timeout = 120000)]
    public async Task RecoverAddressFromEthSign_InvalidSignature()
    {
        var wallet = await PrivateKeyWallet.Generate(_client);
        var wallet2 = await PrivateKeyWallet.Generate(_client);
        var message = "Hello, world!";
        var signature = await wallet2.EthSign(message);
        var recoveredAddress = await wallet.RecoverAddressFromEthSign(message, signature);
        Assert.NotEqual(await wallet.GetAddress(), recoveredAddress);
    }

    [Fact(Timeout = 120000)]
    public async Task RecoverAddressFromPersonalSign_InvalidSignature()
    {
        var wallet = await PrivateKeyWallet.Generate(_client);
        var wallet2 = await PrivateKeyWallet.Generate(_client);
        var message = "Hello, world!";
        var signature = await wallet2.PersonalSign(message);
        var recoveredAddress = await wallet.RecoverAddressFromPersonalSign(message, signature);
        Assert.NotEqual(await wallet.GetAddress(), recoveredAddress);
    }

    [Fact(Timeout = 120000)]
    public async Task RecoverAddressFromPersonalSign_InvalidSignature_SmartWallet()
    {
        var wallet = await GetAccount();
        var wallet2 = await GetAccount();
        var message = "Hello, world!";
        var signature = await wallet2.PersonalSign(message);
        var recoveredAddress = await wallet.RecoverAddressFromPersonalSign(message, signature);
        Assert.NotEqual(await wallet.GetAddress(), recoveredAddress);
    }

    [Fact(Timeout = 120000)]
    public async Task RecoverAddress_AllVariants_NullTests()
    {
        var wallet = await PrivateKeyWallet.Generate(_client);
        var message = "Hello, world!";
        var signature = await wallet.PersonalSign(message);

        _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await wallet.RecoverAddressFromEthSign(null, signature));
        _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await wallet.RecoverAddressFromEthSign(message, null));
        _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await wallet.RecoverAddressFromPersonalSign(null, signature));
        _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await wallet.RecoverAddressFromPersonalSign(message, null));

#nullable disable
        var nullData = null as AccountAbstraction.SignerPermissionRequest;
        var nullTypedData = null as Nethereum.ABI.EIP712.TypedData<Nethereum.ABI.EIP712.Domain>;
        var nullSig = null as string;
        _ = await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await wallet.RecoverAddressFromTypedDataV4<AccountAbstraction.SignerPermissionRequest, Nethereum.ABI.EIP712.Domain>(nullData, nullTypedData, nullSig)
        );
        _ = await Assert.ThrowsAsync<ArgumentNullException>(
            async () =>
                await wallet.RecoverAddressFromTypedDataV4<AccountAbstraction.SignerPermissionRequest, Nethereum.ABI.EIP712.Domain>(
                    new AccountAbstraction.SignerPermissionRequest(),
                    nullTypedData,
                    nullSig
                )
        );
        _ = await Assert.ThrowsAsync<ArgumentNullException>(
            async () =>
                await wallet.RecoverAddressFromTypedDataV4<AccountAbstraction.SignerPermissionRequest, Nethereum.ABI.EIP712.Domain>(
                    new AccountAbstraction.SignerPermissionRequest(),
                    new Nethereum.ABI.EIP712.TypedData<Nethereum.ABI.EIP712.Domain>(),
                    nullSig
                )
        );
#nullable restore
    }
}
