using System.Numerics;

namespace Thirdweb.Tests.Extensions;

public class MarketplaceExtensionsTests : BaseTests
{
    private readonly string _marketplaceContractAddress = "0xc9671F631E8313D53ec0b5358e1a499c574fCe6A";
    private readonly string _marketplaceContractAbiNoEvents =
        "[{\"type\": \"constructor\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"tuple\",\"name\": \"_marketplaceV3Params\",\"components\": [{\"type\": \"tuple[]\",\"name\": \"extensions\",\"components\": [{\"type\": \"tuple\",\"name\": \"metadata\",\"components\": [{\"type\": \"string\",\"name\": \"name\"},{\"type\": \"string\",\"name\": \"metadataURI\"},{\"type\": \"address\",\"name\": \"implementation\"}]},{\"type\": \"tuple[]\",\"name\": \"functions\",\"components\": [{\"type\": \"bytes4\",\"name\": \"functionSelector\"},{\"type\": \"string\",\"name\": \"functionSignature\"}]}]},{\"type\": \"address\",\"name\": \"royaltyEngineAddress\"},{\"type\": \"address\",\"name\": \"nativeTokenWrapper\"}]}]},{\"name\": \"InvalidCodeAtRange\",\"type\": \"error\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_size\"},{\"type\": \"uint256\",\"name\": \"_start\"},{\"type\": \"uint256\",\"name\": \"_end\"}]},{\"name\": \"WriteError\",\"type\": \"error\",\"inputs\": []},{\"name\": \"DEFAULT_ADMIN_ROLE\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [],\"outputs\": [{\"type\": \"bytes32\"}]},{\"name\": \"_disableFunctionInExtension\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"string\",\"name\": \"_extensionName\"},{\"type\": \"bytes4\",\"name\": \"_functionSelector\"}],\"outputs\": []},{\"name\": \"addExtension\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"tuple\",\"name\": \"_extension\",\"components\": [{\"type\": \"tuple\",\"name\": \"metadata\",\"components\": [{\"type\": \"string\",\"name\": \"name\"},{\"type\": \"string\",\"name\": \"metadataURI\"},{\"type\": \"address\",\"name\": \"implementation\"}]},{\"type\": \"tuple[]\",\"name\": \"functions\",\"components\": [{\"type\": \"bytes4\",\"name\": \"functionSelector\"},{\"type\": \"string\",\"name\": \"functionSignature\"}]}]}],\"outputs\": []},{\"name\": \"contractType\",\"type\": \"function\",\"stateMutability\": \"pure\",\"inputs\": [],\"outputs\": [{\"type\": \"bytes32\"}]},{\"name\": \"contractURI\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [],\"outputs\": [{\"type\": \"string\"}]},{\"name\": \"contractVersion\",\"type\": \"function\",\"stateMutability\": \"pure\",\"inputs\": [],\"outputs\": [{\"type\": \"uint8\"}]},{\"name\": \"defaultExtensions\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [],\"outputs\": [{\"type\": \"address\"}]},{\"name\": \"disableFunctionInExtension\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"string\",\"name\": \"_extensionName\"},{\"type\": \"bytes4\",\"name\": \"_functionSelector\"}],\"outputs\": []},{\"name\": \"enableFunctionInExtension\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"string\",\"name\": \"_extensionName\"},{\"type\": \"tuple\",\"name\": \"_function\",\"components\": [{\"type\": \"bytes4\",\"name\": \"functionSelector\"},{\"type\": \"string\",\"name\": \"functionSignature\"}]}],\"outputs\": []},{\"name\": \"getAllExtensions\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [],\"outputs\": [{\"type\": \"tuple[]\",\"name\": \"allExtensions\",\"components\": [{\"type\": \"tuple\",\"name\": \"metadata\",\"components\": [{\"type\": \"string\",\"name\": \"name\"},{\"type\": \"string\",\"name\": \"metadataURI\"},{\"type\": \"address\",\"name\": \"implementation\"}]},{\"type\": \"tuple[]\",\"name\": \"functions\",\"components\": [{\"type\": \"bytes4\",\"name\": \"functionSelector\"},{\"type\": \"string\",\"name\": \"functionSignature\"}]}]}]},{\"name\": \"getExtension\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"string\",\"name\": \"extensionName\"}],\"outputs\": [{\"type\": \"tuple\",\"components\": [{\"type\": \"tuple\",\"name\": \"metadata\",\"components\": [{\"type\": \"string\",\"name\": \"name\"},{\"type\": \"string\",\"name\": \"metadataURI\"},{\"type\": \"address\",\"name\": \"implementation\"}]},{\"type\": \"tuple[]\",\"name\": \"functions\",\"components\": [{\"type\": \"bytes4\",\"name\": \"functionSelector\"},{\"type\": \"string\",\"name\": \"functionSignature\"}]}]}]},{\"name\": \"getFlatPlatformFeeInfo\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [],\"outputs\": [{\"type\": \"address\"},{\"type\": \"uint256\"}]},{\"name\": \"getImplementationForFunction\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"bytes4\",\"name\": \"_functionSelector\"}],\"outputs\": [{\"type\": \"address\"}]},{\"name\": \"getMetadataForFunction\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"bytes4\",\"name\": \"functionSelector\"}],\"outputs\": [{\"type\": \"tuple\",\"components\": [{\"type\": \"string\",\"name\": \"name\"},{\"type\": \"string\",\"name\": \"metadataURI\"},{\"type\": \"address\",\"name\": \"implementation\"}]}]},{\"name\": \"getPlatformFeeInfo\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [],\"outputs\": [{\"type\": \"address\"},{\"type\": \"uint16\"}]},{\"name\": \"getPlatformFeeType\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [],\"outputs\": [{\"type\": \"uint8\"}]},{\"name\": \"getRoleAdmin\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"bytes32\",\"name\": \"role\"}],\"outputs\": [{\"type\": \"bytes32\"}]},{\"name\": \"getRoleMember\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"bytes32\",\"name\": \"role\"},{\"type\": \"uint256\",\"name\": \"index\"}],\"outputs\": [{\"type\": \"address\",\"name\": \"member\"}]},{\"name\": \"getRoleMemberCount\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"bytes32\",\"name\": \"role\"}],\"outputs\": [{\"type\": \"uint256\",\"name\": \"count\"}]},{\"name\": \"getRoyalty\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"address\",\"name\": \"tokenAddress\"},{\"type\": \"uint256\",\"name\": \"tokenId\"},{\"type\": \"uint256\",\"name\": \"value\"}],\"outputs\": [{\"type\": \"address[]\",\"name\": \"recipients\"},{\"type\": \"uint256[]\",\"name\": \"amounts\"}]},{\"name\": \"getRoyaltyEngineAddress\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [],\"outputs\": [{\"type\": \"address\",\"name\": \"royaltyEngineAddress\"}]},{\"name\": \"grantRole\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"bytes32\",\"name\": \"role\"},{\"type\": \"address\",\"name\": \"account\"}],\"outputs\": []},{\"name\": \"hasRole\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"bytes32\",\"name\": \"role\"},{\"type\": \"address\",\"name\": \"account\"}],\"outputs\": [{\"type\": \"bool\"}]},{\"name\": \"hasRoleWithSwitch\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"bytes32\",\"name\": \"role\"},{\"type\": \"address\",\"name\": \"account\"}],\"outputs\": [{\"type\": \"bool\"}]},{\"name\": \"initialize\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"address\",\"name\": \"_defaultAdmin\"},{\"type\": \"string\",\"name\": \"_contractURI\"},{\"type\": \"address[]\",\"name\": \"_trustedForwarders\"},{\"type\": \"address\",\"name\": \"_platformFeeRecipient\"},{\"type\": \"uint16\",\"name\": \"_platformFeeBps\"}],\"outputs\": []},{\"name\": \"isTrustedForwarder\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"address\",\"name\": \"forwarder\"}],\"outputs\": [{\"type\": \"bool\"}]},{\"name\": \"multicall\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"bytes[]\",\"name\": \"data\"}],\"outputs\": [{\"type\": \"bytes[]\",\"name\": \"results\"}]},{\"name\": \"onERC1155BatchReceived\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"address\"},{\"type\": \"address\"},{\"type\": \"uint256[]\"},{\"type\": \"uint256[]\"},{\"type\": \"bytes\"}],\"outputs\": [{\"type\": \"bytes4\"}]},{\"name\": \"onERC1155Received\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"address\"},{\"type\": \"address\"},{\"type\": \"uint256\"},{\"type\": \"uint256\"},{\"type\": \"bytes\"}],\"outputs\": [{\"type\": \"bytes4\"}]},{\"name\": \"onERC721Received\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"address\"},{\"type\": \"address\"},{\"type\": \"uint256\"},{\"type\": \"bytes\"}],\"outputs\": [{\"type\": \"bytes4\"}]},{\"name\": \"removeExtension\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"string\",\"name\": \"_extensionName\"}],\"outputs\": []},{\"name\": \"renounceRole\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"bytes32\",\"name\": \"role\"},{\"type\": \"address\",\"name\": \"account\"}],\"outputs\": []},{\"name\": \"replaceExtension\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"tuple\",\"name\": \"_extension\",\"components\": [{\"type\": \"tuple\",\"name\": \"metadata\",\"components\": [{\"type\": \"string\",\"name\": \"name\"},{\"type\": \"string\",\"name\": \"metadataURI\"},{\"type\": \"address\",\"name\": \"implementation\"}]},{\"type\": \"tuple[]\",\"name\": \"functions\",\"components\": [{\"type\": \"bytes4\",\"name\": \"functionSelector\"},{\"type\": \"string\",\"name\": \"functionSignature\"}]}]}],\"outputs\": []},{\"name\": \"revokeRole\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"bytes32\",\"name\": \"role\"},{\"type\": \"address\",\"name\": \"account\"}],\"outputs\": []},{\"name\": \"setContractURI\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"string\",\"name\": \"_uri\"}],\"outputs\": []},{\"name\": \"setFlatPlatformFeeInfo\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"address\",\"name\": \"_platformFeeRecipient\"},{\"type\": \"uint256\",\"name\": \"_flatFee\"}],\"outputs\": []},{\"name\": \"setPlatformFeeInfo\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"address\",\"name\": \"_platformFeeRecipient\"},{\"type\": \"uint256\",\"name\": \"_platformFeeBps\"}],\"outputs\": []},{\"name\": \"setPlatformFeeType\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"uint8\",\"name\": \"_feeType\"}],\"outputs\": []},{\"name\": \"setRoyaltyEngine\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"address\",\"name\": \"_royaltyEngineAddress\"}],\"outputs\": []},{\"name\": \"supportsInterface\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"bytes4\",\"name\": \"interfaceId\"}],\"outputs\": [{\"type\": \"bool\"}]},{\"type\": \"receive\",\"stateMutability\": \"payable\"},{\"name\": \"_msgData\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [],\"outputs\": [{\"type\": \"bytes\"}]},{\"name\": \"_msgSender\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [],\"outputs\": [{\"type\": \"address\",\"name\": \"sender\"}]},{\"name\": \"approveBuyerForListing\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_listingId\"},{\"type\": \"address\",\"name\": \"_buyer\"},{\"type\": \"bool\",\"name\": \"_toApprove\"}],\"outputs\": []},{\"name\": \"approveCurrencyForListing\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_listingId\"},{\"type\": \"address\",\"name\": \"_currency\"},{\"type\": \"uint256\",\"name\": \"_pricePerTokenInCurrency\"}],\"outputs\": []},{\"name\": \"buyFromListing\",\"type\": \"function\",\"stateMutability\": \"payable\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_listingId\"},{\"type\": \"address\",\"name\": \"_buyFor\"},{\"type\": \"uint256\",\"name\": \"_quantity\"},{\"type\": \"address\",\"name\": \"_currency\"},{\"type\": \"uint256\",\"name\": \"_expectedTotalPrice\"}],\"outputs\": []},{\"name\": \"cancelListing\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_listingId\"}],\"outputs\": []},{\"name\": \"createListing\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"tuple\",\"name\": \"_params\",\"components\": [{\"type\": \"address\",\"name\": \"assetContract\"},{\"type\": \"uint256\",\"name\": \"tokenId\"},{\"type\": \"uint256\",\"name\": \"quantity\"},{\"type\": \"address\",\"name\": \"currency\"},{\"type\": \"uint256\",\"name\": \"pricePerToken\"},{\"type\": \"uint128\",\"name\": \"startTimestamp\"},{\"type\": \"uint128\",\"name\": \"endTimestamp\"},{\"type\": \"bool\",\"name\": \"reserved\"}]}],\"outputs\": [{\"type\": \"uint256\",\"name\": \"listingId\"}]},{\"name\": \"currencyPriceForListing\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_listingId\"},{\"type\": \"address\",\"name\": \"_currency\"}],\"outputs\": [{\"type\": \"uint256\"}]},{\"name\": \"getAllListings\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_startId\"},{\"type\": \"uint256\",\"name\": \"_endId\"}],\"outputs\": [{\"type\": \"tuple[]\",\"name\": \"_allListings\",\"components\": [{\"type\": \"uint256\",\"name\": \"listingId\"},{\"type\": \"uint256\",\"name\": \"tokenId\"},{\"type\": \"uint256\",\"name\": \"quantity\"},{\"type\": \"uint256\",\"name\": \"pricePerToken\"},{\"type\": \"uint128\",\"name\": \"startTimestamp\"},{\"type\": \"uint128\",\"name\": \"endTimestamp\"},{\"type\": \"address\",\"name\": \"listingCreator\"},{\"type\": \"address\",\"name\": \"assetContract\"},{\"type\": \"address\",\"name\": \"currency\"},{\"type\": \"uint8\",\"name\": \"tokenType\"},{\"type\": \"uint8\",\"name\": \"status\"},{\"type\": \"bool\",\"name\": \"reserved\"}]}]},{\"name\": \"getAllValidListings\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_startId\"},{\"type\": \"uint256\",\"name\": \"_endId\"}],\"outputs\": [{\"type\": \"tuple[]\",\"name\": \"_validListings\",\"components\": [{\"type\": \"uint256\",\"name\": \"listingId\"},{\"type\": \"uint256\",\"name\": \"tokenId\"},{\"type\": \"uint256\",\"name\": \"quantity\"},{\"type\": \"uint256\",\"name\": \"pricePerToken\"},{\"type\": \"uint128\",\"name\": \"startTimestamp\"},{\"type\": \"uint128\",\"name\": \"endTimestamp\"},{\"type\": \"address\",\"name\": \"listingCreator\"},{\"type\": \"address\",\"name\": \"assetContract\"},{\"type\": \"address\",\"name\": \"currency\"},{\"type\": \"uint8\",\"name\": \"tokenType\"},{\"type\": \"uint8\",\"name\": \"status\"},{\"type\": \"bool\",\"name\": \"reserved\"}]}]},{\"name\": \"getListing\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_listingId\"}],\"outputs\": [{\"type\": \"tuple\",\"name\": \"listing\",\"components\": [{\"type\": \"uint256\",\"name\": \"listingId\"},{\"type\": \"uint256\",\"name\": \"tokenId\"},{\"type\": \"uint256\",\"name\": \"quantity\"},{\"type\": \"uint256\",\"name\": \"pricePerToken\"},{\"type\": \"uint128\",\"name\": \"startTimestamp\"},{\"type\": \"uint128\",\"name\": \"endTimestamp\"},{\"type\": \"address\",\"name\": \"listingCreator\"},{\"type\": \"address\",\"name\": \"assetContract\"},{\"type\": \"address\",\"name\": \"currency\"},{\"type\": \"uint8\",\"name\": \"tokenType\"},{\"type\": \"uint8\",\"name\": \"status\"},{\"type\": \"bool\",\"name\": \"reserved\"}]}]},{\"name\": \"isBuyerApprovedForListing\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_listingId\"},{\"type\": \"address\",\"name\": \"_buyer\"}],\"outputs\": [{\"type\": \"bool\"}]},{\"name\": \"isCurrencyApprovedForListing\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_listingId\"},{\"type\": \"address\",\"name\": \"_currency\"}],\"outputs\": [{\"type\": \"bool\"}]},{\"name\": \"totalListings\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [],\"outputs\": [{\"type\": \"uint256\"}]},{\"name\": \"updateListing\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_listingId\"},{\"type\": \"tuple\",\"name\": \"_params\",\"components\": [{\"type\": \"address\",\"name\": \"assetContract\"},{\"type\": \"uint256\",\"name\": \"tokenId\"},{\"type\": \"uint256\",\"name\": \"quantity\"},{\"type\": \"address\",\"name\": \"currency\"},{\"type\": \"uint256\",\"name\": \"pricePerToken\"},{\"type\": \"uint128\",\"name\": \"startTimestamp\"},{\"type\": \"uint128\",\"name\": \"endTimestamp\"},{\"type\": \"bool\",\"name\": \"reserved\"}]}],\"outputs\": []},{\"name\": \"bidInAuction\",\"type\": \"function\",\"stateMutability\": \"payable\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_auctionId\"},{\"type\": \"uint256\",\"name\": \"_bidAmount\"}],\"outputs\": []},{\"name\": \"cancelAuction\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_auctionId\"}],\"outputs\": []},{\"name\": \"collectAuctionPayout\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_auctionId\"}],\"outputs\": []},{\"name\": \"collectAuctionTokens\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_auctionId\"}],\"outputs\": []},{\"name\": \"createAuction\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"tuple\",\"name\": \"_params\",\"components\": [{\"type\": \"address\",\"name\": \"assetContract\"},{\"type\": \"uint256\",\"name\": \"tokenId\"},{\"type\": \"uint256\",\"name\": \"quantity\"},{\"type\": \"address\",\"name\": \"currency\"},{\"type\": \"uint256\",\"name\": \"minimumBidAmount\"},{\"type\": \"uint256\",\"name\": \"buyoutBidAmount\"},{\"type\": \"uint64\",\"name\": \"timeBufferInSeconds\"},{\"type\": \"uint64\",\"name\": \"bidBufferBps\"},{\"type\": \"uint64\",\"name\": \"startTimestamp\"},{\"type\": \"uint64\",\"name\": \"endTimestamp\"}]}],\"outputs\": [{\"type\": \"uint256\",\"name\": \"auctionId\"}]},{\"name\": \"getAllAuctions\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_startId\"},{\"type\": \"uint256\",\"name\": \"_endId\"}],\"outputs\": [{\"type\": \"tuple[]\",\"name\": \"_allAuctions\",\"components\": [{\"type\": \"uint256\",\"name\": \"auctionId\"},{\"type\": \"uint256\",\"name\": \"tokenId\"},{\"type\": \"uint256\",\"name\": \"quantity\"},{\"type\": \"uint256\",\"name\": \"minimumBidAmount\"},{\"type\": \"uint256\",\"name\": \"buyoutBidAmount\"},{\"type\": \"uint64\",\"name\": \"timeBufferInSeconds\"},{\"type\": \"uint64\",\"name\": \"bidBufferBps\"},{\"type\": \"uint64\",\"name\": \"startTimestamp\"},{\"type\": \"uint64\",\"name\": \"endTimestamp\"},{\"type\": \"address\",\"name\": \"auctionCreator\"},{\"type\": \"address\",\"name\": \"assetContract\"},{\"type\": \"address\",\"name\": \"currency\"},{\"type\": \"uint8\",\"name\": \"tokenType\"},{\"type\": \"uint8\",\"name\": \"status\"}]}]},{\"name\": \"getAllValidAuctions\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_startId\"},{\"type\": \"uint256\",\"name\": \"_endId\"}],\"outputs\": [{\"type\": \"tuple[]\",\"name\": \"_validAuctions\",\"components\": [{\"type\": \"uint256\",\"name\": \"auctionId\"},{\"type\": \"uint256\",\"name\": \"tokenId\"},{\"type\": \"uint256\",\"name\": \"quantity\"},{\"type\": \"uint256\",\"name\": \"minimumBidAmount\"},{\"type\": \"uint256\",\"name\": \"buyoutBidAmount\"},{\"type\": \"uint64\",\"name\": \"timeBufferInSeconds\"},{\"type\": \"uint64\",\"name\": \"bidBufferBps\"},{\"type\": \"uint64\",\"name\": \"startTimestamp\"},{\"type\": \"uint64\",\"name\": \"endTimestamp\"},{\"type\": \"address\",\"name\": \"auctionCreator\"},{\"type\": \"address\",\"name\": \"assetContract\"},{\"type\": \"address\",\"name\": \"currency\"},{\"type\": \"uint8\",\"name\": \"tokenType\"},{\"type\": \"uint8\",\"name\": \"status\"}]}]},{\"name\": \"getAuction\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_auctionId\"}],\"outputs\": [{\"type\": \"tuple\",\"name\": \"_auction\",\"components\": [{\"type\": \"uint256\",\"name\": \"auctionId\"},{\"type\": \"uint256\",\"name\": \"tokenId\"},{\"type\": \"uint256\",\"name\": \"quantity\"},{\"type\": \"uint256\",\"name\": \"minimumBidAmount\"},{\"type\": \"uint256\",\"name\": \"buyoutBidAmount\"},{\"type\": \"uint64\",\"name\": \"timeBufferInSeconds\"},{\"type\": \"uint64\",\"name\": \"bidBufferBps\"},{\"type\": \"uint64\",\"name\": \"startTimestamp\"},{\"type\": \"uint64\",\"name\": \"endTimestamp\"},{\"type\": \"address\",\"name\": \"auctionCreator\"},{\"type\": \"address\",\"name\": \"assetContract\"},{\"type\": \"address\",\"name\": \"currency\"},{\"type\": \"uint8\",\"name\": \"tokenType\"},{\"type\": \"uint8\",\"name\": \"status\"}]}]},{\"name\": \"getWinningBid\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_auctionId\"}],\"outputs\": [{\"type\": \"address\",\"name\": \"_bidder\"},{\"type\": \"address\",\"name\": \"_currency\"},{\"type\": \"uint256\",\"name\": \"_bidAmount\"}]},{\"name\": \"isAuctionExpired\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_auctionId\"}],\"outputs\": [{\"type\": \"bool\"}]},{\"name\": \"isNewWinningBid\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_auctionId\"},{\"type\": \"uint256\",\"name\": \"_bidAmount\"}],\"outputs\": [{\"type\": \"bool\"}]},{\"name\": \"totalAuctions\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [],\"outputs\": [{\"type\": \"uint256\"}]},{\"name\": \"acceptOffer\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_offerId\"}],\"outputs\": []},{\"name\": \"cancelOffer\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_offerId\"}],\"outputs\": []},{\"name\": \"getAllOffers\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_startId\"},{\"type\": \"uint256\",\"name\": \"_endId\"}],\"outputs\": [{\"type\": \"tuple[]\",\"name\": \"_allOffers\",\"components\": [{\"type\": \"uint256\",\"name\": \"offerId\"},{\"type\": \"uint256\",\"name\": \"tokenId\"},{\"type\": \"uint256\",\"name\": \"quantity\"},{\"type\": \"uint256\",\"name\": \"totalPrice\"},{\"type\": \"uint256\",\"name\": \"expirationTimestamp\"},{\"type\": \"address\",\"name\": \"offeror\"},{\"type\": \"address\",\"name\": \"assetContract\"},{\"type\": \"address\",\"name\": \"currency\"},{\"type\": \"uint8\",\"name\": \"tokenType\"},{\"type\": \"uint8\",\"name\": \"status\"}]}]},{\"name\": \"getAllValidOffers\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_startId\"},{\"type\": \"uint256\",\"name\": \"_endId\"}],\"outputs\": [{\"type\": \"tuple[]\",\"name\": \"_validOffers\",\"components\": [{\"type\": \"uint256\",\"name\": \"offerId\"},{\"type\": \"uint256\",\"name\": \"tokenId\"},{\"type\": \"uint256\",\"name\": \"quantity\"},{\"type\": \"uint256\",\"name\": \"totalPrice\"},{\"type\": \"uint256\",\"name\": \"expirationTimestamp\"},{\"type\": \"address\",\"name\": \"offeror\"},{\"type\": \"address\",\"name\": \"assetContract\"},{\"type\": \"address\",\"name\": \"currency\"},{\"type\": \"uint8\",\"name\": \"tokenType\"},{\"type\": \"uint8\",\"name\": \"status\"}]}]},{\"name\": \"getOffer\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [{\"type\": \"uint256\",\"name\": \"_offerId\"}],\"outputs\": [{\"type\": \"tuple\",\"name\": \"_offer\",\"components\": [{\"type\": \"uint256\",\"name\": \"offerId\"},{\"type\": \"uint256\",\"name\": \"tokenId\"},{\"type\": \"uint256\",\"name\": \"quantity\"},{\"type\": \"uint256\",\"name\": \"totalPrice\"},{\"type\": \"uint256\",\"name\": \"expirationTimestamp\"},{\"type\": \"address\",\"name\": \"offeror\"},{\"type\": \"address\",\"name\": \"assetContract\"},{\"type\": \"address\",\"name\": \"currency\"},{\"type\": \"uint8\",\"name\": \"tokenType\"},{\"type\": \"uint8\",\"name\": \"status\"}]}]},{\"name\": \"makeOffer\",\"type\": \"function\",\"stateMutability\": \"nonpayable\",\"inputs\": [{\"type\": \"tuple\",\"name\": \"_params\",\"components\": [{\"type\": \"address\",\"name\": \"assetContract\"},{\"type\": \"uint256\",\"name\": \"tokenId\"},{\"type\": \"uint256\",\"name\": \"quantity\"},{\"type\": \"address\",\"name\": \"currency\"},{\"type\": \"uint256\",\"name\": \"totalPrice\"},{\"type\": \"uint256\",\"name\": \"expirationTimestamp\"}]}],\"outputs\": [{\"type\": \"uint256\",\"name\": \"_offerId\"}]},{\"name\": \"totalOffers\",\"type\": \"function\",\"stateMutability\": \"view\",\"inputs\": [],\"outputs\": [{\"type\": \"uint256\"}]},{\"stateMutability\": \"payable\",\"type\": \"fallback\"}]";

