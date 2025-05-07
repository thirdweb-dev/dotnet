// using System.Numerics;
using System.Numerics;
using Thirdweb.AI;

namespace Thirdweb.Tests.AI;

public class NebulaTests : BaseTests
{
    // private const string NEBULA_TEST_USDC_ADDRESS = "0x1c7D4B196Cb0C7B01d743Fbc6116a902379C7238";

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
        var response = await nebula.Chat(message: $"What's the symbol of this contract {NEBULA_TEST_CONTRACT}?", context: new NebulaContext(chainIds: new List<BigInteger>() { NEBULA_TEST_CHAIN }));
        Assert.NotNull(response);
        Assert.NotNull(response.Message);
        Assert.Contains("CAT", response.Message);
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
    //     var response = await nebula.Execute(
    //         new List<NebulaChatMessage>
    //         {
    //             new("What's the address of vitalik.eth", NebulaChatRole.User),
    //             new("The address of vitalik.eth is 0xd8dA6BF26964aF8E437eEa5e3616511D7G3a3298", NebulaChatRole.Assistant),
    //             new($"Approve 1 USDC (this contract: {NEBULA_TEST_USDC_ADDRESS}) to them", NebulaChatRole.User),
    //         },
    //         wallet: wallet
    //     );
    //     Assert.NotNull(response);
    //     Assert.NotNull(response.Message);
    //     Assert.NotNull(response.TransactionReceipts);
    //     Assert.NotEmpty(response.TransactionReceipts);
    //     Assert.NotNull(response.TransactionReceipts[0].TransactionHash);
    //     Assert.True(response.TransactionReceipts[0].TransactionHash.Length == 66);
    // }
}
