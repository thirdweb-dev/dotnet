using System.Numerics;
using Nethereum.Util;

namespace Thirdweb.Tests
{
    public class ExtensionsTests : BaseTests
    {
        private readonly string _tokenErc20ContractAddress = "0x81ebd23aA79bCcF5AaFb9c9c5B0Db4223c39102e";
        private readonly string _tokenErc721ContractAddress = "0x345E7B4CCA26725197f1Bed802A05691D8EF7770";
        private readonly string _tokenErc1155ContractAddress = "0x83b5851134DAA0E28d855E7fBbdB6B412b46d26B";
        private readonly string _dropErc20ContractAddress = "0xEBB8a39D865465F289fa349A67B3391d8f910da9";
        private readonly string _dropErc721ContractAddress = "0xD811CB13169C175b64bf8897e2Fd6a69C6343f5C";
        private readonly string _dropErc1155ContractAddress = "0x6A7a26c9a595E6893C255C9dF0b593e77518e0c3";

        private readonly BigInteger _chainId = 421614;
        private readonly ThirdwebClient _client;

        public ExtensionsTests(ITestOutputHelper output)
            : base(output)
        {
            _client = ThirdwebClient.Create(secretKey: _secretKey);
        }

        private async Task<IThirdwebWallet> GetSmartWallet()
        {
            var privateKeyWallet = await PrivateKeyWallet.Generate(_client);
            return await SmartWallet.Create(personalWallet: privateKeyWallet, chainId: 421614);
        }

        private async Task<ThirdwebContract> GetTokenERC20Contract()
        {
            return await ThirdwebContract.Create(_client, _tokenErc20ContractAddress, _chainId);
        }

        private async Task<ThirdwebContract> GetTokenERC721Contract()
        {
            return await ThirdwebContract.Create(_client, _tokenErc721ContractAddress, _chainId);
        }

        private async Task<ThirdwebContract> GetTokenERC1155Contract()
        {
            return await ThirdwebContract.Create(_client, _tokenErc1155ContractAddress, _chainId);
        }

        private async Task<ThirdwebContract> GetDrop20Contract()
        {
            return await ThirdwebContract.Create(_client, _dropErc20ContractAddress, _chainId);
        }

        private async Task<ThirdwebContract> GetDrop721Contract()
        {
            return await ThirdwebContract.Create(_client, _dropErc721ContractAddress, _chainId);
        }

        private async Task<ThirdwebContract> GetDrop1155Contract()
        {
            return await ThirdwebContract.Create(_client, _dropErc1155ContractAddress, _chainId);
        }

        #region Common

        [Fact]
        public async Task NullChecks()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _tokenErc20ContractAddress, _chainId);
            var wallet = await GetSmartWallet();
            var testNFT = new NFT { Metadata = new NFTMetadata { Image = "image_url" } };
            var validAddress = "0x0000000000000000000000000000000000000000";

