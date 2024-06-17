using System.Numerics;

namespace Thirdweb
{
    public static class ThirdwebNFTExtensions
    {
        #region  Common

        public static async Task<byte[]> GetNFTImageBytes(this NFT nft, ThirdwebClient client)
        {
            return await ThirdwebStorage.Download<byte[]>(client, nft.Metadata.Image);
        }

        #endregion

        #region ERC721

        public static async Task<NFT> ERC721_GetNFT(this ThirdwebContract contract, BigInteger tokenId)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            var uri = await contract.ERC721_TokenURI(tokenId);
            var metadata = await ThirdwebStorage.Download<NFTMetadata>(contract.Client, uri);
            metadata.Id = tokenId.ToString();

            var owner = Constants.ADDRESS_ZERO;
            try
            {
                owner = await contract.ERC721_OwnerOf(tokenId);
            }
            catch (Exception)
            {
                owner = Constants.ADDRESS_ZERO;
            }

            var supply = -BigInteger.MinusOne;
            try
            {
                supply = await contract.ERC721_TotalSupply();
            }
            catch (Exception)
            {
                supply = -1;
            }

            var quantityOwned = -1;

            return new NFT
            {
                Metadata = metadata,
                Owner = owner,
                Type = NFTType.ERC721,
                Supply = supply,
                QuantityOwned = quantityOwned
            };
        }

        public static async Task<List<NFT>> ERC721_GetAllNFTs(this ThirdwebContract contract, BigInteger? startTokenIdIncluded = null, BigInteger? endTokenIdExcluded = null)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            var nfts = new List<NFT>();

            if (startTokenIdIncluded == null)
            {
                startTokenIdIncluded = 0;
            }

            if (endTokenIdExcluded == null)
            {
                var totalSupply = await contract.ERC721_TotalSupply();
                endTokenIdExcluded = totalSupply;
            }

            for (var i = startTokenIdIncluded.Value; i < endTokenIdExcluded.Value; i++)
            {
                var nft = await contract.ERC721_GetNFT(i);
                nfts.Add(nft);
            }

            return nfts;
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

            var nfts = new List<NFT>();

            var totalSupply = await contract.ERC721_TotalSupply();

            try
            {
                var balanceOfOwner = await contract.ERC721_BalanceOf(owner);
                for (var i = 0; i < balanceOfOwner; i++)
                {
                    var tokenOfOwnerByIndex = await contract.ERC721_TokenOfOwnerByIndex(owner, i);
                    var nft = await contract.ERC721_GetNFT(tokenOfOwnerByIndex);
                    nfts.Add(nft);
                }
            }
            catch (Exception)
            {
                nfts = new List<NFT>();
                for (var i = 0; i < totalSupply; i++)
                {
                    var nft = await contract.ERC721_GetNFT(i);
                    if (nft.Owner == owner)
                    {
                        nfts.Add(nft);
                    }
                }
            }

            return nfts;
        }

        #endregion

        #region ERC1155

        public static async Task<NFT> ERC1155_GetNFT(this ThirdwebContract contract, BigInteger tokenId)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            var uri = await contract.ERC1155_URI(tokenId);
            var metadata = await ThirdwebStorage.Download<NFTMetadata>(contract.Client, uri);
            metadata.Id = tokenId.ToString();
            var owner = string.Empty;
            var supply = BigInteger.MinusOne;
            try
            {
                supply = await contract.ERC1155_TotalSupply(tokenId);
            }
            catch (Exception)
            {
                supply = BigInteger.MinusOne;
            }
            var quantityOwned = BigInteger.MinusOne;

            return new NFT
            {
                Metadata = metadata,
                Owner = owner,
                Type = NFTType.ERC1155,
                Supply = supply,
                QuantityOwned = quantityOwned
            };
        }

        public static async Task<List<NFT>> ERC1155_GetAllNFTs(this ThirdwebContract contract, BigInteger? startTokenIdIncluded = null, BigInteger? endTokenIdExcluded = null)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            var nfts = new List<NFT>();

            if (startTokenIdIncluded == null)
            {
                startTokenIdIncluded = 0;
            }

            if (endTokenIdExcluded == null)
            {
                var totalSupply = await contract.ERC1155_TotalSupply();
                endTokenIdExcluded = totalSupply;
            }

            for (var i = startTokenIdIncluded.Value; i < endTokenIdExcluded.Value; i++)
            {
                var nft = await contract.ERC1155_GetNFT(i);
                nfts.Add(nft);
            }

            return nfts;
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

            var totalSupply = await contract.ERC1155_TotalSupply();

            var nfts = new List<NFT>();
            for (var i = 0; i < totalSupply; i++)
            {
                var balanceOfOwner = await contract.ERC1155_BalanceOf(owner, i);
                if (balanceOfOwner > 0)
                {
                    var nft = await contract.ERC1155_GetNFT(i);
                    nft.QuantityOwned = balanceOfOwner;
                    nfts.Add(nft);
                }
            }

            return nfts;
        }

        #endregion
    }
}
