namespace Thirdweb.Indexer;

public static class ThirdwebInsightExtensions
{
    public static NFT ToNFT(this Token_NFT token)
    {
        if (token == null)
        {
            return new NFT();
        }

        return new NFT()
        {
            Type = token.Contract?.Type switch
            {
                "ERC721" => NFTType.ERC721,
                "ERC1155" => NFTType.ERC1155,
                _ => throw new Exception($"Unknown NFT type: {token.Contract.Type}")
            },
            Metadata = new NFTMetadata()
            {
                Id = token.TokenId,
                Description = token.Description,
                Image = token.ImageUrl,
                Name = token.Name,
                VideoUrl = token.VideoUrl,
                AnimationUrl = token.AnimationUrl,
                ExternalUrl = token.ExternalUrl,
                BackgroundColor = token.BackgroundColor,
                Attributes = token.ExtraMetadata?.Attributes,
                Properties = token.ExtraMetadata?.Properties,
            }
        };
    }

    public static List<NFT> ToNFTList(this IEnumerable<Token_NFT> tokens)
    {
        if (tokens == null)
        {
            return new List<NFT>();
        }

        return tokens.Select(token => token.ToNFT()).ToList();
    }
}
