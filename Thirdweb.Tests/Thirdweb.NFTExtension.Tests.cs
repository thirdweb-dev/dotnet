using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Thirdweb;
using Xunit;
using Xunit.Abstractions;

namespace Thirdweb.Tests
{
    public class NFTExtensionTests : BaseTests
    {
        private readonly string _erc721ContractAddress = "0xD811CB13169C175b64bf8897e2Fd6a69C6343f5C";
        private readonly string _erc1155ContractAddress = "0x6A7a26c9a595E6893C255C9dF0b593e77518e0c3";
        private readonly BigInteger _chainId = 421614;

        public NFTExtensionTests(ITestOutputHelper output)
            : base(output) { }

        private async Task<ThirdwebContract> Get721Contract()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc721ContractAddress, _chainId);
            return contract;
        }

        private async Task<ThirdwebContract> Get1155Contract()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _erc1155ContractAddress, _chainId);
            return contract;
        }

        [Fact]
        public async Task NullChecks()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract721 = await Get721Contract();
            var contract1155 = await Get1155Contract();

            // ERC721 Null Checks
            contract721 = null;
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract721.ERC721_GetNFT(0));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract721.ERC721_GetAllNFTs());
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract721.ERC721_GetOwnedNFTs("owner"));

            // ERC1155 Null Checks
            contract1155 = null;
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract1155.ERC1155_GetNFT(0));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract1155.ERC1155_GetAllNFTs());
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract1155.ERC1155_GetOwnedNFTs("owner"));
        }

        [Fact]
        public async Task GetNFTBytes_721()
        {
            var contract = await Get721Contract();
            var nft = await contract.ERC721_GetNFT(0);
            var bytes = await nft.GetNFTImageBytes(contract.Client);
            Assert.NotNull(bytes);
            Assert.NotEmpty(bytes);
        }

        [Fact]
        public async Task GetNFT_721()
        {
            var contract = await Get721Contract();
            var nft = await contract.ERC721_GetNFT(0);
            Assert.NotNull(nft.Owner);
            Assert.NotEmpty(nft.Owner);
            Assert.Equal(NFTType.ERC721, nft.Type);
            Assert.True(nft.Supply == -1 || nft.Supply > 0);
            Assert.True(nft.QuantityOwned == -1);
        }

        [Fact]
        public async Task GetAllNFTs_721()
        {
            var contract = await Get721Contract();
            var nfts = await contract.ERC721_GetAllNFTs();
            Assert.NotNull(nfts);
            Assert.NotEmpty(nfts);
        }

        [Fact]
        public async Task GetOwnedNFTs_721()
        {
            var contract = await Get721Contract();
            var ownerAddress = contract.Address;
            var nfts = await contract.ERC721_GetOwnedNFTs(ownerAddress);
            Assert.NotNull(nfts);
        }

        [Fact]
        public async Task GetNFT_1155()
        {
            var contract = await Get1155Contract();
            var nft = await contract.ERC1155_GetNFT(0);
            Assert.Equal(NFTType.ERC1155, nft.Type);
            Assert.True(nft.Supply >= 0);
        }

        [Fact]
        public async Task GetAllNFTs_1155()
        {
            var contract = await Get1155Contract();
            var nfts = await contract.ERC1155_GetAllNFTs();
            Assert.NotNull(nfts);
            Assert.NotEmpty(nfts);
        }

        [Fact]
        public async Task GetOwnedNFTs_1155()
        {
            var contract = await Get1155Contract();
            var ownerAddress = contract.Address;
            var nfts = await contract.ERC1155_GetOwnedNFTs(ownerAddress);
            Assert.NotNull(nfts);
        }
    }
}
