using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;

namespace Thirdweb.Tests;

public class PrivateKeyAccountTests : BaseTests
{
    public PrivateKeyAccountTests(ITestOutputHelper output)
        : base(output) { }

    private async Task<PrivateKeyAccount> GetAccount()
    {
        var client = ThirdwebClient.Create(secretKey: _secretKey);
        var privateKeyAccount = await PrivateKeyAccount.Create(client: client, privateKeyHex: _testPrivateKey);
        return privateKeyAccount;
    }

    [Fact]
    public async Task Initialization_Success()
    {
        var account = await GetAccount();
        Assert.NotNull(account);
    }

    [Fact]
    public async void Initialization_NullPrivateKey()
    {
        var client = ThirdwebClient.Create(secretKey: _secretKey);
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () => await PrivateKeyAccount.Create(client, null));
        Assert.Equal("Private key cannot be null or empty. (Parameter 'privateKeyHex')", ex.Message);
    }

    [Fact]
    public async Task Connect()
    {
        var account = await GetAccount();
        Assert.True(await account.IsConnected());
    }

    [Fact]
    public async Task GetAddress()
    {
        var account = await GetAccount();
        var address = await account.GetAddress();
        Assert.True(address.Length == 42);
    }

    [Fact]
    public async Task EthSign_Success()
    {
        var account = await GetAccount();
        var message = "Hello, World!";
        var signature = await account.EthSign(message);
        Assert.True(signature.Length == 132);
    }

    [Fact]
    public async Task EthSign_NullMessage()
    {
        var account = await GetAccount();
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => account.EthSign(null));
        Assert.Equal("Message to sign cannot be null. (Parameter 'message')", ex.Message);
    }

    [Fact]
    public async Task PersonalSign_Success()
    {
        var account = await GetAccount();
        var message = "Hello, World!";
        var signature = await account.PersonalSign(message);
        Assert.True(signature.Length == 132);
    }

    [Fact]
    public async Task PersonalSign_EmptyMessage()
    {
        var account = await GetAccount();
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => account.PersonalSign(string.Empty));
        Assert.Equal("Message to sign cannot be null. (Parameter 'message')", ex.Message);
    }

    [Fact]
    public async Task PersonalSign_NullyMessage()
    {
        var account = await GetAccount();

        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => account.PersonalSign(null as string));
        Assert.Equal("Message to sign cannot be null. (Parameter 'message')", ex.Message);
    }

    [Fact]
    public async Task PersonalSignRaw_Success()
    {
        var account = await GetAccount();
        var message = System.Text.Encoding.UTF8.GetBytes("Hello, World!");
        var signature = await account.PersonalSign(message);
        Assert.True(signature.Length == 132);
    }

    [Fact]
    public async Task PersonalSignRaw_NullMessage()
    {
        var account = await GetAccount();
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => account.PersonalSign(null as byte[]));
        Assert.Equal("Message to sign cannot be null. (Parameter 'rawMessage')", ex.Message);
    }

    [Fact]
    public async Task SignTypedDataV4_Success()
    {
        var account = await GetAccount();
        var json =
            "{\"types\":{\"EIP712Domain\":[{\"name\":\"name\",\"type\":\"string\"},{\"name\":\"version\",\"type\":\"string\"},{\"name\":\"chainId\",\"type\":\"uint256\"},{\"name\":\"verifyingContract\",\"type\":\"address\"}],\"Person\":[{\"name\":\"name\",\"type\":\"string\"},{\"name\":\"wallet\",\"type\":\"address\"}],\"Mail\":[{\"name\":\"from\",\"type\":\"Person\"},{\"name\":\"to\",\"type\":\"Person\"},{\"name\":\"contents\",\"type\":\"string\"}]},\"primaryType\":\"Mail\",\"domain\":{\"name\":\"Ether Mail\",\"version\":\"1\",\"chainId\":1,\"verifyingContract\":\"0xCcCCccccCCCCcCCCCCCcCcCccCcCCCcCcccccccC\"},\"message\":{\"from\":{\"name\":\"Cow\",\"wallet\":\"0xCD2a3d9F938E13CD947Ec05AbC7FE734Df8DD826\"},\"to\":{\"name\":\"Bob\",\"wallet\":\"0xbBbBBBBbbBBBbbbBbbBbbBBbBbbBbBbBbBbbBBbB\"},\"contents\":\"Hello, Bob!\"}}";
        var signature = await account.SignTypedDataV4(json);
        Assert.True(signature.Length == 132);
    }

    [Fact]
    public async Task SignTypedDataV4_NullJson()
    {
        var account = await GetAccount();
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => account.SignTypedDataV4(null));
        Assert.Equal("Json to sign cannot be null. (Parameter 'json')", ex.Message);
    }

    [Fact]
    public async Task SignTypedDataV4_EmptyJson()
    {
        var account = await GetAccount();
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => account.SignTypedDataV4(string.Empty));
        Assert.Equal("Json to sign cannot be null. (Parameter 'json')", ex.Message);
    }

    [Fact]
    public async Task SignTypedDataV4_Typed()
    {
        var account = await GetAccount();
        var typedData = EIP712.GetTypedDefinition_SmartAccount_AccountMessage("Account", "1", 421614, await account.GetAddress());
        var accountMessage = new AccountAbstraction.AccountMessage { Message = System.Text.Encoding.UTF8.GetBytes("Hello, world!") };
        var signature = await account.SignTypedDataV4(accountMessage, typedData);
        Assert.True(signature.Length == 132);
    }

    [Fact]
    public async Task SignTypedDataV4_Typed_NullData()
    {
        var account = await GetAccount();
        var typedData = EIP712.GetTypedDefinition_SmartAccount_AccountMessage("Account", "1", 421614, await account.GetAddress());
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => account.SignTypedDataV4(null as string, typedData));
        Assert.Equal("Data to sign cannot be null. (Parameter 'data')", ex.Message);
    }

    [Fact]
    public async Task SignTransaction_Success()
    {
        var account = await GetAccount();
        var transaction = new TransactionInput
        {
            From = await account.GetAddress(),
            To = Constants.ADDRESS_ZERO,
            // Value = new HexBigInteger(0),
            Gas = new HexBigInteger(21000),
            Data = "0x",
            Nonce = new HexBigInteger(99999999999),
            GasPrice = new HexBigInteger(10000000000)
        };
        var signature = await account.SignTransaction(transaction, 421614);
        Assert.NotNull(signature);
    }

    [Fact]
    public async Task SignTransaction_NullTransaction()
    {
        var account = await GetAccount();
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => account.SignTransaction(null, 421614));
        Assert.Equal("Value cannot be null. (Parameter 'transaction')", ex.Message);
    }

    [Fact]
    public async Task SignTransaction_NoNonce()
    {
        var account = await GetAccount();
        var transaction = new TransactionInput
        {
            From = await account.GetAddress(),
            To = Constants.ADDRESS_ZERO,
            Value = new HexBigInteger(0),
            Gas = new HexBigInteger(21000),
            Data = "0x"
        };
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => account.SignTransaction(transaction, 421614));
        Assert.Equal("Transaction nonce has not been set (Parameter 'transaction')", ex.Message);
    }

    [Fact]
    public async Task SignTransaction_WrongFrom()
    {
        var account = await GetAccount();
        var transaction = new TransactionInput
        {
            From = Constants.ADDRESS_ZERO,
            To = Constants.ADDRESS_ZERO,
            Value = new HexBigInteger(0),
            Gas = new HexBigInteger(21000),
            Data = "0x",
            Nonce = new HexBigInteger(99999999999)
        };
        var ex = await Assert.ThrowsAsync<Exception>(() => account.SignTransaction(transaction, 421614));
        Assert.Equal("Transaction 'From' address does not match the wallet address", ex.Message);
    }

    [Fact]
    public async Task SignTransaction_NoGasPrice()
    {
        var account = await GetAccount();
        var transaction = new TransactionInput
        {
            From = await account.GetAddress(),
            To = Constants.ADDRESS_ZERO,
            Value = new HexBigInteger(0),
            Gas = new HexBigInteger(21000),
            Data = "0x",
            Nonce = new HexBigInteger(99999999999)
        };
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => account.SignTransaction(transaction, 421614));
        Assert.Equal("Transaction gas price must be set for legacy transactions", ex.Message);
    }

    [Fact]
    public async Task SignTransaction_1559_Success()
    {
        var account = await GetAccount();
        var transaction = new TransactionInput
        {
            From = await account.GetAddress(),
            To = Constants.ADDRESS_ZERO,
            Value = new HexBigInteger(0),
            Gas = new HexBigInteger(21000),
            Data = "0x",
            Nonce = new HexBigInteger(99999999999),
            Type = new HexBigInteger(2),
            MaxFeePerGas = new HexBigInteger(10000000000),
            MaxPriorityFeePerGas = new HexBigInteger(10000000000)
        };
        var signature = await account.SignTransaction(transaction, 421614);
        Assert.NotNull(signature);
    }

    [Fact]
    public async Task SignTransaction_1559_NoMaxFeePerGas()
    {
        var account = await GetAccount();
        var transaction = new TransactionInput
        {
            From = await account.GetAddress(),
            To = Constants.ADDRESS_ZERO,
            Value = new HexBigInteger(0),
            Gas = new HexBigInteger(21000),
            Data = "0x",
            Nonce = new HexBigInteger(99999999999),
            Type = new HexBigInteger(2),
            MaxPriorityFeePerGas = new HexBigInteger(10000000000)
        };
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => account.SignTransaction(transaction, 421614));
        Assert.Equal("Transaction MaxPriorityFeePerGas and MaxFeePerGas must be set for EIP-1559 transactions", ex.Message);
    }

    [Fact]
    public async Task SignTransaction_1559_NoMaxPriorityFeePerGas()
    {
        var account = await GetAccount();
        var transaction = new TransactionInput
        {
            From = await account.GetAddress(),
            To = Constants.ADDRESS_ZERO,
            Value = new HexBigInteger(0),
            Gas = new HexBigInteger(21000),
            Data = "0x",
            Nonce = new HexBigInteger(99999999999),
            Type = new HexBigInteger(2),
            MaxFeePerGas = new HexBigInteger(10000000000)
        };
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => account.SignTransaction(transaction, 421614));
        Assert.Equal("Transaction MaxPriorityFeePerGas and MaxFeePerGas must be set for EIP-1559 transactions", ex.Message);
    }

    [Fact]
    public async Task IsConnected_True()
    {
        var account = await GetAccount();
        Assert.True(await account.IsConnected());
    }

    [Fact]
    public async Task IsConnected_False()
    {
        var account = await GetAccount();
        await account.Disconnect();
        Assert.False(await account.IsConnected());
    }

    [Fact]
    public async Task Disconnect()
    {
        var account = await GetAccount();
        await account.Disconnect();
        Assert.False(await account.IsConnected());
    }

    [Fact]
    public async Task Disconnect_NotConnected()
    {
        var account = await GetAccount();
        await account.Disconnect();
        Assert.False(await account.IsConnected());
    }

    [Fact]
    public async Task Disconnect_Connected()
    {
        var account = await GetAccount();
        await account.Disconnect();
        Assert.False(await account.IsConnected());
    }
}
