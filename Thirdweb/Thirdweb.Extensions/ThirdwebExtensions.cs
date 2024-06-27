using System.Numerics;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Thirdweb
{
    public static class ThirdwebExtensions
    {
        #region Common

        public static async Task<ContractMetadata> GetMetadata(this ThirdwebContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            var contractUri = await ThirdwebContract.Read<string>(contract, "contractURI");

            return await ThirdwebStorage.Download<ContractMetadata>(contract.Client, contractUri);
        }

        public static async Task<byte[]> GetNFTImageBytes(this NFT nft, ThirdwebClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            return string.IsNullOrEmpty(nft.Metadata.Image) ? new byte[] { } : await ThirdwebStorage.Download<byte[]>(client, nft.Metadata.Image).ConfigureAwait(false);
        }

        public static async Task<RoyaltyInfoResult> GetDefaultRoyaltyInfo(this ThirdwebContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<RoyaltyInfoResult>(contract, "getDefaultRoyaltyInfo");
        }

        public static async Task<string> GetPrimarySaleRecipient(this ThirdwebContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<string>(contract, "primarySaleRecipient");
        }

        public static async Task<BigInteger> GetBalanceRaw(ThirdwebClient client, BigInteger chainId, string address, string erc20ContractAddress = null)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (chainId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chainId), "Chain ID must be greater than 0.");
            }

            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentException("Address must be provided");
            }

            if (erc20ContractAddress != null)
            {
                var erc20Contract = await ThirdwebContract.Create(client, erc20ContractAddress, chainId).ConfigureAwait(false);
                return await erc20Contract.ERC20_BalanceOf(address).ConfigureAwait(false);
            }

            var rpc = ThirdwebRPC.GetRpcInstance(client, chainId);
            var balanceHex = await rpc.SendRequestAsync<string>("eth_getBalance", address, "latest").ConfigureAwait(false);
            return new HexBigInteger(balanceHex).Value;
        }

        public static async Task<BigInteger> GetBalance(this ThirdwebContract contract, string erc20ContractAddress = null)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await GetBalanceRaw(contract.Client, contract.Chain, contract.Address, erc20ContractAddress).ConfigureAwait(false);
        }

        public static async Task<BigInteger> GetBalance(this IThirdwebWallet wallet, ThirdwebClient client, BigInteger chainId, string erc20ContractAddress = null)
        {
            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (chainId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chainId), "Chain ID must be greater than 0.");
            }

            var address = await wallet.GetAddress().ConfigureAwait(false);

            return await GetBalanceRaw(client, chainId, address, erc20ContractAddress).ConfigureAwait(false);
        }

        public static async Task<ThirdwebTransactionReceipt> Transfer(this IThirdwebWallet wallet, ThirdwebClient client, BigInteger chainId, string toAddress, BigInteger weiAmount)
        {
            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (chainId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chainId), "Chain ID must be greater than 0.");
            }

            if (string.IsNullOrEmpty(toAddress))
            {
                throw new ArgumentException(nameof(toAddress), "Recipient address cannot be null or empty.");
            }

            if (weiAmount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(weiAmount), "Amount must be 0 or greater.");
            }

            var txInput = new ThirdwebTransactionInput()
            {
                From = await wallet.GetAddress().ConfigureAwait(false),
                To = toAddress,
                Value = new HexBigInteger(weiAmount)
            };
            var tx = await ThirdwebTransaction.Create(client, wallet, txInput, chainId).ConfigureAwait(false);
            return await ThirdwebTransaction.SendAndWaitForTransactionReceipt(tx).ConfigureAwait(false);
        }

        #endregion

        #region ERC20

        // Check the balance of a specific address
        public static async Task<BigInteger> ERC20_BalanceOf(this ThirdwebContract contract, string ownerAddress)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (string.IsNullOrEmpty(ownerAddress))
            {
                throw new ArgumentException("Owner address must be provided");
            }

            return await ThirdwebContract.Read<BigInteger>(contract, "balanceOf", ownerAddress);
        }

        // Get the total supply of the token
        public static async Task<BigInteger> ERC20_TotalSupply(this ThirdwebContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<BigInteger>(contract, "totalSupply");
        }

        // Get the number of decimals used by the token
        public static async Task<int> ERC20_Decimals(this ThirdwebContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<int>(contract, "decimals");
        }

        // Get the symbol of the token
        public static async Task<string> ERC20_Symbol(this ThirdwebContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<string>(contract, "symbol");
        }

        // Get the name of the token
        public static async Task<string> ERC20_Name(this ThirdwebContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<string>(contract, "name");
        }

        // Get the allowance of a spender for a specific owner
        public static async Task<BigInteger> ERC20_Allowance(this ThirdwebContract contract, string ownerAddress, string spenderAddress)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (string.IsNullOrEmpty(ownerAddress))
            {
                throw new ArgumentException("Owner address must be provided");
            }

            if (string.IsNullOrEmpty(spenderAddress))
            {
                throw new ArgumentException("Spender address must be provided");
            }

            return await ThirdwebContract.Read<BigInteger>(contract, "allowance", ownerAddress, spenderAddress);
        }

        // Approve a spender to spend a specific amount of tokens
        public static async Task<ThirdwebTransactionReceipt> ERC20_Approve(this ThirdwebContract contract, IThirdwebWallet wallet, string spenderAddress, BigInteger amount)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(spenderAddress))
            {
                throw new ArgumentException("Spender address must be provided");
            }

            return await ThirdwebContract.Write(wallet, contract, "approve", 0, spenderAddress, amount);
        }

        // Transfer tokens to a specific address
        public static async Task<ThirdwebTransactionReceipt> ERC20_Transfer(this ThirdwebContract contract, IThirdwebWallet wallet, string toAddress, BigInteger amount)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(toAddress))
            {
                throw new ArgumentException("Recipient address must be provided");
            }

            return await ThirdwebContract.Write(wallet, contract, "transfer", 0, toAddress, amount);
        }

        // Transfer tokens from one address to another
        public static async Task<ThirdwebTransactionReceipt> ERC20_TransferFrom(this ThirdwebContract contract, IThirdwebWallet wallet, string fromAddress, string toAddress, BigInteger amount)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new ArgumentException("Sender address must be provided");
            }

            if (string.IsNullOrEmpty(toAddress))
            {
                throw new ArgumentException("Recipient address must be provided");
            }

            return await ThirdwebContract.Write(wallet, contract, "transferFrom", 0, fromAddress, toAddress, amount);
        }

        // Total supply of the token
        public static async Task<BigInteger> ERC721_TotalSupply(this ThirdwebContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<BigInteger>(contract, "totalSupply");
        }

        // Get the token ID of a specific owner by index
        public static async Task<BigInteger> ERC721_TokenOfOwnerByIndex(this ThirdwebContract contract, string ownerAddress, BigInteger index)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (string.IsNullOrEmpty(ownerAddress))
            {
                throw new ArgumentException("Owner address must be provided");
            }

            return await ThirdwebContract.Read<BigInteger>(contract, "tokenOfOwnerByIndex", ownerAddress, index);
        }

        // Get the token ID of a specific owner by index
        public static async Task<BigInteger> ERC721_TokenByIndex(this ThirdwebContract contract, BigInteger index)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<BigInteger>(contract, "tokenByIndex", index);
        }

        #endregion

        #region ERC721

        // Check the balance of a specific address
        public static async Task<BigInteger> ERC721_BalanceOf(this ThirdwebContract contract, string ownerAddress)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (string.IsNullOrEmpty(ownerAddress))
            {
                throw new ArgumentException("Owner address must be provided");
            }

            return await ThirdwebContract.Read<BigInteger>(contract, "balanceOf", ownerAddress);
        }

        // Get the owner of a specific token
        public static async Task<string> ERC721_OwnerOf(this ThirdwebContract contract, BigInteger tokenId)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<string>(contract, "ownerOf", tokenId);
        }

        // Get the name of the token
        public static async Task<string> ERC721_Name(this ThirdwebContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<string>(contract, "name");
        }

        // Get the symbol of the token
        public static async Task<string> ERC721_Symbol(this ThirdwebContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<string>(contract, "symbol");
        }

        // Get the URI of a specific token
        public static async Task<string> ERC721_TokenURI(this ThirdwebContract contract, BigInteger tokenId)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<string>(contract, "tokenURI", tokenId);
        }

        // Approve a specific address to transfer a specific token
        public static async Task<ThirdwebTransactionReceipt> ERC721_Approve(this ThirdwebContract contract, IThirdwebWallet wallet, string toAddress, BigInteger tokenId)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(toAddress))
            {
                throw new ArgumentException("Recipient address must be provided");
            }

            return await ThirdwebContract.Write(wallet, contract, "approve", 0, toAddress, tokenId);
        }

        // Get the approved address for a specific token
        public static async Task<string> ERC721_GetApproved(this ThirdwebContract contract, BigInteger tokenId)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<string>(contract, "getApproved", tokenId);
        }

        // Check if an address is an operator for another address
        public static async Task<bool> ERC721_IsApprovedForAll(this ThirdwebContract contract, string ownerAddress, string operatorAddress)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (string.IsNullOrEmpty(ownerAddress))
            {
                throw new ArgumentException("Owner address must be provided");
            }

            if (string.IsNullOrEmpty(operatorAddress))
            {
                throw new ArgumentException("Operator address must be provided");
            }

            return await ThirdwebContract.Read<bool>(contract, "isApprovedForAll", ownerAddress, operatorAddress);
        }

        // Set or unset an operator for an owner
        public static async Task<ThirdwebTransactionReceipt> ERC721_SetApprovalForAll(this ThirdwebContract contract, IThirdwebWallet wallet, string operatorAddress, bool approved)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(operatorAddress))
            {
                throw new ArgumentException("Operator address must be provided");
            }

            return await ThirdwebContract.Write(wallet, contract, "setApprovalForAll", 0, operatorAddress, approved);
        }

        // Transfer a specific token from one address to another
        public static async Task<ThirdwebTransactionReceipt> ERC721_TransferFrom(this ThirdwebContract contract, IThirdwebWallet wallet, string fromAddress, string toAddress, BigInteger tokenId)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new ArgumentException("Sender address must be provided");
            }

            if (string.IsNullOrEmpty(toAddress))
            {
                throw new ArgumentException("Recipient address must be provided");
            }

            return await ThirdwebContract.Write(wallet, contract, "transferFrom", 0, fromAddress, toAddress, tokenId);
        }

        // Safe transfer a specific token from one address to another
        public static async Task<ThirdwebTransactionReceipt> ERC721_SafeTransferFrom(this ThirdwebContract contract, IThirdwebWallet wallet, string fromAddress, string toAddress, BigInteger tokenId)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new ArgumentException("Sender address must be provided");
            }

            if (string.IsNullOrEmpty(toAddress))
            {
                throw new ArgumentException("Recipient address must be provided");
            }

            return await ThirdwebContract.Write(wallet, contract, "safeTransferFrom", 0, fromAddress, toAddress, tokenId);
        }

        #endregion

        #region ERC1155

        // Check the balance of a specific token for a specific address
        public static async Task<BigInteger> ERC1155_BalanceOf(this ThirdwebContract contract, string ownerAddress, BigInteger tokenId)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (string.IsNullOrEmpty(ownerAddress))
            {
                throw new ArgumentException("Owner address must be provided");
            }

            return await ThirdwebContract.Read<BigInteger>(contract, "balanceOf", ownerAddress, tokenId);
        }

        // Check the balance of multiple tokens for multiple addresses
        public static async Task<List<BigInteger>> ERC1155_BalanceOfBatch(this ThirdwebContract contract, string[] ownerAddresses, BigInteger[] tokenIds)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (ownerAddresses == null || tokenIds == null)
            {
                throw new ArgumentException("Owner addresses and token IDs must be provided");
            }

            return await ThirdwebContract.Read<List<BigInteger>>(contract, "balanceOfBatch", ownerAddresses, tokenIds);
        }

        // Approve a specific address to transfer specific tokens
        public static async Task<ThirdwebTransactionReceipt> ERC1155_SetApprovalForAll(this ThirdwebContract contract, IThirdwebWallet wallet, string operatorAddress, bool approved)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(operatorAddress))
            {
                throw new ArgumentException("Operator address must be provided");
            }

            return await ThirdwebContract.Write(wallet, contract, "setApprovalForAll", 0, operatorAddress, approved);
        }

        // Check if an address is approved to transfer specific tokens
        public static async Task<bool> ERC1155_IsApprovedForAll(this ThirdwebContract contract, string ownerAddress, string operatorAddress)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (string.IsNullOrEmpty(ownerAddress))
            {
                throw new ArgumentException("Owner address must be provided");
            }

            if (string.IsNullOrEmpty(operatorAddress))
            {
                throw new ArgumentException("Operator address must be provided");
            }

            return await ThirdwebContract.Read<bool>(contract, "isApprovedForAll", ownerAddress, operatorAddress);
        }

        // Transfer specific tokens from one address to another
        public static async Task<ThirdwebTransactionReceipt> ERC1155_SafeTransferFrom(
            this ThirdwebContract contract,
            IThirdwebWallet wallet,
            string fromAddress,
            string toAddress,
            BigInteger tokenId,
            BigInteger amount,
            byte[] data
        )
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new ArgumentException("Sender address must be provided");
            }

            if (string.IsNullOrEmpty(toAddress))
            {
                throw new ArgumentException("Recipient address must be provided");
            }

            return await ThirdwebContract.Write(wallet, contract, "safeTransferFrom", 0, fromAddress, toAddress, tokenId, amount, data);
        }

        // Transfer multiple tokens from one address to another
        public static async Task<ThirdwebTransactionReceipt> ERC1155_SafeBatchTransferFrom(
            this ThirdwebContract contract,
            IThirdwebWallet wallet,
            string fromAddress,
            string toAddress,
            BigInteger[] tokenIds,
            BigInteger[] amounts,
            byte[] data
        )
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new ArgumentException("Sender address must be provided");
            }

            if (string.IsNullOrEmpty(toAddress))
            {
                throw new ArgumentException("Recipient address must be provided");
            }

            if (tokenIds == null || amounts == null)
            {
                throw new ArgumentException("Token IDs and amounts must be provided");
            }

            return await ThirdwebContract.Write(wallet, contract, "safeBatchTransferFrom", 0, fromAddress, toAddress, tokenIds, amounts, data);
        }

        // Get the URI for a specific token
        public static async Task<string> ERC1155_URI(this ThirdwebContract contract, BigInteger tokenId)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<string>(contract, "uri", tokenId);
        }

        // Total Supply of id
        public static async Task<BigInteger> ERC1155_TotalSupply(this ThirdwebContract contract, BigInteger tokenId)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return await ThirdwebContract.Read<BigInteger>(contract, "totalSupply", tokenId);
        }

        // Total Supply
        public static async Task<BigInteger> ERC1155_TotalSupply(this ThirdwebContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            try
            {
                return await ThirdwebContract.Read<BigInteger>(contract, "nextTokenIdToMint");
            }
            catch (Exception)
            {
                return await ThirdwebContract.Read<BigInteger>(contract, "totalSupply");
            }
        }

        #endregion

        #region NFT

        public static async Task<NFT> ERC721_GetNFT(this ThirdwebContract contract, BigInteger tokenId)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            var uri = await contract.ERC721_TokenURI(tokenId).ConfigureAwait(false);
            var metadata = await ThirdwebStorage.Download<NFTMetadata>(contract.Client, uri).ConfigureAwait(false);
            metadata.Id = tokenId.ToString();

            var owner = Constants.ADDRESS_ZERO;
            try
            {
                owner = await contract.ERC721_OwnerOf(tokenId).ConfigureAwait(false);
            }
            catch (Exception)
            {
                owner = Constants.ADDRESS_ZERO;
            }

            return new NFT
            {
                Metadata = metadata,
                Owner = owner,
                Type = NFTType.ERC721,
                Supply = 1,
            };
        }

        public static async Task<List<NFT>> ERC721_GetAllNFTs(this ThirdwebContract contract, BigInteger? startTokenIdIncluded = null, BigInteger? endTokenIdExcluded = null)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (startTokenIdIncluded == null)
            {
                startTokenIdIncluded = 0;
            }

            if (endTokenIdExcluded == null)
            {
                var totalSupply = await contract.ERC721_TotalSupply().ConfigureAwait(false);
                endTokenIdExcluded = totalSupply;
            }

            var nftTasks = new List<Task<NFT>>();
            for (var i = startTokenIdIncluded.Value; i < endTokenIdExcluded.Value; i++)
            {
                nftTasks.Add(contract.ERC721_GetNFT(i));
            }

            var allNfts = await Task.WhenAll(nftTasks).ConfigureAwait(false);
            return allNfts.ToList();
        }

        public static async Task<List<NFT>> ERC721_GetOwnedNFTs(this ThirdwebContract contract, string owner)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (string.IsNullOrEmpty(owner))
            {
                throw new ArgumentException("Owner must be provided");
            }

            try
            {
                var balanceOfOwner = await contract.ERC721_BalanceOf(owner).ConfigureAwait(false);
                var ownedNftTasks = new List<Task<NFT>>();
                for (var i = 0; i < balanceOfOwner; i++)
                {
                    var tokenOfOwnerByIndex = await contract.ERC721_TokenOfOwnerByIndex(owner, i).ConfigureAwait(false);
                    ownedNftTasks.Add(contract.ERC721_GetNFT(tokenOfOwnerByIndex));
                }
                var ownedNfts = await Task.WhenAll(ownedNftTasks).ConfigureAwait(false);
                return ownedNfts.ToList();
            }
            catch (Exception)
            {
                var allNfts = await contract.ERC721_GetAllNFTs().ConfigureAwait(false);
                var ownedNfts = new List<NFT>();
                foreach (var nft in allNfts)
                {
                    if (nft.Owner == owner)
                    {
                        ownedNfts.Add(nft);
                    }
                }
                return ownedNfts.ToList();
            }
        }

        public static async Task<NFT> ERC1155_GetNFT(this ThirdwebContract contract, BigInteger tokenId)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            var uri = await contract.ERC1155_URI(tokenId).ConfigureAwait(false);
            var metadata = await ThirdwebStorage.Download<NFTMetadata>(contract.Client, uri).ConfigureAwait(false);
            metadata.Id = tokenId.ToString();
            var owner = string.Empty;
            var supply = BigInteger.MinusOne;
            try
            {
                supply = await contract.ERC1155_TotalSupply(tokenId).ConfigureAwait(false);
            }
            catch (Exception)
            {
                supply = BigInteger.MinusOne;
            }

            return new NFT
            {
                Metadata = metadata,
                Owner = owner,
                Type = NFTType.ERC1155,
                Supply = supply,
            };
        }

        public static async Task<List<NFT>> ERC1155_GetAllNFTs(this ThirdwebContract contract, BigInteger? startTokenIdIncluded = null, BigInteger? endTokenIdExcluded = null)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (startTokenIdIncluded == null)
            {
                startTokenIdIncluded = 0;
            }

            if (endTokenIdExcluded == null)
            {
                var totalSupply = await contract.ERC1155_TotalSupply().ConfigureAwait(false);
                endTokenIdExcluded = totalSupply;
            }

            var nftTasks = new List<Task<NFT>>();
            for (var i = startTokenIdIncluded.Value; i < endTokenIdExcluded.Value; i++)
            {
                nftTasks.Add(contract.ERC1155_GetNFT(i));
            }

            var allNfts = await Task.WhenAll(nftTasks).ConfigureAwait(false);
            return allNfts.ToList();
        }

        public static async Task<List<NFT>> ERC1155_GetOwnedNFTs(this ThirdwebContract contract, string owner)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (string.IsNullOrEmpty(owner))
            {
                throw new ArgumentException("Owner must be provided");
            }

            var totalSupply = await contract.ERC1155_TotalSupply().ConfigureAwait(false);

            var nfts = new List<NFT>();
            for (var i = 0; i < totalSupply; i++)
            {
                var balanceOfOwner = await contract.ERC1155_BalanceOf(owner, i).ConfigureAwait(false);
                if (balanceOfOwner > 0)
                {
                    var nft = await contract.ERC1155_GetNFT(i).ConfigureAwait(false);
                    nft.QuantityOwned = balanceOfOwner;
                    nfts.Add(nft);
                }
            }

            return nfts;
        }

        #endregion

        #region DropERC20

        public static async Task<ThirdwebTransactionReceipt> DropERC20_Claim(this ThirdwebContract contract, IThirdwebWallet wallet, string receiverAddress, string amount)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(receiverAddress))
            {
                throw new ArgumentException("Receiver address must be provided");
            }

            if (string.IsNullOrEmpty(amount))
            {
                throw new ArgumentException("Amount must be provided");
            }

            // TODO: Task.WhenAll

            var activeClaimCondition = await contract.DropERC20_GetActiveClaimCondition();

            var erc20Decimals = await contract.ERC20_Decimals();

            var rawAmountToClaim = BigInteger.Parse(amount.ToWei()).AdjustDecimals(18, erc20Decimals);

            var isNativeToken = activeClaimCondition.Currency == Constants.NATIVE_TOKEN_ADDRESS;

            var payableAmount = isNativeToken ? rawAmountToClaim * activeClaimCondition.PricePerToken : BigInteger.Zero;

            // TODO: Merkle
            var allowlistProof = new object[] { new byte[] { }, BigInteger.Zero, BigInteger.Zero, Constants.ADDRESS_ZERO };

            var fnArgs = new object[]
            {
                receiverAddress, // receiver
                rawAmountToClaim, // quantity
                activeClaimCondition.Currency, // currency
                activeClaimCondition.PricePerToken, // pricePerToken
                allowlistProof, // allowlistProof
                new byte[] { } // data
            };

            return await ThirdwebContract.Write(wallet, contract, "claim", payableAmount, fnArgs);
        }

        public static async Task<BigInteger> DropERC20_GetActiveClaimConditionId(this ThirdwebContract contract)
        {
            return await ThirdwebContract.Read<BigInteger>(contract, "getActiveClaimConditionId");
        }

        public static async Task<Drop_ClaimCondition> DropERC20_GetClaimConditionById(this ThirdwebContract contract, BigInteger claimConditionId)
        {
            return await ThirdwebContract.Read<Drop_ClaimCondition>(contract, "getClaimConditionById", claimConditionId);
        }

        public static async Task<Drop_ClaimCondition> DropERC20_GetActiveClaimCondition(this ThirdwebContract contract)
        {
            var activeClaimConditionId = await contract.DropERC20_GetActiveClaimConditionId();
            return await contract.DropERC20_GetClaimConditionById(activeClaimConditionId);
        }

        #endregion

        #region DropERC721

        public static async Task<ThirdwebTransactionReceipt> DropERC721_Claim(this ThirdwebContract contract, IThirdwebWallet wallet, string receiverAddress, BigInteger quantity)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(receiverAddress))
            {
                throw new ArgumentException("Receiver address must be provided");
            }

            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than 0");
            }

            // TODO: Task.WhenAll

            var activeClaimCondition = await contract.DropERC721_GetActiveClaimCondition();

            var isNativeToken = activeClaimCondition.Currency == Constants.NATIVE_TOKEN_ADDRESS;

            var payableAmount = isNativeToken ? quantity * activeClaimCondition.PricePerToken : BigInteger.Zero;

            // TODO: Merkle
            var allowlistProof = new object[] { new byte[] { }, BigInteger.Zero, BigInteger.Zero, Constants.ADDRESS_ZERO };

            var fnArgs = new object[]
            {
                receiverAddress, // receiver
                quantity, // quantity
                activeClaimCondition.Currency, // currency
                activeClaimCondition.PricePerToken, // pricePerToken
                allowlistProof, // allowlistProof
                new byte[] { } // data
            };

            return await ThirdwebContract.Write(wallet, contract, "claim", payableAmount, fnArgs);
        }

        public static async Task<BigInteger> DropERC721_GetActiveClaimConditionId(this ThirdwebContract contract)
        {
            return await ThirdwebContract.Read<BigInteger>(contract, "getActiveClaimConditionId");
        }

        public static async Task<Drop_ClaimCondition> DropERC721_GetClaimConditionById(this ThirdwebContract contract, BigInteger claimConditionId)
        {
            return await ThirdwebContract.Read<Drop_ClaimCondition>(contract, "getClaimConditionById", claimConditionId);
        }

        public static async Task<Drop_ClaimCondition> DropERC721_GetActiveClaimCondition(this ThirdwebContract contract)
        {
            var activeClaimConditionId = await contract.DropERC721_GetActiveClaimConditionId();
            return await contract.DropERC20_GetClaimConditionById(activeClaimConditionId);
        }

        #endregion

        #region DropERC1155

        public static async Task<ThirdwebTransactionReceipt> DropERC1155_Claim(this ThirdwebContract contract, IThirdwebWallet wallet, string receiverAddress, BigInteger tokenId, BigInteger quantity)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(receiverAddress))
            {
                throw new ArgumentException("Receiver address must be provided");
            }

            if (tokenId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tokenId), "Token ID must be equal or greater than 0");
            }

            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than 0");
            }

            // TODO: Task.WhenAll

            var activeClaimCondition = await contract.DropERC1155_GetActiveClaimCondition(tokenId);

            var isNativeToken = activeClaimCondition.Currency == Constants.NATIVE_TOKEN_ADDRESS;

            var payableAmount = isNativeToken ? quantity * activeClaimCondition.PricePerToken : BigInteger.Zero;

            // TODO: Merkle
            var allowlistProof = new object[] { new byte[] { }, BigInteger.Zero, BigInteger.Zero, Constants.ADDRESS_ZERO };

            var fnArgs = new object[]
            {
                receiverAddress, // receiver
                tokenId, // tokenId
                quantity, // quantity
                activeClaimCondition.Currency, // currency
                activeClaimCondition.PricePerToken, // pricePerToken
                allowlistProof, // allowlistProof
                new byte[] { } // data
            };

            return await ThirdwebContract.Write(wallet, contract, "claim", payableAmount, fnArgs);
        }

        public static async Task<BigInteger> DropERC1155_GetActiveClaimConditionId(this ThirdwebContract contract, BigInteger tokenId)
        {
            return await ThirdwebContract.Read<BigInteger>(contract, "getActiveClaimConditionId", tokenId);
        }

        public static async Task<Drop_ClaimCondition> DropERC1155_GetClaimConditionById(this ThirdwebContract contract, BigInteger tokenId, BigInteger claimConditionId)
        {
            return await ThirdwebContract.Read<Drop_ClaimCondition>(contract, "getClaimConditionById", tokenId, claimConditionId);
        }

        public static async Task<Drop_ClaimCondition> DropERC1155_GetActiveClaimCondition(this ThirdwebContract contract, BigInteger tokenId)
        {
            var activeClaimConditionId = await contract.DropERC1155_GetActiveClaimConditionId(tokenId);
            return await contract.DropERC1155_GetClaimConditionById(tokenId, activeClaimConditionId);
        }

        #endregion

        #region TokenERC20

        public static async Task<ThirdwebTransactionReceipt> TokenERC20_MintTo(this ThirdwebContract contract, IThirdwebWallet wallet, string receiverAddress, string amount)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(receiverAddress))
            {
                throw new ArgumentException("Receiver address must be provided");
            }

            if (string.IsNullOrEmpty(amount))
            {
                throw new ArgumentException("Amount must be provided");
            }

            var erc20Decimals = await contract.ERC20_Decimals();

            var rawAmountToMint = BigInteger.Parse(amount.ToWei()).AdjustDecimals(18, erc20Decimals);

            return await ThirdwebContract.Write(wallet, contract, "mintTo", 0, receiverAddress, rawAmountToMint);
        }

        public static async Task<ThirdwebTransactionReceipt> TokenERC20_MintWithSignature(this ThirdwebContract contract, IThirdwebWallet wallet, TokenERC20_MintRequest mintRequest, string signature)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (mintRequest == null)
            {
                throw new ArgumentNullException(nameof(mintRequest));
            }

            if (string.IsNullOrEmpty(signature))
            {
                throw new ArgumentException("Signature must be provided");
            }

            var isNativeToken = mintRequest.Currency == Constants.NATIVE_TOKEN_ADDRESS;

            var payableAmount = isNativeToken ? mintRequest.Quantity * mintRequest.Price : BigInteger.Zero;

            return await ThirdwebContract.Write(wallet, contract, "mintWithSignature", payableAmount, mintRequest, signature);
        }

        public static async Task<(TokenERC20_MintRequest, string)> TokenERC20_GenerateMintSignature(this ThirdwebContract contract, IThirdwebWallet wallet, TokenERC20_MintRequest mintRequest)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (mintRequest == null)
            {
                throw new ArgumentNullException(nameof(mintRequest));
            }

            mintRequest = new TokenERC20_MintRequest
            {
                To = mintRequest.To ?? throw new ArgumentNullException(nameof(mintRequest.To)),
                PrimarySaleRecipient = mintRequest.PrimarySaleRecipient ?? await contract.GetPrimarySaleRecipient(),
                Quantity = mintRequest.Quantity > 0 ? mintRequest.Quantity : 1,
                Price = mintRequest.Price,
                Currency = mintRequest.Currency ?? Constants.NATIVE_TOKEN_ADDRESS,
                ValidityStartTimestamp = mintRequest.ValidityStartTimestamp,
                ValidityEndTimestamp = mintRequest.ValidityEndTimestamp > 0 ? mintRequest.ValidityEndTimestamp : Utils.GetUnixTimeStampIn10Years(),
                Uid = mintRequest.Uid ?? Guid.NewGuid().ToByteArray().PadTo32Bytes()
            };

            var contractMetadata = await contract.GetMetadata();

            var signature = await EIP712.GenerateSignature_TokenERC20(
                domainName: contractMetadata.Name,
                version: "1",
                chainId: contract.Chain,
                verifyingContract: contract.Address,
                mintRequest: mintRequest,
                signer: wallet
            );

            return (mintRequest, signature);
        }

        public static async Task<VerifyResult> TokenERC20_VerifyMintSignature(this ThirdwebContract contract, TokenERC20_MintRequest mintRequest, string signature)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (mintRequest == null)
            {
                throw new ArgumentNullException(nameof(mintRequest));
            }

            if (string.IsNullOrEmpty(signature))
            {
                throw new ArgumentException("Signature must be provided");
            }

            return await ThirdwebContract.Read<VerifyResult>(contract, "verify", mintRequest, signature.HexToBytes());
        }

        #endregion

        #region TokenERC721

        public static async Task<ThirdwebTransactionReceipt> TokenERC721_MintTo(this ThirdwebContract contract, IThirdwebWallet wallet, string receiverAddress, BigInteger tokenId, string uri)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(receiverAddress))
            {
                throw new ArgumentException("Receiver address must be provided");
            }

            if (tokenId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tokenId), "Token ID must be equal or greater than 0");
            }

            if (uri == null) // allow empty string uri
            {
                throw new ArgumentException("URI must be provided");
            }

            return await ThirdwebContract.Write(wallet, contract, "mintTo", 0, receiverAddress, tokenId, uri);
        }

        public static async Task<ThirdwebTransactionReceipt> TokenERC721_MintTo(
            this ThirdwebContract contract,
            IThirdwebWallet wallet,
            string receiverAddress,
            BigInteger tokenId,
            NFTMetadata metadata
        )
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(receiverAddress))
            {
                throw new ArgumentException("Receiver address must be provided");
            }

            if (tokenId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tokenId), "Token ID must be equal or greater than 0");
            }

            var json = JsonConvert.SerializeObject(metadata);

            var jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

            var ipfsResult = await ThirdwebStorage.UploadRaw(contract.Client, jsonBytes);

            return await ThirdwebContract.Write(wallet, contract, "mintTo", 0, receiverAddress, tokenId, $"ipfs://{ipfsResult.IpfsHash}");
        }

        public static async Task<ThirdwebTransactionReceipt> TokenERC721_MintWithSignature(
            this ThirdwebContract contract,
            IThirdwebWallet wallet,
            TokenERC721_MintRequest mintRequest,
            string signature
        )
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (mintRequest == null)
            {
                throw new ArgumentNullException(nameof(mintRequest));
            }

            if (string.IsNullOrEmpty(signature))
            {
                throw new ArgumentException("Signature must be provided");
            }

            var isNativeToken = mintRequest.Currency == Constants.NATIVE_TOKEN_ADDRESS;

            var payableAmount = isNativeToken ? mintRequest.Price : BigInteger.Zero;

            return await ThirdwebContract.Write(wallet, contract, "mintWithSignature", payableAmount, mintRequest, signature);
        }

        public static async Task<(TokenERC721_MintRequest, string)> TokenERC721_GenerateMintSignature(
            this ThirdwebContract contract,
            IThirdwebWallet wallet,
            TokenERC721_MintRequest mintRequest,
            NFTMetadata? metadataOverride = null
        )
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (mintRequest == null)
            {
                throw new ArgumentNullException(nameof(mintRequest));
            }

            var finalUri = mintRequest.Uri;

            if (finalUri == null) // allow empty string uri
            {
                if (metadataOverride != null)
                {
                    var json = JsonConvert.SerializeObject(metadataOverride);

                    var jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

                    var ipfsResult = await ThirdwebStorage.UploadRaw(contract.Client, jsonBytes);

                    finalUri = $"ipfs://{ipfsResult.IpfsHash}";
                }
                else
                {
                    throw new ArgumentException("MintRequest URI or NFTMetadata override must be provided");
                }
            }

            var defaultRoyaltyInfo = await contract.GetDefaultRoyaltyInfo();

            mintRequest = new TokenERC721_MintRequest
            {
                To = mintRequest.To ?? throw new ArgumentNullException(nameof(mintRequest.To)),
                RoyaltyRecipient = defaultRoyaltyInfo.Recipient,
                RoyaltyBps = defaultRoyaltyInfo.Bps,
                PrimarySaleRecipient = mintRequest.PrimarySaleRecipient ?? await contract.GetPrimarySaleRecipient(),
                Uri = finalUri,
                Price = mintRequest.Price,
                Currency = mintRequest.Currency ?? Constants.NATIVE_TOKEN_ADDRESS,
                ValidityStartTimestamp = mintRequest.ValidityStartTimestamp,
                ValidityEndTimestamp = mintRequest.ValidityEndTimestamp > 0 ? mintRequest.ValidityEndTimestamp : Utils.GetUnixTimeStampIn10Years(),
                Uid = mintRequest.Uid ?? Guid.NewGuid().ToByteArray().PadTo32Bytes()
            };

            var signature = await EIP712.GenerateSignature_TokenERC721(
                domainName: "TokenERC721",
                version: "1",
                chainId: contract.Chain,
                verifyingContract: contract.Address,
                mintRequest: mintRequest,
                signer: wallet
            );

            return (mintRequest, signature);
        }

        public static async Task<VerifyResult> TokenERC721_VerifyMintSignature(this ThirdwebContract contract, TokenERC721_MintRequest mintRequest, string signature)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (mintRequest == null)
            {
                throw new ArgumentNullException(nameof(mintRequest));
            }

            if (string.IsNullOrEmpty(signature))
            {
                throw new ArgumentException("Signature must be provided");
            }

            return await ThirdwebContract.Read<VerifyResult>(contract, "verify", mintRequest, signature.HexToBytes());
        }

        #endregion

        #region TokenERC1155

        public static async Task<ThirdwebTransactionReceipt> TokenERC1155_MintTo(
            this ThirdwebContract contract,
            IThirdwebWallet wallet,
            string receiverAddress,
            BigInteger tokenId,
            BigInteger quantity,
            string uri
        )
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(receiverAddress))
            {
                throw new ArgumentException("Receiver address must be provided");
            }

            if (tokenId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tokenId), "Token ID must be equal or greater than 0");
            }

            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than 0");
            }

            if (uri == null) // allow empty string uri
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return await ThirdwebContract.Write(wallet, contract, "mintTo", 0, receiverAddress, tokenId, uri, quantity);
        }

        public static async Task<ThirdwebTransactionReceipt> TokenERC1155_MintTo(
            this ThirdwebContract contract,
            IThirdwebWallet wallet,
            string receiverAddress,
            BigInteger tokenId,
            BigInteger quantity,
            NFTMetadata metadata
        )
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (string.IsNullOrEmpty(receiverAddress))
            {
                throw new ArgumentException("Receiver address must be provided");
            }

            if (tokenId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tokenId), "Token ID must be equal or greater than 0");
            }

            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than 0");
            }

            var json = JsonConvert.SerializeObject(metadata);

            var jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

            var ipfsResult = await ThirdwebStorage.UploadRaw(contract.Client, jsonBytes);

            return await ThirdwebContract.Write(wallet, contract, "mintTo", 0, receiverAddress, tokenId, $"ipfs://{ipfsResult.IpfsHash}", quantity);
        }

        public static async Task<ThirdwebTransactionReceipt> TokenERC1155_MintWithSignature(
            this ThirdwebContract contract,
            IThirdwebWallet wallet,
            TokenERC1155_MintRequest mintRequest,
            string signature
        )
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (mintRequest == null)
            {
                throw new ArgumentNullException(nameof(mintRequest));
            }

            if (string.IsNullOrEmpty(signature))
            {
                throw new ArgumentException("Signature must be provided");
            }

            var isNativeToken = mintRequest.Currency == Constants.NATIVE_TOKEN_ADDRESS;

            var payableAmount = isNativeToken ? mintRequest.Quantity * mintRequest.PricePerToken : BigInteger.Zero;

            return await ThirdwebContract.Write(wallet, contract, "mintWithSignature", payableAmount, mintRequest, signature);
        }

        public static async Task<(TokenERC1155_MintRequest, string)> TokenERC1155_GenerateMintSignature(
            this ThirdwebContract contract,
            IThirdwebWallet wallet,
            TokenERC1155_MintRequest mintRequest,
            NFTMetadata? metadataOverride = null
        )
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (mintRequest == null)
            {
                throw new ArgumentNullException(nameof(mintRequest));
            }

            var finalUri = mintRequest.Uri;

            if (finalUri == null) // allow empty string uri
            {
                if (metadataOverride != null)
                {
                    var json = JsonConvert.SerializeObject(metadataOverride);

                    var jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

                    var ipfsResult = await ThirdwebStorage.UploadRaw(contract.Client, jsonBytes);

                    finalUri = $"ipfs://{ipfsResult.IpfsHash}";
                }
                else
                {
                    throw new ArgumentException("MintRequest URI or NFTMetadata override must be provided");
                }
            }

            var defaultRoyaltyInfo = await contract.GetDefaultRoyaltyInfo();

            mintRequest = new TokenERC1155_MintRequest
            {
                To = mintRequest.To ?? throw new ArgumentNullException(nameof(mintRequest.To)),
                RoyaltyRecipient = defaultRoyaltyInfo.Recipient,
                RoyaltyBps = defaultRoyaltyInfo.Bps,
                TokenId = mintRequest.TokenId ?? await contract.ERC1155_TotalSupply(),
                PrimarySaleRecipient = mintRequest.PrimarySaleRecipient ?? await contract.GetPrimarySaleRecipient(),
                Uri = finalUri,
                Quantity = mintRequest.Quantity > 0 ? mintRequest.Quantity : 1,
                PricePerToken = mintRequest.PricePerToken,
                Currency = mintRequest.Currency ?? Constants.NATIVE_TOKEN_ADDRESS,
                ValidityStartTimestamp = mintRequest.ValidityStartTimestamp,
                ValidityEndTimestamp = mintRequest.ValidityEndTimestamp > 0 ? mintRequest.ValidityEndTimestamp : Utils.GetUnixTimeStampIn10Years(),
                Uid = mintRequest.Uid ?? Guid.NewGuid().ToByteArray().PadTo32Bytes()
            };

            var signature = await EIP712.GenerateSignature_TokenERC1155(
                domainName: "TokenERC1155",
                version: "1",
                chainId: contract.Chain,
                verifyingContract: contract.Address,
                mintRequest: mintRequest,
                signer: wallet
            );

            return (mintRequest, signature);
        }

        public static async Task<VerifyResult> TokenERC1155_VerifyMintSignature(this ThirdwebContract contract, TokenERC1155_MintRequest mintRequest, string signature)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (mintRequest == null)
            {
                throw new ArgumentNullException(nameof(mintRequest));
            }

            if (string.IsNullOrEmpty(signature))
            {
                throw new ArgumentException("Signature must be provided");
            }

            return await ThirdwebContract.Read<VerifyResult>(contract, "verify", mintRequest, signature.HexToBytes());
        }

        #endregion
    }
}