            // GetMetadata
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => ThirdwebExtensions.GetMetadata(null));

            // GetNFTImageBytes
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => ThirdwebExtensions.GetNFTImageBytes(testNFT, null));

            // GetDefaultRoyaltyInfo
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => ThirdwebExtensions.GetDefaultRoyaltyInfo(null));

            // GetPrimarySaleRecipient
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => ThirdwebExtensions.GetPrimarySaleRecipient(null));

            // GetBalanceRaw
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => ThirdwebExtensions.GetBalanceRaw(null, _chainId, validAddress));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ThirdwebExtensions.GetBalanceRaw(client, 0, validAddress));
            _ = await Assert.ThrowsAsync<ArgumentException>(() => ThirdwebExtensions.GetBalanceRaw(client, _chainId, null));

            // GetBalance (contract)
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => ThirdwebExtensions.GetBalance(null));

            // GetBalance (wallet)
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => ThirdwebExtensions.GetBalance(null, _chainId));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ThirdwebExtensions.GetBalance(wallet, 0));

            // Transfer
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => ThirdwebExtensions.Transfer(null, _chainId, validAddress, 0));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ThirdwebExtensions.Transfer(wallet, 0, validAddress, 0));
            _ = await Assert.ThrowsAsync<ArgumentException>(() => ThirdwebExtensions.Transfer(wallet, _chainId, null, 0));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ThirdwebExtensions.Transfer(wallet, _chainId, validAddress, -1));
        }

        [Fact]
        public async Task GetMetadata()
        {
            var contract = await GetTokenERC20Contract();
            var metadata = await contract.GetMetadata();
            Assert.NotNull(metadata);
            Assert.NotNull(metadata.Name);
            Assert.NotEmpty(metadata.Name);
            Assert.NotNull(metadata.Symbol);
            Assert.NotEmpty(metadata.Symbol);
            Assert.NotNull(metadata.Description);
            Assert.NotEmpty(metadata.Description);
            Assert.NotNull(metadata.Image);
            Assert.NotEmpty(metadata.Image);
        }

        [Fact]
        public async Task GetNFTBytes_721()
        {
            var contract = await GetDrop721Contract();
            var nft = await contract.ERC721_GetNFT(0);
            var bytes = await nft.GetNFTImageBytes(contract.Client);
            Assert.NotNull(bytes);
            Assert.NotEmpty(bytes);
        }

        [Fact]
        public async Task GetNFTBytes_1155()
        {
            var contract = await GetDrop1155Contract();
            var nft = await contract.ERC1155_GetNFT(0);
            var bytes = await nft.GetNFTImageBytes(contract.Client);
            Assert.NotNull(bytes);
            Assert.NotEmpty(bytes);
        }

        [Fact]
        public async Task GetPrimarySaleRecipient()
        {
            var contract = await GetTokenERC20Contract();
            var primarySaleRecipient = await contract.GetPrimarySaleRecipient();
            Assert.NotNull(primarySaleRecipient);
            Assert.NotEmpty(primarySaleRecipient);
        }

        [Fact]
        public async Task GetBalanceRaw()
        {
            var address = "0xd8dA6BF26964aF9D7eEd9e03E53415D37aA96045"; // vitalik.eth
            var chainId = BigInteger.One;
            var balance = await ThirdwebExtensions.GetBalanceRaw(_client, chainId, address);
            Assert.True(balance >= 0);
        }

        [Fact]
        public async Task GetBalanceRaw_WithERC20()
        {
            var address = "0xd8dA6BF26964aF9D7eEd9e03E53415D37aA96045"; // vitalik.eth
            var chainId = _chainId;
            var contractAddress = _tokenErc20ContractAddress;
            var balance = await ThirdwebExtensions.GetBalanceRaw(_client, chainId, address, contractAddress);
            Assert.True(balance >= 0);
        }

        [Fact]
        public async Task GetBalance_Contract()
        {
            var contract = await GetTokenERC20Contract();
            var balance = await contract.GetBalance();
            Assert.True(balance >= 0);
        }

        [Fact]
        public async Task GetBalance_Contract_WithERC20()
        {
            var contract = await GetTokenERC20Contract();
            var balance = await contract.GetBalance(_tokenErc20ContractAddress);
            Assert.True(balance >= 0);
        }

        [Fact]
        public async Task GetBalance_Wallet()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var wallet = await GetSmartWallet();
            var balance = await wallet.GetBalance(_chainId);
            Assert.True(balance >= 0);
        }

        [Fact]
        public async Task GetBalance_Wallet_WithERC20()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var wallet = await GetSmartWallet();
            var balance = await wallet.GetBalance(_chainId, _tokenErc20ContractAddress);
            Assert.True(balance >= 0);
        }

        [Fact]
        public async Task Transfer()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var wallet = await GetSmartWallet();
            var toAddress = await wallet.GetAddress();
            var receipt = await wallet.Transfer(_chainId, toAddress, BigInteger.Zero);
            Assert.NotNull(receipt);
            Assert.True(receipt.TransactionHash.Length == 66);
        }

        #endregion


        #region ERC20

        [Fact]
        public async Task ERC20_NullChecks()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _tokenErc20ContractAddress, _chainId);
            var wallet = await GetSmartWallet();
            var validAddress = "0x0000000000000000000000000000000000000000";

            // ERC20_BalanceOf
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_BalanceOf(null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_BalanceOf(string.Empty));

            // ERC20_Allowance
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_Allowance(null, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_Allowance(string.Empty, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_Allowance(null, string.Empty));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_Allowance(validAddress, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_Allowance(null, validAddress));

            // ERC20_Approve
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_Approve(null, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_Approve(wallet, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_Approve(wallet, string.Empty, BigInteger.Zero));

            // ERC20_Transfer
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_Transfer(null, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_Transfer(wallet, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_Transfer(wallet, string.Empty, BigInteger.Zero));

            // ERC20_TransferFrom
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_TransferFrom(null, null, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_TransferFrom(wallet, null, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_TransferFrom(wallet, string.Empty, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_TransferFrom(wallet, validAddress, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC20_TransferFrom(wallet, validAddress, string.Empty, BigInteger.Zero));

            // Null wallet checks
            wallet = null;

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_Approve(wallet, validAddress, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_Transfer(wallet, validAddress, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_TransferFrom(wallet, validAddress, validAddress, BigInteger.Zero));

            // Null contract checks
            contract = null;

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_BalanceOf(validAddress));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_TotalSupply());
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_Decimals());
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_Symbol());
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_Name());
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_Allowance(validAddress, validAddress));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_Approve(wallet, validAddress, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_Transfer(wallet, validAddress, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC20_TransferFrom(wallet, validAddress, validAddress, BigInteger.Zero));
        }

        [Fact]
        public async Task ERC20_BalanceOf()
        {
            var contract = await GetTokenERC20Contract();
            var ownerAddress = Constants.ADDRESS_ZERO;
            var balance = await contract.ERC20_BalanceOf(ownerAddress);
            Assert.True(balance >= 0);
        }

        [Fact]
        public async Task ERC20_TotalSupply()
        {
            var contract = await GetTokenERC20Contract();
            var totalSupply = await contract.ERC20_TotalSupply();
            Assert.True(totalSupply > 0);
        }

        [Fact]
        public async Task ERC20_Decimals()
        {
            var contract = await GetTokenERC20Contract();
            var decimals = await contract.ERC20_Decimals();
            Assert.InRange(decimals, 0, 18);
        }

        [Fact]
        public async Task ERC20_Symbol()
        {
            var contract = await GetTokenERC20Contract();
            var symbol = await contract.ERC20_Symbol();
            Assert.False(string.IsNullOrEmpty(symbol));
        }

        [Fact]
        public async Task ERC20_Name()
        {
            var contract = await GetTokenERC20Contract();
            var name = await contract.ERC20_Name();
            Assert.False(string.IsNullOrEmpty(name));
        }

        [Fact]
        public async Task ERC20_Allowance()
        {
            var contract = await GetTokenERC20Contract();
            var ownerAddress = Constants.ADDRESS_ZERO;
            var spenderAddress = contract.Address;
            var allowance = await contract.ERC20_Allowance(ownerAddress, spenderAddress);
            Assert.True(allowance >= 0);
        }

        [Fact]
        public async Task ERC20_Approve()
        {
            var contract = await GetTokenERC20Contract();
            var wallet = await GetSmartWallet();
            var spenderAddress = contract.Address;
            var amount = BigInteger.Parse("1000000000000000000");
            var receipt = await contract.ERC20_Approve(wallet, spenderAddress, amount);
            Assert.True(receipt.TransactionHash.Length == 66);
        }

        #endregion

        #region ERC721

        [Fact]
        public async Task ERC721_NullChecks()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _tokenErc721ContractAddress, _chainId);
            var wallet = await GetSmartWallet();
            var validAddress = "0x0000000000000000000000000000000000000000";

            // ERC721_BalanceOf
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_BalanceOf(null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_BalanceOf(string.Empty));

            // ERC721_OwnerOf
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await contract.ERC721_OwnerOf(BigInteger.MinusOne));

            // ERC721_TokenURI
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await contract.ERC721_TokenURI(BigInteger.MinusOne));

            // ERC721_Approve
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_Approve(null, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_Approve(wallet, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_Approve(wallet, string.Empty, BigInteger.Zero));

            // ERC721_GetApproved
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await contract.ERC721_GetApproved(BigInteger.MinusOne));

            // ERC721_IsApprovedForAll
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_IsApprovedForAll(null, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_IsApprovedForAll(string.Empty, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_IsApprovedForAll(validAddress, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_IsApprovedForAll(validAddress, string.Empty));

            // ERC721_SetApprovalForAll
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_SetApprovalForAll(null, null, false));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_SetApprovalForAll(wallet, null, false));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_SetApprovalForAll(wallet, string.Empty, false));

            // ERC721_TransferFrom
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_TransferFrom(null, null, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_TransferFrom(wallet, null, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_TransferFrom(wallet, string.Empty, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_TransferFrom(wallet, validAddress, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_TransferFrom(wallet, validAddress, string.Empty, BigInteger.Zero));

            // ERC721_SafeTransferFrom
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_SafeTransferFrom(null, null, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_SafeTransferFrom(wallet, null, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_SafeTransferFrom(wallet, string.Empty, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_SafeTransferFrom(wallet, validAddress, null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_SafeTransferFrom(wallet, validAddress, string.Empty, BigInteger.Zero));

            // ERC721_TokenOfOwnerByIndex
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_TokenOfOwnerByIndex(null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC721_TokenOfOwnerByIndex(string.Empty, BigInteger.Zero));

            // Null wallet checks
            wallet = null;

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_Approve(wallet, validAddress, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_SetApprovalForAll(wallet, validAddress, false));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_TransferFrom(wallet, validAddress, validAddress, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_SafeTransferFrom(wallet, validAddress, validAddress, BigInteger.Zero));

            // Null contract checks
            contract = null;

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_BalanceOf(validAddress));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_OwnerOf(BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_Name());
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_Symbol());
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_TokenURI(BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_GetApproved(BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_IsApprovedForAll(validAddress, validAddress));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_SetApprovalForAll(wallet, validAddress, false));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_TransferFrom(wallet, validAddress, validAddress, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_SafeTransferFrom(wallet, validAddress, validAddress, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_Approve(wallet, validAddress, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC721_TokenOfOwnerByIndex(validAddress, BigInteger.Zero));
        }

        [Fact]
        public async Task ERC721_BalanceOf()
        {
            var contract = await GetTokenERC721Contract();
            var wallet = await GetSmartWallet();
            var ownerAddress = await wallet.GetAddress();
            var balance = await contract.ERC721_BalanceOf(ownerAddress);
            Assert.True(balance >= 0);
        }

        [Fact]
        public async Task ERC721_OwnerOf()
        {
            var contract = await GetTokenERC721Contract();
            var tokenId = BigInteger.Parse("1");
            var owner = await contract.ERC721_OwnerOf(tokenId);
            Assert.False(string.IsNullOrEmpty(owner));
        }

        [Fact]
        public async Task ERC721_Name()
        {
            var contract = await GetTokenERC721Contract();
            var name = await contract.ERC721_Name();
            Assert.False(string.IsNullOrEmpty(name));
        }

        [Fact]
        public async Task ERC721_Symbol()
        {
            var contract = await GetTokenERC721Contract();
            var symbol = await contract.ERC721_Symbol();
            Assert.False(string.IsNullOrEmpty(symbol));
        }

        [Fact]
        public async Task ERC721_TokenURI()
        {
            var contract = await GetTokenERC721Contract();
            var tokenId = BigInteger.Parse("1");
            var uri = await contract.ERC721_TokenURI(tokenId);
            Assert.False(string.IsNullOrEmpty(uri));
        }

        [Fact]
        public async Task ERC721_GetApproved()
        {
            var contract = await GetTokenERC721Contract();
            var tokenId = BigInteger.Parse("1");
            var approved = await contract.ERC721_GetApproved(tokenId);
            Assert.False(string.IsNullOrEmpty(approved));
        }

        [Fact]
        public async Task ERC721_IsApprovedForAll()
        {
            var contract = await GetTokenERC721Contract();
            var ownerAddress = Constants.ADDRESS_ZERO;
            var operatorAddress = contract.Address;
            var isApproved = await contract.ERC721_IsApprovedForAll(ownerAddress, operatorAddress);
            Assert.True(isApproved || !isApproved);
        }

        [Fact]
        public async Task ERC721_TotalSupply()
        {
            var contract = await GetTokenERC721Contract();
            var totalSupply = await contract.ERC721_TotalSupply();
            Assert.True(totalSupply >= 0);
        }

        [Fact]
        public async Task ERC721_TokenOfOwnerByIndex()
        {
            var contract = await GetTokenERC721Contract();
            var ownerAddress = "0xE33653ce510Ee767d8824b5EcDeD27125D49889D";
            var index = BigInteger.Zero;
            var tokenId = await contract.ERC721_TokenOfOwnerByIndex(ownerAddress, index);
            Assert.True(tokenId >= 0);
        }

        [Fact]
        public async Task ERC721_SetApprovalForAll()
        {
            var contract = await GetTokenERC721Contract();
            var wallet = await GetSmartWallet();
            var operatorAddress = contract.Address;
            var approved = true;
            var receipt = await contract.ERC721_SetApprovalForAll(wallet, operatorAddress, approved);
            Assert.True(receipt.TransactionHash.Length == 66);
        }

        #endregion

        #region ERC1155

        [Fact]
        public async Task ERC1155_NullChecks()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _tokenErc1155ContractAddress, _chainId);
            var wallet = await GetSmartWallet();
            var validAddress = "0x0000000000000000000000000000000000000000";
            var validTokenId = BigInteger.One;
            var validAmount = BigInteger.One;
            var validData = new byte[] { };

            // ERC1155_BalanceOf
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_BalanceOf(null, validTokenId));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_BalanceOf(string.Empty, validTokenId));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await contract.ERC1155_BalanceOf(validAddress, BigInteger.MinusOne));

            // ERC1155_BalanceOfBatch
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_BalanceOfBatch(null, new BigInteger[] { validTokenId }));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_BalanceOfBatch(new string[] { validAddress }, null));

            // ERC1155_SetApprovalForAll
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC1155_SetApprovalForAll(null, null, false));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_SetApprovalForAll(wallet, null, false));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_SetApprovalForAll(wallet, string.Empty, false));

            // ERC1155_IsApprovedForAll
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_IsApprovedForAll(null, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_IsApprovedForAll(string.Empty, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_IsApprovedForAll(validAddress, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_IsApprovedForAll(validAddress, string.Empty));

            // ERC1155_SafeTransferFrom
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC1155_SafeTransferFrom(null, null, null, validTokenId, validAmount, validData));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_SafeTransferFrom(wallet, null, null, validTokenId, validAmount, validData));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_SafeTransferFrom(wallet, string.Empty, null, validTokenId, validAmount, validData));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_SafeTransferFrom(wallet, validAddress, null, validTokenId, validAmount, validData));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.ERC1155_SafeTransferFrom(wallet, validAddress, string.Empty, validTokenId, validAmount, validData));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await contract.ERC1155_SafeTransferFrom(wallet, validAddress, validAddress, BigInteger.MinusOne, validAmount, validData)
            );

            // ERC1155_SafeBatchTransferFrom
            _ = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await contract.ERC1155_SafeBatchTransferFrom(null, null, null, new BigInteger[] { validTokenId }, new BigInteger[] { validAmount }, validData)
            );
            _ = await Assert.ThrowsAsync<ArgumentException>(
                async () => await contract.ERC1155_SafeBatchTransferFrom(wallet, null, null, new BigInteger[] { validTokenId }, new BigInteger[] { validAmount }, validData)
            );
            _ = await Assert.ThrowsAsync<ArgumentException>(
                async () => await contract.ERC1155_SafeBatchTransferFrom(wallet, string.Empty, null, new BigInteger[] { validTokenId }, new BigInteger[] { validAmount }, validData)
            );
            _ = await Assert.ThrowsAsync<ArgumentException>(
                async () => await contract.ERC1155_SafeBatchTransferFrom(wallet, validAddress, null, new BigInteger[] { validTokenId }, new BigInteger[] { validAmount }, validData)
            );
            _ = await Assert.ThrowsAsync<ArgumentException>(
                async () => await contract.ERC1155_SafeBatchTransferFrom(wallet, validAddress, string.Empty, new BigInteger[] { validTokenId }, new BigInteger[] { validAmount }, validData)
            );
            _ = await Assert.ThrowsAsync<ArgumentException>(
                async () => await contract.ERC1155_SafeBatchTransferFrom(wallet, validAddress, validAddress, null, new BigInteger[] { validAmount }, validData)
            );
            _ = await Assert.ThrowsAsync<ArgumentException>(
                async () => await contract.ERC1155_SafeBatchTransferFrom(wallet, validAddress, validAddress, new BigInteger[] { validTokenId }, null, validData)
            );

            // ERC1155_URI
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await contract.ERC1155_URI(BigInteger.MinusOne));

            // ERC1155_TotalSupply (with tokenId)
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await contract.ERC1155_TotalSupply(BigInteger.MinusOne));

            // Null wallet checks
            wallet = null;

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC1155_SetApprovalForAll(wallet, validAddress, false));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC1155_SafeTransferFrom(wallet, validAddress, validAddress, validTokenId, validAmount, validData));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await contract.ERC1155_SafeBatchTransferFrom(wallet, validAddress, validAddress, new BigInteger[] { validTokenId }, new BigInteger[] { validAmount }, validData)
            );

            // Null contract checks
            contract = null;

            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC1155_BalanceOf(validAddress, validTokenId));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC1155_BalanceOfBatch(new string[] { validAddress }, new BigInteger[] { validTokenId }));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC1155_SetApprovalForAll(wallet, validAddress, false));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC1155_IsApprovedForAll(validAddress, validAddress));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC1155_SafeTransferFrom(wallet, validAddress, validAddress, validTokenId, validAmount, validData));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await contract.ERC1155_SafeBatchTransferFrom(wallet, validAddress, validAddress, new BigInteger[] { validTokenId }, new BigInteger[] { validAmount }, validData)
            );
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC1155_URI(validTokenId));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC1155_TotalSupply(validTokenId));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.ERC1155_TotalSupply());
        }

        [Fact]
        public async Task ERC1155_BalanceOf()
        {
            var contract = await GetTokenERC1155Contract();
            var wallet = await GetSmartWallet();
            var ownerAddress = await wallet.GetAddress();
            var tokenId = BigInteger.Parse("1");
            var balance = await contract.ERC1155_BalanceOf(ownerAddress, tokenId);
            Assert.True(balance >= 0);
        }

        [Fact]
        public async Task ERC1155_BalanceOfBatch()
        {
            var contract = await GetTokenERC1155Contract();
            var wallet = await GetSmartWallet();
            var ownerAddresses = new string[] { await wallet.GetAddress(), await wallet.GetAddress() };
            var tokenIds = new BigInteger[] { BigInteger.Parse("1"), BigInteger.Parse("2") };
            var balances = await contract.ERC1155_BalanceOfBatch(ownerAddresses, tokenIds);
            Assert.True(balances.Count == ownerAddresses.Length);
        }

        [Fact]
        public async Task ERC1155_IsApprovedForAll()
        {
            var contract = await GetTokenERC1155Contract();
            var ownerAddress = Constants.ADDRESS_ZERO;
            var operatorAddress = contract.Address;
            var isApproved = await contract.ERC1155_IsApprovedForAll(ownerAddress, operatorAddress);
            Assert.True(isApproved || !isApproved);
        }

        [Fact]
        public async Task ERC1155_URI()
        {
            var contract = await GetTokenERC1155Contract();
            var tokenId = BigInteger.Parse("1");
            var uri = await contract.ERC1155_URI(tokenId);
            Assert.False(string.IsNullOrEmpty(uri));
        }

        [Fact]
        public async Task ERC1155_SetApprovalForAll()
        {
            var contract = await GetTokenERC1155Contract();
            var wallet = await GetSmartWallet();
            var operatorAddress = contract.Address;
            var approved = true;
            var receipt = await contract.ERC1155_SetApprovalForAll(wallet, operatorAddress, approved);
            Assert.True(receipt.TransactionHash.Length == 66);
        }

        [Fact]
        public async Task ERC1155_TotalSupply()
        {
            var contract = await GetTokenERC1155Contract();
            var totalSupply = await contract.ERC1155_TotalSupply();
            Assert.True(totalSupply >= 0);
        }

        [Fact]
        public async Task ERC1155_TotalSupply_WithTokenId()
        {
            var contract = await GetTokenERC1155Contract();
            var tokenId = BigInteger.Parse("1");
            var totalSupply = await contract.ERC1155_TotalSupply(tokenId);
            Assert.True(totalSupply >= 0);
        }

        #endregion

        #region NFT

        [Fact]
        public async Task NFT_NullChecks()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract721 = await GetTokenERC721Contract();
            var contract1155 = await GetTokenERC1155Contract();

            // ERC721 Null Checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.ERC721_GetNFT(null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.ERC721_GetAllNFTs(null));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.ERC721_GetOwnedNFTs(null, "owner"));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract721.ERC721_GetOwnedNFTs(null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract721.ERC721_GetOwnedNFTs(string.Empty));

            // ERC1155 Null Checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.ERC1155_GetNFT(null, BigInteger.Zero));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.ERC1155_GetAllNFTs(null));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.ERC1155_GetOwnedNFTs(null, "owner"));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract1155.ERC1155_GetOwnedNFTs(null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract1155.ERC1155_GetOwnedNFTs(string.Empty));

            // ERC721 Token ID Out of Range Checks
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await contract721.ERC721_GetNFT(BigInteger.MinusOne));

            // ERC1155 Token ID Out of Range Checks
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await contract1155.ERC1155_GetNFT(BigInteger.MinusOne));

            // Null contract checks
            contract721 = null;
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract721.ERC721_GetNFT(0));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract721.ERC721_GetAllNFTs());
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract721.ERC721_GetOwnedNFTs("owner"));

            contract1155 = null;
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract1155.ERC1155_GetNFT(0));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract1155.ERC1155_GetAllNFTs());
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract1155.ERC1155_GetOwnedNFTs("owner"));
        }

        [Fact]
        public async Task GetNFT_721()
        {
            var contract = await GetTokenERC721Contract();
            var nft = await contract.ERC721_GetNFT(0);
            Assert.NotNull(nft.Owner);
            Assert.NotEmpty(nft.Owner);
            Assert.Equal(NFTType.ERC721, nft.Type);
            Assert.True(nft.Supply == 1);
            Assert.Null(nft.QuantityOwned);
        }

        [Fact]
        public async Task GetAllNFTs_721()
        {
            var contract = await GetTokenERC721Contract();
            var nfts = await contract.ERC721_GetAllNFTs();
            Assert.NotNull(nfts);
            Assert.NotEmpty(nfts);
        }

        [Fact]
        public async Task GetAllNFTs_721_WithRange()
        {
            var contract = await GetTokenERC721Contract();
            var nfts = await contract.ERC721_GetAllNFTs(1, 2);
            Assert.NotNull(nfts);
            Assert.NotEmpty(nfts);
            Assert.True(nfts.Count == 1);
        }

        [Fact]
        public async Task GetOwnedNFTs_721()
        {
            var contract = await GetTokenERC721Contract();
            var ownerAddress = contract.Address;
            var nfts = await contract.ERC721_GetOwnedNFTs(ownerAddress);
            Assert.NotNull(nfts);
        }

        [Fact]
        public async Task GetNFT_1155()
        {
            var contract = await GetTokenERC1155Contract();
            var nft = await contract.ERC1155_GetNFT(0);
            Assert.Equal(NFTType.ERC1155, nft.Type);
            Assert.True(nft.Supply >= 0);
        }

        [Fact]
        public async Task GetAllNFTs_1155()
        {
            var contract = await GetTokenERC1155Contract();
            var nfts = await contract.ERC1155_GetAllNFTs();
            Assert.NotNull(nfts);
            Assert.NotEmpty(nfts);
        }

        [Fact]
        public async Task GetAllNFTs_1155_WithRange()
        {
            var contract = await GetTokenERC1155Contract();
            var nfts = await contract.ERC1155_GetAllNFTs(1, 2);
            Assert.NotNull(nfts);
            Assert.NotEmpty(nfts);
            Assert.True(nfts.Count == 1);
        }

        [Fact]
        public async Task GetOwnedNFTs_1155()
        {
            var contract = await GetTokenERC1155Contract();
            var ownerAddress = contract.Address;
            var nfts = await contract.ERC1155_GetOwnedNFTs(ownerAddress);
            Assert.NotNull(nfts);
        }

        #endregion

        #region DropERC20

        [Fact]
        public async Task DropERC20_NullChecks()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _dropErc20ContractAddress, _chainId);
            var wallet = await GetSmartWallet();
            var validAddress = "0x0000000000000000000000000000000000000000";
            var validAmount = "10";
            var validClaimConditionId = BigInteger.One;
            var invalidClaimConditionId = BigInteger.MinusOne;

            // DropERC20_Claim null checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.DropERC20_Claim(null, wallet, validAddress, validAmount));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.DropERC20_Claim(contract, null, validAddress, validAmount));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.DropERC20_Claim(wallet, null, validAmount));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.DropERC20_Claim(wallet, string.Empty, validAmount));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.DropERC20_Claim(wallet, validAddress, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.DropERC20_Claim(wallet, validAddress, string.Empty));

            // DropERC20_GetActiveClaimConditionId null checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.DropERC20_GetActiveClaimConditionId(null));

            // DropERC20_GetClaimConditionById null and out of range checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.DropERC20_GetClaimConditionById(null, validClaimConditionId));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await contract.DropERC20_GetClaimConditionById(invalidClaimConditionId));

            // DropERC20_GetActiveClaimCondition null checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.DropERC20_GetActiveClaimCondition(null));

            // Null contract checks
            contract = null;
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.DropERC20_Claim(wallet, validAddress, validAmount));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.DropERC20_GetActiveClaimConditionId());
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.DropERC20_GetClaimConditionById(validClaimConditionId));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.DropERC20_GetActiveClaimCondition());
        }

        [Fact]
        public async Task DropERC20_Claim()
        {
            var contract = await GetDrop20Contract();
            var wallet = await GetSmartWallet();
            var receiverAddress = await wallet.GetAddress();
            var balanceBefore = await contract.ERC20_BalanceOf(receiverAddress);
            var receipt = await contract.DropERC20_Claim(wallet, receiverAddress, "1.5");
            var balanceAfter = await contract.ERC20_BalanceOf(receiverAddress);
            Assert.NotNull(receipt);
            Assert.True(receipt.TransactionHash.Length == 66);
            Assert.True(balanceAfter == balanceBefore + BigInteger.Parse("1.5".ToWei()));
        }

        [Fact]
        public async Task DropERC20_GetActiveClaimConditionId()
        {
            var contract = await GetDrop20Contract();
            var conditionId = await contract.DropERC20_GetActiveClaimConditionId();
            Assert.True(conditionId >= 0);
        }

        [Fact]
        public async Task DropERC20_GetClaimConditionById()
        {
            var contract = await GetDrop20Contract();
            var conditionId = await contract.DropERC20_GetActiveClaimConditionId();
            var condition = await contract.DropERC20_GetClaimConditionById(conditionId);
            Assert.NotNull(condition);
            Assert.True(condition.Currency.Length == 42);
        }

        [Fact]
        public async Task DropERC20_GetActiveClaimCondition()
        {
            var contract = await GetDrop20Contract();
            var condition = await contract.DropERC20_GetActiveClaimCondition();
            Assert.NotNull(condition);
            Assert.True(condition.Currency.Length == 42);

            // Compare to raw GetClaimConditionById
            var conditionId = await contract.DropERC20_GetActiveClaimConditionId();
            var conditionById = await contract.DropERC20_GetClaimConditionById(conditionId);
            Assert.Equal(condition.Currency, conditionById.Currency);
        }

        #endregion

        #region DropERC721

        [Fact]
        public async Task DropERC721_NullChecks()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _dropErc721ContractAddress, _chainId);
            var wallet = await GetSmartWallet();
            var validAddress = "0x0000000000000000000000000000000000000000";
            var validQuantity = BigInteger.One;
            var invalidQuantity = BigInteger.Zero;
            var validClaimConditionId = BigInteger.One;
            var invalidClaimConditionId = BigInteger.MinusOne;

            // DropERC721_Claim null checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.DropERC721_Claim(null, wallet, validAddress, validQuantity));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.DropERC721_Claim(contract, null, validAddress, validQuantity));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.DropERC721_Claim(wallet, null, validQuantity));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.DropERC721_Claim(wallet, string.Empty, validQuantity));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await contract.DropERC721_Claim(wallet, validAddress, invalidQuantity));

            // DropERC721_GetActiveClaimConditionId null checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.DropERC721_GetActiveClaimConditionId(null));

            // DropERC721_GetClaimConditionById null and out of range checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.DropERC721_GetClaimConditionById(null, validClaimConditionId));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await contract.DropERC721_GetClaimConditionById(invalidClaimConditionId));

            // DropERC721_GetActiveClaimCondition null checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.DropERC721_GetActiveClaimCondition(null));

            // Null contract checks
            contract = null;
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.DropERC721_Claim(wallet, validAddress, validQuantity));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.DropERC721_GetActiveClaimConditionId());
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.DropERC721_GetClaimConditionById(validClaimConditionId));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.DropERC721_GetActiveClaimCondition());
        }

        [Fact]
        public async Task DropERC721_Claim_ShouldThrowTokens()
        {
            var contract = await GetDrop721Contract();
            var wallet = await GetSmartWallet();
            var ex = await Assert.ThrowsAsync<Exception>(async () => await contract.DropERC721_Claim(wallet, await wallet.GetAddress(), 1));
            Assert.Contains("!Tokens", ex.Message);
        }

        [Fact]
        public async Task DropERC721_GetActiveClaimConditionId()
        {
            var contract = await GetDrop721Contract();
            var conditionId = await contract.DropERC721_GetActiveClaimConditionId();
            Assert.True(conditionId >= 0);
        }

        [Fact]
        public async Task DropERC721_GetClaimConditionById()
        {
            var contract = await GetDrop721Contract();
            var conditionId = await contract.DropERC721_GetActiveClaimConditionId();
            var condition = await contract.DropERC721_GetClaimConditionById(conditionId);
            Assert.NotNull(condition);
            Assert.True(condition.Currency.Length == 42);
        }

        [Fact]
        public async Task DropERC721_GetActiveClaimCondition()
        {
            var contract = await GetDrop721Contract();
            var condition = await contract.DropERC721_GetActiveClaimCondition();
            Assert.NotNull(condition);
            Assert.True(condition.Currency.Length == 42);

            // Compare to raw GetClaimConditionById
            var conditionId = await contract.DropERC721_GetActiveClaimConditionId();
            var conditionById = await contract.DropERC721_GetClaimConditionById(conditionId);
            Assert.Equal(condition.Currency, conditionById.Currency);
        }

        #endregion

        #region DropERC1155

        [Fact]
        public async Task DropERC1155_NullChecks()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _dropErc1155ContractAddress, _chainId);
            var wallet = await GetSmartWallet();
            var validAddress = "0x0000000000000000000000000000000000000000";
            var validTokenId = BigInteger.One;
            var invalidTokenId = BigInteger.MinusOne;
            var validQuantity = BigInteger.One;
            var invalidQuantity = BigInteger.Zero;
            var validClaimConditionId = BigInteger.One;
            var invalidClaimConditionId = BigInteger.MinusOne;

            // DropERC1155_Claim null checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.DropERC1155_Claim(null, wallet, validAddress, validTokenId, validQuantity));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.DropERC1155_Claim(contract, null, validAddress, validTokenId, validQuantity));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.DropERC1155_Claim(wallet, null, validTokenId, validQuantity));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await contract.DropERC1155_Claim(wallet, string.Empty, validTokenId, validQuantity));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await contract.DropERC1155_Claim(wallet, validAddress, invalidTokenId, validQuantity));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await contract.DropERC1155_Claim(wallet, validAddress, validTokenId, invalidQuantity));

            // DropERC1155_GetActiveClaimConditionId null and out of range checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.DropERC1155_GetActiveClaimConditionId(null, validTokenId));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await contract.DropERC1155_GetActiveClaimConditionId(invalidTokenId));

            // DropERC1155_GetClaimConditionById null and out of range checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.DropERC1155_GetClaimConditionById(null, validTokenId, validClaimConditionId));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await contract.DropERC1155_GetClaimConditionById(invalidTokenId, validClaimConditionId));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await contract.DropERC1155_GetClaimConditionById(validTokenId, invalidClaimConditionId));

            // DropERC1155_GetActiveClaimCondition null and out of range checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.DropERC1155_GetActiveClaimCondition(null, validTokenId));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await contract.DropERC1155_GetActiveClaimCondition(invalidTokenId));

            // Null contract checks
            contract = null;
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.DropERC1155_Claim(wallet, validAddress, validTokenId, validQuantity));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.DropERC1155_GetActiveClaimConditionId(validTokenId));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.DropERC1155_GetClaimConditionById(validTokenId, validClaimConditionId));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.DropERC1155_GetActiveClaimCondition(validTokenId));
        }

        [Fact]
        public async Task DropERC1155_Claim()
        {
            var contract = await GetDrop1155Contract();
            var wallet = await GetSmartWallet();
            var tokenId = 0;
            var quantity = 10;
            var receiverAddress = await wallet.GetAddress();

            var balanceBefore = await contract.ERC1155_BalanceOf(receiverAddress, tokenId);
            var receipt = await contract.DropERC1155_Claim(wallet, receiverAddress, tokenId, quantity);
            var balanceAfter = await contract.ERC1155_BalanceOf(receiverAddress, tokenId);
            Assert.NotNull(receipt);
            Assert.True(receipt.TransactionHash.Length == 66);
            Assert.True(balanceAfter == balanceBefore + quantity);
        }

        [Fact]
        public async Task DropERC1155_GetActiveClaimConditionId()
        {
            var contract = await GetDrop1155Contract();
            var tokenId = 0;
            var conditionId = await contract.DropERC1155_GetActiveClaimConditionId(tokenId);
            Assert.True(conditionId >= 0);
        }

        [Fact]
        public async Task DropERC1155_GetClaimConditionById()
        {
            var contract = await GetDrop1155Contract();
            var tokenId = 0;
            var conditionId = await contract.DropERC1155_GetActiveClaimConditionId(tokenId);
            var condition = await contract.DropERC1155_GetClaimConditionById(tokenId, conditionId);
            Assert.NotNull(condition);
            Assert.True(condition.Currency.Length == 42);
        }

        [Fact]
        public async Task DropERC1155_GetActiveClaimCondition()
        {
            var contract = await GetDrop1155Contract();
            var tokenId = 0;
            var condition = await contract.DropERC1155_GetActiveClaimCondition(tokenId);
            Assert.NotNull(condition);
            Assert.True(condition.Currency.Length == 42);

            // Compare to raw GetClaimConditionById
            var conditionId = await contract.DropERC1155_GetActiveClaimConditionId(tokenId);
            var conditionById = await contract.DropERC1155_GetClaimConditionById(tokenId, conditionId);
            Assert.Equal(condition.Currency, conditionById.Currency);
        }

        #endregion

        #region TokenERC20

        [Fact]
        public async Task TokenERC20_NullChecks()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _tokenErc20ContractAddress, _chainId);
            var wallet = await GetSmartWallet();
            var validAddress = "0x0000000000000000000000000000000000000000";
            var validAmount = "100";
            var invalidAmount = string.Empty;
            var validSignature = "0x123";
            var invalidSignature = string.Empty;

            var validMintRequest = new TokenERC20_MintRequest
            {
                To = validAddress,
                PrimarySaleRecipient = validAddress,
                Quantity = BigInteger.One,
                Price = BigInteger.One,
                Currency = Constants.NATIVE_TOKEN_ADDRESS,
                ValidityStartTimestamp = 0,
                ValidityEndTimestamp = 0,
                Uid = Guid.NewGuid().ToByteArray().PadTo32Bytes()
            };

            // TokenERC20_MintTo null checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC20_MintTo(null, wallet, validAddress, validAmount));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC20_MintTo(contract, null, validAddress, validAmount));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC20_MintTo(contract, wallet, null, validAmount));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC20_MintTo(contract, wallet, string.Empty, validAmount));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC20_MintTo(contract, wallet, validAddress, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC20_MintTo(contract, wallet, validAddress, string.Empty));

            // TokenERC20_MintWithSignature null checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC20_MintWithSignature(null, wallet, validMintRequest, validSignature));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC20_MintWithSignature(contract, null, validMintRequest, validSignature));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC20_MintWithSignature(contract, wallet, null, validSignature));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC20_MintWithSignature(contract, wallet, validMintRequest, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC20_MintWithSignature(contract, wallet, validMintRequest, string.Empty));

            // TokenERC20_GenerateMintSignature null checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC20_GenerateMintSignature(null, wallet, validMintRequest));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC20_GenerateMintSignature(contract, null, validMintRequest));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC20_GenerateMintSignature(contract, wallet, null));

            // TokenERC20_VerifyMintSignature null checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC20_VerifyMintSignature(null, validMintRequest, validSignature));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC20_VerifyMintSignature(contract, null, validSignature));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC20_VerifyMintSignature(contract, validMintRequest, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC20_VerifyMintSignature(contract, validMintRequest, string.Empty));

            // Null contract checks
            contract = null;
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.TokenERC20_MintTo(wallet, validAddress, validAmount));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.TokenERC20_MintWithSignature(wallet, validMintRequest, validSignature));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.TokenERC20_GenerateMintSignature(wallet, validMintRequest));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.TokenERC20_VerifyMintSignature(validMintRequest, validSignature));
        }

        // TODO: MintTo

        // TODO: MintWithSignature

        [Fact]
        public async Task TokenERC20_GenerateMintSignature_WithVerify()
        {
            var contract = await GetTokenERC20Contract();
            var fakeAuthorizedSigner = await PrivateKeyWallet.Generate(_client);
            var randomReceiver = await PrivateKeyWallet.Generate(_client);
            var mintRequest = new TokenERC20_MintRequest { To = await randomReceiver.GetAddress(), Quantity = BigInteger.Parse("1.5".ToWei()), };

            (var payload, var signature) = await contract.TokenERC20_GenerateMintSignature(fakeAuthorizedSigner, mintRequest);

            // returned payload should be filled with defaults
            Assert.NotNull(payload);
            Assert.NotNull(payload.To);
            Assert.True(payload.To.Length == 42);
            Assert.True(payload.To == await randomReceiver.GetAddress());
            Assert.NotNull(payload.PrimarySaleRecipient);
            Assert.True(payload.PrimarySaleRecipient.Length == 42);
            Assert.True(payload.Quantity != BigInteger.Zero);
            Assert.True(payload.Price >= 0);
            Assert.NotNull(payload.Currency);
            Assert.True(payload.Currency.Length == 42);
            Assert.True(payload.ValidityStartTimestamp >= 0);
            Assert.True(payload.ValidityEndTimestamp >= 0);
            Assert.NotNull(payload.Uid);
            Assert.True(payload.Uid.Length == 32); // bytes32

            // signature should not be valid
            Assert.NotNull(signature);
            Assert.NotEmpty(signature);
            var verifyResult = await contract.TokenERC20_VerifyMintSignature(payload, signature);
            Assert.False(verifyResult.IsValid);
            Assert.Equal(await fakeAuthorizedSigner.GetAddress(), verifyResult.Signer);
        }

        #endregion

        #region TokenERC721

        [Fact]
        public async Task TokenERC721_NullChecks()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _tokenErc721ContractAddress, _chainId);
            var wallet = await GetSmartWallet();
            var validAddress = "0x0000000000000000000000000000000000000000";
            var validTokenId = BigInteger.One;
            var invalidTokenId = BigInteger.MinusOne;
            var validUri = "ipfs://validUri";
            var invalidUri = null as string;
            var validSignature = "0x123";
            var invalidSignature = string.Empty;

            var validMintRequest = new TokenERC721_MintRequest
            {
                To = validAddress,
                PrimarySaleRecipient = validAddress,
                Uri = validUri,
                Price = BigInteger.One,
                Currency = Constants.NATIVE_TOKEN_ADDRESS,
                ValidityStartTimestamp = 0,
                ValidityEndTimestamp = 0,
                Uid = Guid.NewGuid().ToByteArray().PadTo32Bytes()
            };

            // TokenERC721_MintTo (with URI) null checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC721_MintTo(null, wallet, validAddress, validTokenId, validUri));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC721_MintTo(contract, null, validAddress, validTokenId, validUri));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC721_MintTo(contract, wallet, null, validTokenId, validUri));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC721_MintTo(contract, wallet, string.Empty, validTokenId, validUri));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await ThirdwebExtensions.TokenERC721_MintTo(contract, wallet, validAddress, invalidTokenId, validUri));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC721_MintTo(contract, wallet, validAddress, validTokenId, invalidUri));

            // TokenERC721_MintTo (with metadata) null checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC721_MintTo(null, wallet, validAddress, validTokenId, new NFTMetadata()));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC721_MintTo(contract, null, validAddress, validTokenId, new NFTMetadata()));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC721_MintTo(contract, wallet, null, validTokenId, new NFTMetadata()));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC721_MintTo(contract, wallet, string.Empty, validTokenId, new NFTMetadata()));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await ThirdwebExtensions.TokenERC721_MintTo(contract, wallet, validAddress, invalidTokenId, new NFTMetadata()));

            // TokenERC721_MintWithSignature null checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC721_MintWithSignature(null, wallet, validMintRequest, validSignature));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC721_MintWithSignature(contract, null, validMintRequest, validSignature));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC721_MintWithSignature(contract, wallet, null, validSignature));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC721_MintWithSignature(contract, wallet, validMintRequest, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC721_MintWithSignature(contract, wallet, validMintRequest, string.Empty));

            // TokenERC721_GenerateMintSignature null checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC721_GenerateMintSignature(null, wallet, validMintRequest));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC721_GenerateMintSignature(contract, null, validMintRequest));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC721_GenerateMintSignature(contract, wallet, null));

            // TokenERC721_VerifyMintSignature null checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC721_VerifyMintSignature(null, validMintRequest, validSignature));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC721_VerifyMintSignature(contract, null, validSignature));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC721_VerifyMintSignature(contract, validMintRequest, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC721_VerifyMintSignature(contract, validMintRequest, string.Empty));

            // Null contract checks
            contract = null;
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.TokenERC721_MintTo(wallet, validAddress, validTokenId, validUri));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.TokenERC721_MintTo(wallet, validAddress, validTokenId, new NFTMetadata()));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.TokenERC721_MintWithSignature(wallet, validMintRequest, validSignature));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.TokenERC721_GenerateMintSignature(wallet, validMintRequest));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.TokenERC721_VerifyMintSignature(validMintRequest, validSignature));
        }

        // TODO: MintTo

        // TODO: MintWithSignature

        [Fact]
        public async Task TokenERC721_GenerateMintSignature_WithUri_WithVerify()
        {
            var contract = await GetTokenERC721Contract();
            var fakeAuthorizedSigner = await PrivateKeyWallet.Generate(_client);
            var randomReceiver = await PrivateKeyWallet.Generate(_client);
            var mintRequest = new TokenERC721_MintRequest { To = await randomReceiver.GetAddress(), Uri = "", };

            (var payload, var signature) = await contract.TokenERC721_GenerateMintSignature(fakeAuthorizedSigner, mintRequest);

            // returned payload should be filled with defaults
            Assert.NotNull(payload);
            Assert.NotNull(payload.To);
            Assert.True(payload.To.Length == 42);
            Assert.True(payload.To == await randomReceiver.GetAddress());
            Assert.True(payload.RoyaltyRecipient.Length == 42);
            Assert.True(payload.RoyaltyBps >= 0);
            Assert.NotNull(payload.PrimarySaleRecipient);
            Assert.True(payload.PrimarySaleRecipient.Length == 42);
            Assert.True(payload.Price >= 0);
            Assert.NotNull(payload.Currency);
            Assert.True(payload.Currency.Length == 42);
            Assert.True(payload.ValidityStartTimestamp >= 0);
            Assert.True(payload.ValidityEndTimestamp >= 0);
            Assert.NotNull(payload.Uid);
            Assert.True(payload.Uid.Length == 32); // bytes32

            // signature should not be valid
            Assert.NotNull(signature);
            Assert.NotEmpty(signature);
            var verifyResult = await contract.TokenERC721_VerifyMintSignature(payload, signature);
            Assert.False(verifyResult.IsValid);
            Assert.Equal(await fakeAuthorizedSigner.GetAddress(), verifyResult.Signer);
        }

        [Fact]
        public async Task TokenERC721_GenerateMintSignature_WithNFTMetadata_WithVerify()
        {
            var contract = await GetTokenERC721Contract();
            var fakeAuthorizedSigner = await PrivateKeyWallet.Generate(_client);
            var randomReceiver = await PrivateKeyWallet.Generate(_client);
            var mintRequest = new TokenERC721_MintRequest { To = await randomReceiver.GetAddress() };

            (var payload, var signature) = await contract.TokenERC721_GenerateMintSignature(
                fakeAuthorizedSigner,
                mintRequest,
                new NFTMetadata
                {
                    Name = "Test",
                    Description = "Test",
                    Image = "Test",
                    ExternalUrl = "Test",
                    Attributes = new Dictionary<string, string> { { "Test", "Test" } },
                }
            );

            // returned payload should be filled with defaults
            Assert.NotNull(payload);
            Assert.NotNull(payload.To);
            Assert.True(payload.To.Length == 42);
            Assert.True(payload.To == await randomReceiver.GetAddress());
            Assert.True(payload.RoyaltyRecipient.Length == 42);
            Assert.True(payload.RoyaltyBps >= 0);
            Assert.NotNull(payload.PrimarySaleRecipient);
            Assert.True(payload.PrimarySaleRecipient.Length == 42);
            Assert.True(payload.Price >= 0);
            Assert.NotNull(payload.Currency);
            Assert.True(payload.Currency.Length == 42);
            Assert.True(payload.ValidityStartTimestamp >= 0);
            Assert.True(payload.ValidityEndTimestamp >= 0);
            Assert.NotNull(payload.Uid);
            Assert.True(payload.Uid.Length == 32); // bytes32
            Assert.NotNull(payload.Uri);
            Assert.True(payload.Uri.Length > 0);

            // signature should not be valid
            Assert.NotNull(signature);
            Assert.NotEmpty(signature);
            var verifyResult = await contract.TokenERC721_VerifyMintSignature(payload, signature);
            Assert.False(verifyResult.IsValid);
            Assert.Equal(await fakeAuthorizedSigner.GetAddress(), verifyResult.Signer);
        }

        #endregion

        #region TokenERC1155

        [Fact]
        public async Task TokenERC1155_NullChecks()
        {
            var client = ThirdwebClient.Create(secretKey: _secretKey);
            var contract = await ThirdwebContract.Create(client, _tokenErc1155ContractAddress, _chainId);
            var wallet = await GetSmartWallet();
            var validAddress = "0x0000000000000000000000000000000000000000";
            var validTokenId = BigInteger.One;
            var invalidTokenId = BigInteger.MinusOne;
            var validQuantity = BigInteger.One;
            var invalidQuantity = BigInteger.Zero;
            var validUri = "ipfs://validUri";
            var invalidUri = null as string;
            var validSignature = "0x123";
            var invalidSignature = string.Empty;

            var validMintRequest = new TokenERC1155_MintRequest
            {
                To = validAddress,
                PrimarySaleRecipient = validAddress,
                Uri = validUri,
                Quantity = BigInteger.One,
                PricePerToken = BigInteger.One,
                Currency = Constants.NATIVE_TOKEN_ADDRESS,
                ValidityStartTimestamp = 0,
                ValidityEndTimestamp = 0,
                Uid = Guid.NewGuid().ToByteArray().PadTo32Bytes()
            };

            // TokenERC1155_MintTo (with URI) null checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC1155_MintTo(null, wallet, validAddress, validTokenId, validQuantity, validUri));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC1155_MintTo(contract, null, validAddress, validTokenId, validQuantity, validUri));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC1155_MintTo(contract, wallet, null, validTokenId, validQuantity, validUri));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC1155_MintTo(contract, wallet, string.Empty, validTokenId, validQuantity, validUri));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await ThirdwebExtensions.TokenERC1155_MintTo(contract, wallet, validAddress, invalidTokenId, validQuantity, validUri)
            );
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await ThirdwebExtensions.TokenERC1155_MintTo(contract, wallet, validAddress, validTokenId, invalidQuantity, validUri)
            );
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC1155_MintTo(contract, wallet, validAddress, validTokenId, validQuantity, invalidUri));

            // TokenERC1155_MintTo (with metadata) null checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC1155_MintTo(null, wallet, validAddress, validTokenId, validQuantity, new NFTMetadata()));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC1155_MintTo(contract, null, validAddress, validTokenId, validQuantity, new NFTMetadata()));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC1155_MintTo(contract, wallet, null, validTokenId, validQuantity, new NFTMetadata()));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC1155_MintTo(contract, wallet, string.Empty, validTokenId, validQuantity, new NFTMetadata()));
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await ThirdwebExtensions.TokenERC1155_MintTo(contract, wallet, validAddress, invalidTokenId, validQuantity, new NFTMetadata())
            );
            _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await ThirdwebExtensions.TokenERC1155_MintTo(contract, wallet, validAddress, validTokenId, invalidQuantity, new NFTMetadata())
            );

            // TokenERC1155_MintWithSignature null checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC1155_MintWithSignature(null, wallet, validMintRequest, validSignature));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC1155_MintWithSignature(contract, null, validMintRequest, validSignature));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC1155_MintWithSignature(contract, wallet, null, validSignature));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC1155_MintWithSignature(contract, wallet, validMintRequest, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC1155_MintWithSignature(contract, wallet, validMintRequest, string.Empty));

            // TokenERC1155_GenerateMintSignature null checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC1155_GenerateMintSignature(null, wallet, validMintRequest));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC1155_GenerateMintSignature(contract, null, validMintRequest));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC1155_GenerateMintSignature(contract, wallet, null));

            // TokenERC1155_VerifyMintSignature null checks
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC1155_VerifyMintSignature(null, validMintRequest, validSignature));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await ThirdwebExtensions.TokenERC1155_VerifyMintSignature(contract, null, validSignature));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC1155_VerifyMintSignature(contract, validMintRequest, null));
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await ThirdwebExtensions.TokenERC1155_VerifyMintSignature(contract, validMintRequest, string.Empty));

            // Null contract checks
            contract = null;
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.TokenERC1155_MintTo(wallet, validAddress, validTokenId, validQuantity, validUri));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.TokenERC1155_MintTo(wallet, validAddress, validTokenId, validQuantity, new NFTMetadata()));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.TokenERC1155_MintWithSignature(wallet, validMintRequest, validSignature));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.TokenERC1155_GenerateMintSignature(wallet, validMintRequest));
            _ = await Assert.ThrowsAsync<ArgumentNullException>(async () => await contract.TokenERC1155_VerifyMintSignature(validMintRequest, validSignature));
        }

        // TODO: MintTo

        // TODO: MintWithSignature

        [Fact]
        public async Task TokenERC1155_GenerateMintSignature_WithUri_WithVerify()
        {
            var contract = await GetTokenERC1155Contract();
            var fakeAuthorizedSigner = await PrivateKeyWallet.Generate(_client);
            var randomReceiver = await PrivateKeyWallet.Generate(_client);
            var mintRequest = new TokenERC1155_MintRequest { To = await randomReceiver.GetAddress(), Uri = "", };

            (var payload, var signature) = await contract.TokenERC1155_GenerateMintSignature(fakeAuthorizedSigner, mintRequest);

            // returned payload should be filled with defaults
            Assert.NotNull(payload);
            Assert.NotNull(payload.To);
            Assert.True(payload.To.Length == 42);
            Assert.True(payload.To == await randomReceiver.GetAddress());
            Assert.True(payload.RoyaltyRecipient.Length == 42);
            Assert.True(payload.RoyaltyBps >= 0);
            Assert.NotNull(payload.PrimarySaleRecipient);
            Assert.True(payload.PrimarySaleRecipient.Length == 42);
            Assert.True(payload.PricePerToken >= 0);
            Assert.NotNull(payload.Currency);
            Assert.True(payload.Currency.Length == 42);
            Assert.True(payload.ValidityStartTimestamp >= 0);
            Assert.True(payload.ValidityEndTimestamp >= 0);
            Assert.NotNull(payload.Uid);
            Assert.True(payload.Uid.Length == 32); // bytes32
            Assert.NotNull(payload.TokenId);
            Assert.True(payload.TokenId >= 0);

            // signature should not be valid
            Assert.NotNull(signature);
            Assert.NotEmpty(signature);
            var verifyResult = await contract.TokenERC1155_VerifyMintSignature(payload, signature);
            Assert.False(verifyResult.IsValid);
            Assert.Equal(await fakeAuthorizedSigner.GetAddress(), verifyResult.Signer);
        }

        [Fact]
        public async Task TokenERC1155_GenerateMintSignature_WithNFTMetadata_WithVerify()
        {
            var contract = await GetTokenERC1155Contract();
            var fakeAuthorizedSigner = await PrivateKeyWallet.Generate(_client);
            var randomReceiver = await PrivateKeyWallet.Generate(_client);
            var mintRequest = new TokenERC1155_MintRequest { To = await randomReceiver.GetAddress() };

            (var payload, var signature) = await contract.TokenERC1155_GenerateMintSignature(
                fakeAuthorizedSigner,
                mintRequest,
                new NFTMetadata
                {
                    Name = "Test",
                    Description = "Test",
                    Image = "Test",
                    ExternalUrl = "Test",
                    Attributes = new Dictionary<string, string> { { "Test", "Test" } },
                }
            );

            // returned payload should be filled with defaults
            Assert.NotNull(payload);
            Assert.NotNull(payload.To);
            Assert.True(payload.To.Length == 42);
            Assert.True(payload.To == await randomReceiver.GetAddress());
            Assert.True(payload.RoyaltyRecipient.Length == 42);
            Assert.True(payload.RoyaltyBps >= 0);
            Assert.NotNull(payload.PrimarySaleRecipient);
            Assert.True(payload.PrimarySaleRecipient.Length == 42);
            Assert.True(payload.PricePerToken >= 0);
            Assert.NotNull(payload.Currency);
            Assert.True(payload.Currency.Length == 42);
            Assert.True(payload.ValidityStartTimestamp >= 0);
            Assert.True(payload.ValidityEndTimestamp >= 0);
            Assert.NotNull(payload.Uid);
            Assert.True(payload.Uid.Length == 32); // bytes32
            Assert.NotNull(payload.TokenId);
            Assert.True(payload.TokenId >= 0);
            Assert.NotNull(payload.Uri);
            Assert.True(payload.Uri.Length > 0);

            // signature should not be valid
            Assert.NotNull(signature);
            Assert.NotEmpty(signature);
            var verifyResult = await contract.TokenERC1155_VerifyMintSignature(payload, signature);
            Assert.False(verifyResult.IsValid);
            Assert.Equal(await fakeAuthorizedSigner.GetAddress(), verifyResult.Signer);
        }

        #endregion
    }
}
