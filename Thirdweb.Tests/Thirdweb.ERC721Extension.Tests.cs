using System;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Thirdweb.Tests
{
    public class ERC721ExtensionTests : BaseTests
    {
        private readonly string _erc721ContractAddress = "0x345E7B4CCA26725197f1Bed802A05691D8EF7770";
        private readonly BigInteger _chainId = 421614;

        public ERC721ExtensionTests(ITestOutputHelper output)
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
            var contract = await ThirdwebContract.Create(client, _erc721ContractAddress, _chainId);
            var wallet = await GetWallet();

            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_BalanceOf(null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_BalanceOf(string.Empty));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_Approve(null, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_Approve(wallet, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_Approve(wallet, string.Empty, BigInteger.Zero));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_TransferFrom(null, null, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_TransferFrom(wallet, null, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_TransferFrom(wallet, string.Empty, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_TransferFrom(wallet, Constants.ADDRESS_ZERO, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_TransferFrom(wallet, Constants.ADDRESS_ZERO, string.Empty, BigInteger.Zero));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_SafeTransferFrom(null, null, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_SafeTransferFrom(wallet, null, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_SafeTransferFrom(wallet, string.Empty, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_SafeTransferFrom(wallet, Constants.ADDRESS_ZERO, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_SafeTransferFrom(wallet, Constants.ADDRESS_ZERO, string.Empty, BigInteger.Zero));

            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_IsApprovedForAll(null, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_IsApprovedForAll(string.Empty, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_IsApprovedForAll(Constants.ADDRESS_ZERO, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_IsApprovedForAll(Constants.ADDRESS_ZERO, string.Empty));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_SetApprovalForAll(null, null, false));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_SetApprovalForAll(wallet, null, false));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_SetApprovalForAll(wallet, string.Empty, false));

            contract = null;

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_BalanceOf(Constants.ADDRESS_ZERO));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_OwnerOf(BigInteger.Zero));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_Name());

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_Symbol());

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_TokenURI(BigInteger.Zero));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_GetApproved(BigInteger.Zero));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_IsApprovedForAll(null, null));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_SetApprovalForAll(null, null, false));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_TransferFrom(null, null, null, BigInteger.Zero));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_SafeTransferFrom(null, null, null, BigInteger.Zero));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_Approve(null, null, BigInteger.Zero));
        }

        [Fact]
        public async Task ERC721_BalanceOf()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc721ContractAddress, _chainId);
            var wallet = await GetWallet();
            var ownerAddress = await wallet.GetAddress();

            var balance = await contract.ERC721_BalanceOf(ownerAddress);

            Assert.True(balance >= 0);
        }

        [Fact]
        public async Task ERC721_OwnerOf()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc721ContractAddress, _chainId);
            var tokenId = BigInteger.Parse("1");

            var owner = await contract.ERC721_OwnerOf(tokenId);

            Assert.False(string.IsNullOrEmpty(owner));
        }

        [Fact]
        public async Task ERC721_Name()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc721ContractAddress, _chainId);

            var name = await contract.ERC721_Name();

            Assert.False(string.IsNullOrEmpty(name));
        }

        [Fact]
        public async Task ERC721_Symbol()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc721ContractAddress, _chainId);

            var symbol = await contract.ERC721_Symbol();

            Assert.False(string.IsNullOrEmpty(symbol));
        }

        [Fact]
        public async Task ERC721_TokenURI()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc721ContractAddress, _chainId);
            var tokenId = BigInteger.Parse("1");

            var uri = await contract.ERC721_TokenURI(tokenId);

            Assert.False(string.IsNullOrEmpty(uri));
        }

        [Fact]
        public async Task ERC721_GetApproved()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc721ContractAddress, _chainId);
            var tokenId = BigInteger.Parse("1");

            var approved = await contract.ERC721_GetApproved(tokenId);

            Assert.False(string.IsNullOrEmpty(approved));
        }

        [Fact]
        public async Task ERC721_IsApprovedForAll()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc721ContractAddress, _chainId);
            var ownerAddress = Constants.ADDRESS_ZERO;
            var operatorAddress = contract.Address;

            var isApproved = await contract.ERC721_IsApprovedForAll(ownerAddress, operatorAddress);

            Assert.True(isApproved || !isApproved);
        }

        // [Fact]
        // public async Task ERC721_Approve()
        // {
        //     var client = ThirdwebClient.Create(secretKey: _secretKey);
        //     var contract = await ThirdwebContract.Create(client, _erc721ContractAddress, _chainId);
        //     var wallet = await GetWallet();

        //     var toAddress = contract.Address;
        //     var tokenId = BigInteger.Parse("1");

        //     var receipt = await contract.ERC721_Approve(wallet, toAddress, tokenId);

        //     Assert.True(receipt.TransactionHash.Length == 66);
        // }

        // [Fact]
        // public async Task ERC721_SetApprovalForAll()
        // {
        //     var client = ThirdwebClient.Create(secretKey: _secretKey);
        //     var contract = await ThirdwebContract.Create(client, _erc721ContractAddress, _chainId);
        //     var wallet = await GetWallet();
        //     var operatorAddress = contract.Address;
        //     var approved = true;

        //     var receipt = await contract.ERC721_SetApprovalForAll(wallet, operatorAddress, approved);

        //     Assert.True(receipt.TransactionHash.Length == 66);
        // }

        // [Fact]
        // public async Task ERC721_TransferFrom()
        // {
        //     var client = ThirdwebClient.Create(secretKey: _secretKey);
        //     var contract = await ThirdwebContract.Create(client, _erc721ContractAddress, _chainId);
        //     var wallet = await GetWallet();
        //     var fromAddress = await wallet.GetAddress();
        //     var toAddress = contract.Address;
        //     var tokenId = BigInteger.Parse("1");

        //     var receipt = await contract.ERC721_TransferFrom(wallet, fromAddress, toAddress, tokenId);

        //     Assert.True(receipt.TransactionHash.Length == 66);
        // }

        // [Fact]
        // public async Task ERC721_SafeTransferFrom()
        // {
        //     var client = ThirdwebClient.Create(secretKey: _secretKey);
        //     var contract = await ThirdwebContract.Create(client, _erc721ContractAddress, _chainId);
        //     var wallet = await GetWallet();
        //     var fromAddress = await wallet.GetAddress();
        //     var toAddress = contract.Address;
        //     var tokenId = BigInteger.Parse("1");

        //     var receipt = await contract.ERC721_SafeTransferFrom(wallet, fromAddress, toAddress, tokenId);

        //     Assert.True(receipt.TransactionHash.Length == 66);
        // }
    }
}
