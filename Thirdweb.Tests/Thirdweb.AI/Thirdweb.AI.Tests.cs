using System.Numerics;
using Thirdweb.AI;

namespace Thirdweb.Tests.AI;

public class NebulaTests : BaseTests
{
    private const string NEBULA_TEST_CONTRACT = "0xe2cb0eb5147b42095c2FfA6F7ec953bb0bE347D8";
    private const int NEBULA_TEST_CHAIN = 11155111;

    public NebulaTests(ITestOutputHelper output)
        : base(output) { }

    [Fact(Timeout = 120000)]
    public async Task Create_CreatesSession()
    {
        var nebula = await ThirdwebNebula.Create(this.Client);
        Assert.NotNull(nebula);
        Assert.NotNull(nebula.SessionId);
    }

    [Fact(Timeout = 120000)]
    public async Task Create_ResumesSession()
    {
        var nebula = await ThirdwebNebula.Create(this.Client);
        var sessionId = nebula.SessionId;
        Assert.NotNull(nebula);
        Assert.NotNull(nebula.SessionId);

        nebula = await ThirdwebNebula.Create(this.Client, sessionId);
        Assert.NotNull(nebula);
        Assert.Equal(sessionId, nebula.SessionId);
    }

    [Fact(Timeout = 120000)]
    public async Task Chat_Single_ReturnsResponse()
    {
        var nebula = await ThirdwebNebula.Create(this.Client);
        var response = await nebula.Chat(
            message: "What's the symbol of this contract?",
            context: new NebulaContext(contractAddresses: new List<string> { NEBULA_TEST_CONTRACT }, chainIds: new List<BigInteger> { NEBULA_TEST_CHAIN })
        );
        Assert.NotNull(response);
        Assert.NotNull(response.Message);
        Assert.Contains("CAT", response.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task Chat_Single_NoContext_ReturnsResponse()
    {
        var nebula = await ThirdwebNebula.Create(this.Client);
        var response = await nebula.Chat(message: $"What's the symbol of this contract: {NEBULA_TEST_CONTRACT} (Sepolia)?");
        Assert.NotNull(response);
        Assert.NotNull(response.Message);
        Assert.Contains("CAT", response.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task Chat_Multiple_ReturnsResponse()
    {
        var nebula = await ThirdwebNebula.Create(this.Client);
        var response = await nebula.Chat(
            messages: new List<NebulaChatMessage>
            {
                new("What's the symbol of this contract?", NebulaChatRole.User),
                new("The symbol is CAT", NebulaChatRole.Assistant),
                new("What's the name of this contract?", NebulaChatRole.User),
            },
            context: new NebulaContext(contractAddresses: new List<string> { NEBULA_TEST_CONTRACT }, chainIds: new List<BigInteger> { NEBULA_TEST_CHAIN })
        );
        Assert.NotNull(response);
        Assert.NotNull(response.Message);
        Assert.Contains("CatDrop", response.Message);
    }

    [Fact(Timeout = 120000)]
    public async Task Chat_UnderstandsWalletContext()
    {
        var wallet = await PrivateKeyWallet.Generate(this.Client);
        var expectedAddress = await wallet.GetAddress();
        var nebula = await ThirdwebNebula.Create(this.Client);
        var response = await nebula.Chat(message: "What is my wallet address?", wallet: wallet);
        Assert.NotNull(response);
        Assert.NotNull(response.Message);
        Assert.Contains(expectedAddress, response.Message);
    }

    // [Fact(Timeout = 120000)]
    // public async Task Execute_ReturnsMessageAndReceipt()
    // {
    //     var signer = await PrivateKeyWallet.Generate(this.Client);
    //     var wallet = await SmartWallet.Create(signer, NEBULA_TEST_CHAIN);
    //     var nebula = await ThirdwebNebula.Create(this.Client);
    //     var response = await nebula.Execute("Send 0 ETH to vitalik.eth", wallet: wallet);
    //     Assert.NotNull(response);
    //     Assert.NotNull(response.Message);
    //     Assert.NotNull(response.TransactionReceipts);
    //     Assert.NotEmpty(response.TransactionReceipts);
    //     Assert.NotNull(response.TransactionReceipts[0].TransactionHash);
    //     Assert.True(response.TransactionReceipts[0].TransactionHash.Length == 66);
    // }

    // [Fact(Timeout = 120000)]
    // public async Task Execute_ReturnsMessageAndReceipts()
    // {
    //     var signer = await PrivateKeyWallet.Generate(this.Client);
    //     var wallet = await SmartWallet.Create(signer, NEBULA_TEST_CHAIN);
    //     var nebula = await ThirdwebNebula.Create(this.Client);
    //     var response = await nebula.Execute(
    //         new List<NebulaChatMessage> { new("Send 0 ETH to vitalik.eth and satoshi.eth", NebulaChatRole.User), new("Are you sure?", NebulaChatRole.Assistant), new("Yes", NebulaChatRole.User) },
    //         wallet: wallet
    //     );
    //     Assert.NotNull(response);
    //     Assert.NotNull(response.Message);
    //     Assert.NotNull(response.TransactionReceipts);
    //     Assert.NotEmpty(response.TransactionReceipts);
    //     Assert.NotNull(response.TransactionReceipts[0].TransactionHash);
    //     Assert.True(response.TransactionReceipts[0].TransactionHash.Length == 66);
    //     Assert.NotNull(response.TransactionReceipts[1].TransactionHash);
    //     Assert.True(response.TransactionReceipts[1].TransactionHash.Length == 66);
    // }
}
