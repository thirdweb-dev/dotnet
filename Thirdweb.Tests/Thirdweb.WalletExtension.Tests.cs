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
            var privateKeyWallet = await PrivateKeyWallet.Generate(client);
            var smartAccount = await SmartWallet.Create(client, personalWallet: privateKeyWallet, factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052", gasless: true, chainId: 421614);
            return smartAccount;
        }

        [Fact]
        public async Task NullChecks()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var wallet = await GetWallet();

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await wallet.GetBalance(null, _chainId));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await wallet.GetBalance(client, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await wallet.GetBalance(client, -1));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await wallet.Transfer(null, BigInteger.Zero, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await wallet.Transfer(client, BigInteger.Zero, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await wallet.Transfer(client, -1, string.Empty, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await wallet.Transfer(client, _chainId, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await wallet.Transfer(client, _chainId, string.Empty, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await wallet.Transfer(client, _chainId, Constants.ADDRESS_ZERO, -1));

            client = null;

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await wallet.GetBalance(client, _chainId));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await wallet.Transfer(client, BigInteger.Zero, null, BigInteger.Zero));

            wallet = null;
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await wallet.GetBalance(client, _chainId));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await wallet.Transfer(client, BigInteger.Zero, null, BigInteger.Zero));
        }

        [Fact]
        public async Task GetBalance()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var wallet = await GetWallet();
            var balance = await wallet.GetBalance(client, _chainId);
            Assert.True(balance >= 0);
        }

        [Fact]
        public async Task GetBalance_WithERC20()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var wallet = await GetWallet();
            var balance = await wallet.GetBalance(client, _chainId, "0x81ebd23aA79bCcF5AaFb9c9c5B0Db4223c39102e");
            Assert.True(balance >= 0);
        }

        [Fact]
        public async Task Transfer()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var wallet = await GetWallet();
            var toAddress = await wallet.GetAddress();
            var receipt = await wallet.Transfer(client, _chainId, toAddress, BigInteger.Zero);
            Assert.NotNull(receipt);
            Assert.True(receipt.TransactionHash.Length == 66);
        }
    }
}
