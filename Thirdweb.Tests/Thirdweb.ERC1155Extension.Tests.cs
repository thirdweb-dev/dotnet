using System;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Thirdweb.Tests
{
    public class ERC1155ExtensionTests : BaseTests
    {
        private readonly string _erc1155ContractAddress = "0x83b5851134DAA0E28d855E7fBbdB6B412b46d26B";
        private readonly BigInteger _chainId = 421614;

        public ERC1155ExtensionTests(ITestOutputHelper output)
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
            var contract = await ThirdwebContract.Create(client, _erc1155ContractAddress, _chainId);
            var wallet = await GetWallet();

            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_BalanceOf(null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_BalanceOf(string.Empty, BigInteger.Zero));

            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_BalanceOfBatch(null, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_BalanceOfBatch(new string[] { }, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_BalanceOfBatch(null, new BigInteger[] { }));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC1155_SetApprovalForAll(null, null, false));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_SetApprovalForAll(wallet, null, false));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_SetApprovalForAll(wallet, string.Empty, false));

            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_IsApprovedForAll(null, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_IsApprovedForAll(string.Empty, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_IsApprovedForAll(Constants.ADDRESS_ZERO, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_IsApprovedForAll(Constants.ADDRESS_ZERO, string.Empty));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC1155_SafeTransferFrom(null, null, null, BigInteger.Zero, BigInteger.Zero, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_SafeTransferFrom(wallet, null, null, BigInteger.Zero, BigInteger.Zero, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_SafeTransferFrom(wallet, string.Empty, null, BigInteger.Zero, BigInteger.Zero, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_SafeTransferFrom(wallet, Constants.ADDRESS_ZERO, null, BigInteger.Zero, BigInteger.Zero, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_SafeTransferFrom(wallet, Constants.ADDRESS_ZERO, string.Empty, BigInteger.Zero, BigInteger.Zero, null));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC1155_SafeBatchTransferFrom(null, null, null, null, null, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_SafeBatchTransferFrom(wallet, null, null, new BigInteger[] { }, new BigInteger[] { }, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_SafeBatchTransferFrom(wallet, string.Empty, null, new BigInteger[] { }, new BigInteger[] { }, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(
                async () => await contract.ERC1155_SafeBatchTransferFrom(wallet, Constants.ADDRESS_ZERO, null, new BigInteger[] { }, new BigInteger[] { }, null)
            );
            _ = await Assert.ThrowsAsync<ArgumentException>(
                async () => await contract.ERC1155_SafeBatchTransferFrom(wallet, Constants.ADDRESS_ZERO, string.Empty, new BigInteger[] { }, new BigInteger[] { }, null)
            );
            _ = await Assert.ThrowsAsync<ArgumentException>(
                async () => await contract.ERC1155_SafeBatchTransferFrom(wallet, Constants.ADDRESS_ZERO, Constants.ADDRESS_ZERO, null, new BigInteger[] { }, null)
            );
            _ = await Assert.ThrowsAsync<ArgumentException>(
                async () => await contract.ERC1155_SafeBatchTransferFrom(wallet, Constants.ADDRESS_ZERO, Constants.ADDRESS_ZERO, new BigInteger[] { }, null, null)
            );

            contract = null;

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC1155_BalanceOf(Constants.ADDRESS_ZERO, BigInteger.Zero));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC1155_URI(BigInteger.Zero));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC1155_BalanceOfBatch(new string[] { }, new BigInteger[] { }));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC1155_SetApprovalForAll(wallet, Constants.ADDRESS_ZERO, false));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC1155_IsApprovedForAll(null, null));

            _ = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await contract.ERC1155_SafeTransferFrom(wallet, Constants.ADDRESS_ZERO, Constants.ADDRESS_ZERO, BigInteger.Zero, BigInteger.Zero, null)
            );

            _ = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await contract.ERC1155_SafeBatchTransferFrom(wallet, Constants.ADDRESS_ZERO, Constants.ADDRESS_ZERO, new BigInteger[] { }, new BigInteger[] { }, null)
            );
        }

        [Fact]
        public async Task ERC1155_BalanceOf()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc1155ContractAddress, _chainId);
            var wallet = await GetWallet();
            var ownerAddress = await wallet.GetAddress();
            var tokenId = BigInteger.Parse("1");

            var balance = await contract.ERC1155_BalanceOf(ownerAddress, tokenId);

            Assert.True(balance >= 0);
        }

        [Fact]
        public async Task ERC1155_BalanceOfBatch()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc1155ContractAddress, _chainId);
            var wallet = await GetWallet();
            var ownerAddresses = new string[] { await wallet.GetAddress(), await wallet.GetAddress() };
            var tokenIds = new BigInteger[] { BigInteger.Parse("1"), BigInteger.Parse("2") };

            var balances = await contract.ERC1155_BalanceOfBatch(ownerAddresses, tokenIds);

            Assert.True(balances.Count == ownerAddresses.Length);
        }

        [Fact]
        public async Task ERC1155_IsApprovedForAll()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc1155ContractAddress, _chainId);
            var ownerAddress = Constants.ADDRESS_ZERO;
            var operatorAddress = contract.Address;

            var isApproved = await contract.ERC1155_IsApprovedForAll(ownerAddress, operatorAddress);

            Assert.True(isApproved || !isApproved);
        }

        [Fact]
        public async Task ERC1155_URI()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc1155ContractAddress, _chainId);
            var tokenId = BigInteger.Parse("1");

            var uri = await contract.ERC1155_URI(tokenId);

            Assert.False(string.IsNullOrEmpty(uri));
        }

        [Fact]
        public async Task ERC1155_SetApprovalForAll()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc1155ContractAddress, _chainId);
            var wallet = await GetWallet();
            var operatorAddress = contract.Address;
            var approved = true;

            var receipt = await contract.ERC1155_SetApprovalForAll(wallet, operatorAddress, approved);

            Assert.True(receipt.TransactionHash.Length == 66);
        }

        // [Fact]
        // public async Task ERC1155_SafeTransferFrom()
        // {
        //     var client = ThirdwebClient.Create(secretKey: _secretKey);
        //     var contract = await ThirdwebContract.Create(client, _erc1155ContractAddress, _chainId);
        //     var wallet = await GetWallet();
        //     var fromAddress = await wallet.GetAddress();
        //     var toAddress = contract.Address;
        //     var tokenId = BigInteger.Parse("1");
        //     var amount = BigInteger.Parse("1");
        //     var data = new byte[] { };

        //     var receipt = await contract.ERC1155_SafeTransferFrom(wallet, fromAddress, toAddress, tokenId, amount, data);

        //     Assert.True(receipt.TransactionHash.Length == 66);
        // }

        // [Fact]
        // public async Task ERC1155_SafeBatchTransferFrom()
        // {
        //     var client = ThirdwebClient.Create(secretKey: _secretKey);
        //     var contract = await ThirdwebContract.Create(client, _erc1155ContractAddress, _chainId);
        //     var wallet = await GetWallet();
        //     var fromAddress = await wallet.GetAddress();
        //     var toAddress = contract.Address;
        //     var tokenIds = new BigInteger[] { BigInteger.Parse("1"), BigInteger.Parse("2") };
        //     var amounts = new BigInteger[] { BigInteger.Parse("1"), BigInteger.Parse("1") };
        //     var data = new byte[] { };

        //     var receipt = await contract.ERC1155_SafeBatchTransferFrom(wallet, fromAddress, toAddress, tokenIds, amounts, data);

        //     Assert.True(receipt.TransactionHash.Length == 66);
        // }
    }
}
