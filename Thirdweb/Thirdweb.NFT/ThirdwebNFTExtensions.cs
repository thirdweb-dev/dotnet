using System.Numerics;

namespace Thirdweb
{
    public static class ThirdwebNFTExtensions
    {
        #region  Common

        public static async Task<byte[]> GetNFTImageBytes(this NFT nft, ThirdwebClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            return string.IsNullOrEmpty(nft.Metadata.Image) ? new byte[] { } : await ThirdwebStorage.Download<byte[]>(client, nft.Metadata.Image).ConfigureAwait(false);
        }

        #endregion

        #region ERC721

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

        #endregion

        #region ERC1155

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
    }
}
