using System.Numerics;
using Newtonsoft.Json.Linq;

namespace Thirdweb.Tests;

public class ContractsTests : BaseTests
{
    public ContractsTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task FetchAbi()
    {
        var abi = await ThirdwebContract.FetchAbi(address: "0x1320Cafa93fb53Ed9068E3272cb270adbBEf149C", chainId: 84532);
        Assert.NotNull(abi);
        Assert.NotEmpty(abi);
    }

    [Fact]
    public async Task InitTest_NullClient()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebContract.Create(null, "0x123", 1, "[]"));
        Assert.Contains("Client must be provided", exception.Message);
    }

    [Fact]
    public async Task InitTest_NullAddress()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebContract.Create(ThirdwebClient.Create(secretKey: _secretKey), null, 1, "[]"));
        Assert.Contains("Address must be provided", exception.Message);
    }

    [Fact]
    public async Task InitTest_ZeroChain()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebContract.Create(ThirdwebClient.Create(secretKey: _secretKey), "0x123", 0, "[]"));
        Assert.Contains("Chain must be provided", exception.Message);
    }

    [Fact]
    public async Task InitTest_NullAbi()
    {
        var res = await ThirdwebContract.Create(ThirdwebClient.Create(secretKey: _secretKey), "0x81ebd23aA79bCcF5AaFb9c9c5B0Db4223c39102e", 421614, null);
        Assert.NotNull(res);
    }

    [Fact]
    public async Task ReadTest_String()
    {
        var contract = await GetContract();
        var result = await ThirdwebContract.Read<string>(contract, "name");
        Assert.Equal("Kitty DropERC20", result);
    }

    [Fact]
    public async Task ReadTest_BigInteger()
    {
        var contract = await GetContract();
        var result = await ThirdwebContract.Read<BigInteger>(contract, "decimals");
        Assert.Equal(18, result);
    }

    [Nethereum.ABI.FunctionEncoding.Attributes.FunctionOutput]
    private class GetPlatformFeeInfoOutputDTO : Nethereum.ABI.FunctionEncoding.Attributes.IFunctionOutputDTO
    {
        [Nethereum.ABI.FunctionEncoding.Attributes.Parameter("address", "", 1)]
        public virtual required string ReturnValue1 { get; set; }

        [Nethereum.ABI.FunctionEncoding.Attributes.Parameter("uint16", "", 2)]
        public virtual required ushort ReturnValue2 { get; set; }
    }

    [Fact]
    public async Task ReadTest_Tuple()
    {
        var contract = await GetContract();
        var result = await ThirdwebContract.Read<GetPlatformFeeInfoOutputDTO>(contract, "getPlatformFeeInfo");
        Assert.Equal("0xDaaBDaaC8073A7dAbdC96F6909E8476ab4001B34", result.ReturnValue1);
        Assert.Equal(0, result.ReturnValue2);
    }

    private class AllowlistProof
    {
        public List<byte[]> Proof { get; set; } = new();
        public BigInteger QuantityLimitPerWallet { get; set; } = BigInteger.Zero;
        public BigInteger PricePerToken { get; set; } = BigInteger.Zero;
        public string Currency { get; set; } = Constants.ADDRESS_ZERO;
    }

    [Fact]
    public async Task WriteTest_SmartAccount()
    {
        var contract = await GetContract();
        var smartAccount = await GetAccount();
        var receiver = await smartAccount.GetAddress();
        var quantity = BigInteger.One;
        var currency = Constants.NATIVE_TOKEN_ADDRESS;
        var pricePerToken = BigInteger.Zero;
        var allowlistProof = new object[] { new byte[] { }, BigInteger.Zero, BigInteger.Zero, Constants.ADDRESS_ZERO };
        var data = new byte[] { };
        var result = await ThirdwebContract.Write(smartAccount, contract, "claim", 0, receiver, quantity, currency, pricePerToken, allowlistProof, data);
        Assert.NotNull(result);
        var receipt = await ThirdwebTransaction.WaitForTransactionReceipt(contract.Client, contract.Chain, result.TransactionHash);
        Assert.NotNull(receipt);
        Assert.Equal(result.TransactionHash, receipt.TransactionHash);
    }

    [Fact]
    public async Task WriteTest_PrivateKeyAccount()
    {
        var contract = await GetContract();
        var privateKeyAccount = await PrivateKeyWallet.Create(contract.Client, _testPrivateKey);
        var receiver = await privateKeyAccount.GetAddress();
        var quantity = BigInteger.One;
        var currency = Constants.NATIVE_TOKEN_ADDRESS;
        var pricePerToken = BigInteger.Zero;
        var allowlistProof = new object[] { new byte[] { }, BigInteger.Zero, BigInteger.Zero, Constants.ADDRESS_ZERO };
        var data = new byte[] { };
        var exception = await Assert.ThrowsAsync<Exception>(
            async () => await ThirdwebContract.Write(privateKeyAccount, contract, "claim", 0, receiver, quantity, currency, pricePerToken, allowlistProof, data)
        );
        Assert.Contains("insufficient funds", exception.Message);
    }

    private async Task<SmartWallet> GetAccount()
    {
        var client = ThirdwebClient.Create(secretKey: _secretKey);
        var privateKeyAccount = await PrivateKeyWallet.Create(client, _testPrivateKey);
        var smartAccount = await SmartWallet.Create(client, personalWallet: privateKeyAccount, factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052", gasless: true, chainId: 421614);
        return smartAccount;
    }

    private async Task<ThirdwebContract> GetContract()
    {
        var client = ThirdwebClient.Create(secretKey: _secretKey);
        var contract = await ThirdwebContract.Create(client: client, address: "0xEBB8a39D865465F289fa349A67B3391d8f910da9", chain: 421614);
        return contract;
    }
}
