using System.Numerics;

namespace Thirdweb.Tests
{
    public class WalletExtensionTests : BaseTests
    {
        private readonly BigInteger _chainId = 421614;

        public WalletExtensionTests(ITestOutputHelper output)
            : base(output) { }

        private async Task<IThirdwebWallet> GetWallet()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var privateKeyWallet = await PrivateKeyWallet.Create(client, _testPrivateKey);
            return privateKeyWallet;
        }

        [Fact]
        public async Task NullChecks()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var wallet = await GetWallet();
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await wallet.GetBalance(null, _chainId));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await wallet.GetBalance(client, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await wallet.GetBalance(client, -1));

            client = null;
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await wallet.GetBalance(client, _chainId));

            wallet = null;
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await wallet.GetBalance(client, _chainId));
        }

        [Fact]
        public async Task GetBalance()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var wallet = await GetWallet();
            var balance = await wallet.GetBalance(client, _chainId);
            Assert.True(balance >= 0);
        }
    }
}