    private readonly BigInteger _chainId = 421614;

    public MarketplaceExtensionsTests(ITestOutputHelper output)
        : base(output) { }

    private async Task<IThirdwebWallet> GetSmartWallet()
    {
        var privateKeyWallet = await PrivateKeyWallet.Generate(this.Client);
        return await SmartWallet.Create(personalWallet: privateKeyWallet, chainId: 421614);
    }

    private async Task<ThirdwebContract> GetMarketplaceContract()
    {
        return await ThirdwebContract.Create(this.Client, this._marketplaceContractAddress, this._chainId, this._marketplaceContractAbiNoEvents);
    }

    #region IDirectListings

    [Fact(Timeout = 120000)]
    public async Task Marketplace_DirectListings_TotalListings_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var totalListings = await contract.Marketplace_DirectListings_TotalListings();
        Assert.True(totalListings >= 0);
    }

    [Fact(Timeout = 120000)]
    public async Task Marketplace_DirectListings_GetAllListings_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var startId = BigInteger.Zero;
        var endId = 10;

        var listings = await contract.Marketplace_DirectListings_GetAllListings(startId, endId);
        Assert.NotNull(listings);
        Assert.True(listings.Count >= 0);
    }

    [Fact(Timeout = 120000)]
    public async Task Marketplace_DirectListings_GetAllValidListings_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var startId = BigInteger.Zero;
        var endId = 10;

        var listings = await contract.Marketplace_DirectListings_GetAllValidListings(startId, endId);
        Assert.NotNull(listings);
        Assert.True(listings.Count >= 0);
    }

    [Fact(Timeout = 120000)]
    public async Task Marketplace_DirectListings_GetListing_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var listingId = BigInteger.One;

        var listing = await contract.Marketplace_DirectListings_GetListing(listingId);
        Assert.NotNull(listing);
    }

    #endregion

    #region IEnglishAuctions

    [Fact(Timeout = 120000)]
    public async Task Marketplace_EnglishAuctions_GetAuction_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var auctionId = BigInteger.One;

        var auction = await contract.Marketplace_EnglishAuctions_GetAuction(auctionId);
        Assert.NotNull(auction);
    }

    [Fact(Timeout = 120000)]
    public async Task Marketplace_EnglishAuctions_GetAllAuctions_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var startId = BigInteger.Zero;
        var endId = BigInteger.Zero;

        var auctions = await contract.Marketplace_EnglishAuctions_GetAllAuctions(startId, endId);
        Assert.NotNull(auctions);
    }

    [Fact(Timeout = 120000)]
    public async Task Marketplace_EnglishAuctions_GetAllValidAuctions_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var startId = BigInteger.Zero;
        var endId = BigInteger.Zero;

        var auctions = await contract.Marketplace_EnglishAuctions_GetAllValidAuctions(startId, endId);
        Assert.NotNull(auctions);
        Assert.True(auctions.Count >= 0);
    }

    // [Fact(Timeout = 120000)]
    // public async Task Marketplace_EnglishAuctions_GetWinningBid_Success()
    // {
    //     var contract = await this.GetMarketplaceContract();
    //     var auctionId = BigInteger.One;

    //     var (bidder, currency, bidAmount) = await contract.Marketplace_EnglishAuctions_GetWinningBid(auctionId);
    //     Assert.NotNull(bidder);
    //     Assert.NotNull(currency);
    //     Assert.True(bidAmount >= 0);
    // }

    // [Fact(Timeout = 120000)]
    // public async Task Marketplace_EnglishAuctions_IsAuctionExpired_Success()
    // {
    //     var contract = await this.GetMarketplaceContract();
    //     var auctionId = BigInteger.One;

    //     _ = await contract.Marketplace_EnglishAuctions_IsAuctionExpired(auctionId);
    // }

    #endregion

    #region IOffers

    [Fact(Timeout = 120000)]
    public async Task Marketplace_Offers_GetOffer_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var offerId = BigInteger.One;

        var offer = await contract.Marketplace_Offers_GetOffer(offerId);
        Assert.NotNull(offer);
    }

    [Fact(Timeout = 120000)]
    public async Task Marketplace_Offers_GetAllOffers_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var startId = BigInteger.Zero;
        var endId = BigInteger.Zero;

        var offers = await contract.Marketplace_Offers_GetAllOffers(startId, endId);
        Assert.NotNull(offers);
        Assert.True(offers.Count >= 0);
    }

    [Fact(Timeout = 120000)]
    public async Task Marketplace_Offers_GetAllValidOffers_Success()
    {
        var contract = await this.GetMarketplaceContract();
        var startId = BigInteger.Zero;
        var endId = BigInteger.Zero;

        var offers = await contract.Marketplace_Offers_GetAllValidOffers(startId, endId);
        Assert.NotNull(offers);
        Assert.True(offers.Count >= 0);
    }

    #endregion
}
