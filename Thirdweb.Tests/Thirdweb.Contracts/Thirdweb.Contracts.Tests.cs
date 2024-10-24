using System.Numerics;

namespace Thirdweb.Tests.Contracts;

public class ContractsTests : BaseTests
{
    public ContractsTests(ITestOutputHelper output)
        : base(output) { }

    [Fact(Timeout = 120000)]
    public async Task FetchAbi()
    {
        var abi = await ThirdwebContract.FetchAbi(client: this.Client, address: "0x1320Cafa93fb53Ed9068E3272cb270adbBEf149C", chainId: 84532);
        Assert.NotNull(abi);
        Assert.NotEmpty(abi);
    }

    [Fact(Timeout = 120000)]
    public async Task InitTest_NullClient()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebContract.Create(null, "0x123", 1, "[]"));
        Assert.Contains("Client must be provided", exception.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task InitTest_NullAddress()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebContract.Create(this.Client, null, 1, "[]"));
        Assert.Contains("Address must be provided", exception.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task InitTest_ZeroChain()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebContract.Create(this.Client, "0x123", 0, "[]"));
        Assert.Contains("Chain must be provided", exception.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task InitTest_NullAbi()
    {
        var res = await ThirdwebContract.Create(this.Client, "0x81ebd23aA79bCcF5AaFb9c9c5B0Db4223c39102e", 421614, null);
        Assert.NotNull(res);
    }

    [Fact(Timeout = 120000)]
    public async Task ReadTest_String()
    {
        var contract = await this.GetContract();
        var result = await ThirdwebContract.Read<string>(contract, "name");
        Assert.Equal("Kitty DropERC20", result);
    }

    [Fact(Timeout = 120000)]
    public async Task ReadTest_BigInteger()
    {
        var contract = await this.GetContract();
        var result = await ThirdwebContract.Read<BigInteger>(contract, "decimals");
        Assert.Equal(18, result);
    }

    [Nethereum.ABI.FunctionEncoding.Attributes.FunctionOutput]
    private sealed class GetPlatformFeeInfoOutputDTO : Nethereum.ABI.FunctionEncoding.Attributes.IFunctionOutputDTO
    {
        [Nethereum.ABI.FunctionEncoding.Attributes.Parameter("address", "", 1)]
        public required string ReturnValue1 { get; set; }

        [Nethereum.ABI.FunctionEncoding.Attributes.Parameter("uint16", "", 2)]
        public required ushort ReturnValue2 { get; set; }
    }

    [Fact(Timeout = 120000)]
    public async Task ReadTest_Tuple()
    {
        var contract = await this.GetContract();
        var result = await ThirdwebContract.Read<GetPlatformFeeInfoOutputDTO>(contract, "getPlatformFeeInfo");
        Assert.Equal("0xDaaBDaaC8073A7dAbdC96F6909E8476ab4001B34", result.ReturnValue1);
        Assert.Equal(0, result.ReturnValue2);
    }

    [Fact(Timeout = 120000)]
    public async Task ReadTest_FullSig()
    {
        var contract = await this.GetContract();
        var result = await ThirdwebContract.Read<string>(contract, "function name() view returns (string)");
        Assert.Equal("Kitty DropERC20", result);
    }

    [Fact(Timeout = 120000)]
    public async Task ReadTest_PartialSig()
    {
        var contract = await this.GetContract();
        var result = await ThirdwebContract.Read<string>(contract, "name()");
        Assert.Equal("Kitty DropERC20", result);
    }

    [Fact(Timeout = 120000)]
    public async Task PrepareTest_NoSig()
    {
        var contract = await this.GetContract();
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebContract.Prepare(null, contract, "sup", 0));
        Assert.Contains("Method signature not found in contract ABI.", exception.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task ReadTest_TupleIntoArray()
    {
        var contract = await ThirdwebContract.Create(this.Client, "0xEb4AAB0253a50918a2Cbb7ADBaab78Ad19C07Bb1", 421614);
        var result = await contract.Read<List<object>>("getReserves");
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Count == 3);
    }

    [Fact(Timeout = 120000)]
    public async Task ReadTest_TupleIntoList()
    {
        var contract = await ThirdwebContract.Create(this.Client, "0xEb4AAB0253a50918a2Cbb7ADBaab78Ad19C07Bb1", 421614);
        var result = await contract.Read<List<object>>("getReserves");
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Count == 3);
    }

    private sealed class AllowlistProof
    {
        public List<byte[]> Proof { get; set; } = new List<byte[]>();
        public BigInteger QuantityLimitPerWallet { get; set; } = BigInteger.Zero;
        public BigInteger PricePerToken { get; set; } = BigInteger.Zero;
        public string Currency { get; set; } = Constants.ADDRESS_ZERO;
    }

    [Fact(Timeout = 120000)]
    public async Task WriteTest_SmartAccount()
    {
        var contract = await this.GetContract();
        var smartAccount = await this.GetAccount();
        var receiver = await smartAccount.GetAddress();
        var quantity = BigInteger.One;
        var currency = Constants.NATIVE_TOKEN_ADDRESS;
        var pricePerToken = BigInteger.Zero;
        var allowlistProof = new object[] { Array.Empty<byte>(), BigInteger.Zero, BigInteger.Zero, Constants.ADDRESS_ZERO };
        var data = Array.Empty<byte>();
        var result = await ThirdwebContract.Write(smartAccount, contract, "claim", 0, receiver, quantity, currency, pricePerToken, allowlistProof, data);
        Assert.NotNull(result);
        var receipt = await ThirdwebTransaction.WaitForTransactionReceipt(contract.Client, contract.Chain, result.TransactionHash);
        Assert.NotNull(receipt);
        Assert.Equal(result.TransactionHash, receipt.TransactionHash);
    }

    [Fact(Timeout = 120000)]
    public async Task WriteTest_SmartAccount_FullSig()
    {
        var contract = await this.GetContract();
        var smartAccount = await this.GetAccount();
        var receiver = await smartAccount.GetAddress();
        var quantity = BigInteger.One;
        var currency = Constants.NATIVE_TOKEN_ADDRESS;
        var pricePerToken = BigInteger.Zero;
        var allowlistProof = new object[] { Array.Empty<byte>(), BigInteger.Zero, BigInteger.Zero, Constants.ADDRESS_ZERO };
        var data = Array.Empty<byte>();
        var result = await ThirdwebContract.Write(
            smartAccount,
            contract,
            "claim(address, uint256, address, uint256, (bytes32[], uint256, uint256, address), bytes)",
            0,
            receiver,
            quantity,
            currency,
            pricePerToken,
            allowlistProof,
            data
        );
        Assert.NotNull(result);
        var receipt = await ThirdwebTransaction.WaitForTransactionReceipt(contract.Client, contract.Chain, result.TransactionHash);
        Assert.NotNull(receipt);
        Assert.Equal(result.TransactionHash, receipt.TransactionHash);
    }

    [Fact(Timeout = 120000)]
    public async Task WriteTest_PrivateKeyAccount()
    {
        var contract = await this.GetContract();
        var privateKeyAccount = await PrivateKeyWallet.Generate(contract.Client);
        var receiver = await privateKeyAccount.GetAddress();
        var quantity = BigInteger.One;
        var currency = Constants.NATIVE_TOKEN_ADDRESS;
        var pricePerToken = BigInteger.Zero;
        var allowlistProof = new object[] { Array.Empty<byte>(), BigInteger.Zero, BigInteger.Zero, Constants.ADDRESS_ZERO };
        var data = Array.Empty<byte>();
        try
        {
            var res = await ThirdwebContract.Write(privateKeyAccount, contract, "claim", 0, receiver, quantity, currency, pricePerToken, allowlistProof, data);
            Assert.NotNull(res);
            Assert.NotNull(res.TransactionHash);
            Assert.Equal(66, res.TransactionHash.Length);
        }
        catch (Exception ex)
        {
            Assert.Contains("insufficient funds", ex.Message);
        }
    }

    [Fact(Timeout = 120000)]
    public async Task SignatureMint_Generate()
    {
        var client = this.Client;
        var signer = await PrivateKeyWallet.Generate(client);

        var randomDomain = "Test";
        var randomVersion = "1.0.0";
        var randomChainId = 421614;
        var randomContractAddress = "0xD04F98C88cE1054c90022EE34d566B9237a1203C";

        // GenerateSignature_MinimalForwarder
        var forwardRequest = new Forwarder_ForwardRequest
        {
            From = "0x123",
            To = "0x456",
            Value = BigInteger.Zero,
            Gas = BigInteger.Zero,
            Nonce = BigInteger.Zero,
            Data = "0x"
        };
        var signature = await EIP712.GenerateSignature_MinimalForwarder(randomDomain, randomVersion, randomChainId, randomContractAddress, forwardRequest, signer);
        Assert.NotNull(signature);
        Assert.StartsWith("0x", signature);
        // GenerateSignature_TokenERC20
        var mintRequest20 = new TokenERC20_MintRequest
        {
            To = await signer.GetAddress(),
            PrimarySaleRecipient = await signer.GetAddress(),
            Quantity = 1,
            Price = 0,
            Currency = Constants.ADDRESS_ZERO,
            ValidityEndTimestamp = 0,
            ValidityStartTimestamp = Utils.GetUnixTimeStampIn10Years(),
            Uid = new byte[] { 0x01 }
        };
        var signature20 = await EIP712.GenerateSignature_TokenERC20(randomDomain, randomVersion, randomChainId, randomContractAddress, mintRequest20, signer);
        Assert.NotNull(signature20);
        Assert.StartsWith("0x", signature20);

        // GenerateSignature_TokenERC721
        var mintRequest721 = new TokenERC721_MintRequest
        {
            To = await signer.GetAddress(),
            RoyaltyRecipient = await signer.GetAddress(),
            RoyaltyBps = 0,
            PrimarySaleRecipient = await signer.GetAddress(),
            Uri = "https://example.com",
            Price = 0,
            Currency = Constants.ADDRESS_ZERO,
            ValidityEndTimestamp = 0,
            ValidityStartTimestamp = Utils.GetUnixTimeStampIn10Years(),
            Uid = new byte[] { 0x01 }
        };
        var signature721 = await EIP712.GenerateSignature_TokenERC721(randomDomain, randomVersion, randomChainId, randomContractAddress, mintRequest721, signer);
        Assert.NotNull(signature721);
        Assert.StartsWith("0x", signature721);

        // GenerateSignature_TokenERC1155
        var mintRequest1155 = new TokenERC1155_MintRequest
        {
            To = await signer.GetAddress(),
            RoyaltyRecipient = await signer.GetAddress(),
            RoyaltyBps = 0,
            PrimarySaleRecipient = await signer.GetAddress(),
            TokenId = 1,
            Uri = "https://example.com",
            Quantity = 1,
            PricePerToken = 0,
            Currency = Constants.ADDRESS_ZERO,
            ValidityEndTimestamp = 0,
            ValidityStartTimestamp = Utils.GetUnixTimeStampIn10Years(),
            Uid = new byte[] { 0x01 }
        };
        var signature1155 = await EIP712.GenerateSignature_TokenERC1155(randomDomain, randomVersion, randomChainId, randomContractAddress, mintRequest1155, signer);
        Assert.NotNull(signature1155);
        Assert.StartsWith("0x", signature1155);
    }

    private async Task<SmartWallet> GetAccount()
    {
        var client = this.Client;
        var privateKeyAccount = await PrivateKeyWallet.Generate(client);
        var smartAccount = await SmartWallet.Create(personalWallet: privateKeyAccount, factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052", gasless: true, chainId: 421614);
        return smartAccount;
    }

    private async Task<ThirdwebContract> GetContract()
    {
        var client = this.Client;
        var contract = await ThirdwebContract.Create(client: client, address: "0xEBB8a39D865465F289fa349A67B3391d8f910da9", chain: 421614); // DropERC20
        return contract;
    }
}
