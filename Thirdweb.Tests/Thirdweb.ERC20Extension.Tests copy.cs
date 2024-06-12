using System.Numerics;

namespace Thirdweb.Tests
{
    public class ERC20ExtensionTests : BaseTests
    {
        private readonly string _erc20ContractAddress = "0x81ebd23aA79bCcF5AaFb9c9c5B0Db4223c39102e";
        private readonly BigInteger _chainId = 421614;

        public ERC20ExtensionTests(ITestOutputHelper output)
            : base(output) { }

        private async Task<IThirdwebWallet> GetWallet()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var privateKeyWallet = await PrivateKeyWallet.Create(client, _testPrivateKey);
            var smartAccount = await SmartWallet.Create(client, personalWallet: privateKeyWallet, factoryAddress: "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052", gasless: true, chainId: 421614);
            return smartAccount;
        }

        [Fact]
        public async Task NullChecks()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc20ContractAddress, _chainId);
            var wallet = await GetWallet();

            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_BalanceOf(null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_BalanceOf(string.Empty));

            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_Allowance(null, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_Allowance(string.Empty, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_Allowance(null, string.Empty));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_Allowance(Constants.ADDRESS_ZERO, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_Allowance(null, Constants.ADDRESS_ZERO));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_Approve(null, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_Approve(wallet, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_Approve(wallet, string.Empty, BigInteger.Zero));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_Transfer(null, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_Transfer(wallet, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_Transfer(wallet, string.Empty, BigInteger.Zero));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_TransferFrom(null, null, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_TransferFrom(wallet, null, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_TransferFrom(wallet, string.Empty, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_TransferFrom(wallet, Constants.ADDRESS_ZERO, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_TransferFrom(wallet, Constants.ADDRESS_ZERO, string.Empty, BigInteger.Zero));

            contract = null;

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_BalanceOf(Constants.ADDRESS_ZERO));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_TotalSupply());

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_Decimals());

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_Symbol());

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_Name());

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_Allowance(null, null));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_Approve(wallet, Constants.ADDRESS_ZERO, BigInteger.Zero));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_Transfer(wallet, Constants.ADDRESS_ZERO, BigInteger.Zero));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_TransferFrom(wallet, Constants.ADDRESS_ZERO, Constants.ADDRESS_ZERO, BigInteger.Zero));
        }

        [Fact]
        public async Task ERC20_BalanceOf()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc20ContractAddress, _chainId);
            var ownerAddress = Constants.ADDRESS_ZERO;

            var balance = await contract.ERC20_BalanceOf(ownerAddress);

            Assert.True(balance >= 0);
        }

        [Fact]
        public async Task ERC20_TotalSupply()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc20ContractAddress, _chainId);

            var totalSupply = await contract.ERC20_TotalSupply();

            Assert.True(totalSupply > 0);
        }

        [Fact]
        public async Task ERC20_Decimals()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc20ContractAddress, _chainId);

            var decimals = await contract.ERC20_Decimals();

            Assert.InRange(decimals, 0, 18);
        }

        [Fact]
        public async Task ERC20_Symbol()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc20ContractAddress, _chainId);

            var symbol = await contract.ERC20_Symbol();

            Assert.False(string.IsNullOrEmpty(symbol));
        }

        [Fact]
        public async Task ERC20_Name()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc20ContractAddress, _chainId);

            var name = await contract.ERC20_Name();

            Assert.False(string.IsNullOrEmpty(name));
        }

        [Fact]
        public async Task ERC20_Allowance()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc20ContractAddress, _chainId);
            var ownerAddress = Constants.ADDRESS_ZERO;
            var spenderAddress = contract.Address;

            var allowance = await contract.ERC20_Allowance(ownerAddress, spenderAddress);

            Assert.True(allowance >= 0);
        }

        [Fact]
        public async Task ERC20_Approve()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc20ContractAddress, _chainId);
            var wallet = await GetWallet();
            var spenderAddress = contract.Address;
            var amount = BigInteger.Parse("1000000000000000000");

            var receipt = await contract.ERC20_Approve(wallet, spenderAddress, amount);

            Assert.True(receipt.TransactionHash.Length == 66);
        }

        [Fact]
        public async Task ERC20_Transfer()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc20ContractAddress, _chainId);
            var wallet = await GetWallet();
            var toAddress = await wallet.GetAddress();
            var amount = BigInteger.Parse("1000000000000000000");

            var receipt = await contract.ERC20_Transfer(wallet, toAddress, amount);

            Assert.True(receipt.TransactionHash.Length == 66);
        }

        [Fact]
        public async Task ERC20_TransferFrom()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc20ContractAddress, _chainId);
            var wallet = await GetWallet();

            // SW Approval
            _ = await contract.ERC20_Approve(wallet, await wallet.GetAddress(), BigInteger.Parse("1000000000000000000"));

            var fromAddress = await wallet.GetAddress();
            var toAddress = await wallet.GetAddress();
            var amount = BigInteger.Parse("1000000000000000000");

            var receipt = await contract.ERC20_TransferFrom(wallet, fromAddress, toAddress, amount);

            Assert.True(receipt.TransactionHash.Length == 66);
        }
    }
}
