using System.Numerics;

namespace Thirdweb.Tests
{
    public class CommonExtensionTests : BaseTests
    {
        private readonly string _erc20ContractAddress = "0x81ebd23aA79bCcF5AaFb9c9c5B0Db4223c39102e";
        private readonly BigInteger _chainId = 421614;

        public CommonExtensionTests(ITestOutputHelper output)
            : base(output) { }

        private async Task<ThirdwebContract> GetContract()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc20ContractAddress, _chainId);
            return contract;
        }

        [Fact]
        public async Task NullChecks()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await GetContract();

            client = null;
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.GetBalance());

            contract = null;
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.GetBalance());
        }

        [Fact]
        public async Task GetBalance()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await GetContract();
            var balance = await contract.GetBalance();
            Assert.True(balance >= 0);
        }
    }
}
