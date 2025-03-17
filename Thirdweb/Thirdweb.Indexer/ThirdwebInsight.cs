using System.Numerics;
using Newtonsoft.Json;

namespace Thirdweb.Indexer;

public enum SortBy
{
    BlockNumber,
    BlockTimestamp,
    TransactionIndex,
}

public enum SortOrder
{
    Asc,
    Desc,
}

public class InsightEvents
{
    public Event[] Events { get; set; }
    public Meta Meta { get; set; }
}

public class InsightTransactions
{
    public Transaction[] Transactions { get; set; }
    public Meta Meta { get; set; }
}

public class ThirdwebInsight
{
    private readonly IThirdwebHttpClient _httpClient;

    internal ThirdwebInsight(ThirdwebClient client)
    {
        this._httpClient = client.HttpClient;
    }

    /// <summary>
    /// Create a new instance of the ThirdwebInsight class.
    /// </summary>
    /// <param name="client">The ThirdwebClient instance.</param>
    /// <returns>A new instance of <see cref="ThirdwebInsight"/>.</returns>
    public static Task<ThirdwebInsight> Create(ThirdwebClient client)
    {
        return Task.FromResult(new ThirdwebInsight(client));
    }

    public async Task<Token_Price> GetTokenPrice(string addressOrSymbol, BigInteger chainId, long? timestamp = null)
    {
        var prices = await this.GetTokenPrices(new[] { addressOrSymbol }, new[] { chainId }, timestamp).ConfigureAwait(false);
        if (prices.Length == 0)
        {
            throw new Exception("Token price not found.");
        }
        return prices[0];
    }

    public async Task<Token_Price[]> GetTokenPrices(string[] addressOrSymbols, BigInteger[] chainIds, long? timestamp = null)
    {
        var addresses = addressOrSymbols.Where(Utils.IsValidAddress).ToArray();
        var symbols = addressOrSymbols.Except(addresses).ToArray();

        var url = AppendChains($"{Constants.INSIGHT_API_URL}/v1/tokens/price", chainIds);

        if (addresses.Length > 0)
        {
            url += $"&address={string.Join("&address=", addresses)}";
        }

        if (symbols.Length > 0)
        {
            url += $"&symbol={string.Join("&symbol=", symbols)}";
        }

        if (timestamp.HasValue)
        {
            url += $"&timestamp={timestamp}";
        }

        var response = await this._httpClient.GetAsync(url).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonConvert.DeserializeObject<ResponseModel<Token_Price>>(responseContent).Data;
    }

    /// <summary>
    /// Get the token balances of an address.
    /// </summary>
    /// <param name="ownerAddress">The address to get the token balances of.</param>
    /// <param name="chainIds">The chain IDs to get the token balances from.</param>
    /// <param name="withMetadata">Whether to include NFT metadata in the response. (Default: true)</param>
    /// <returns>A tuple containing the ERC20, ERC721, and ERC1155 tokens.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the owner address is null or empty.</exception>
    /// <exception cref="ArgumentException">Thrown when no chain IDs are provided.</exception>
    public async Task<(Token_ERC20[] erc20Tokens, Token_ERC721[] erc721Tokens, Token_ERC1155[] erc1155Tokens)> GetTokens(string ownerAddress, BigInteger[] chainIds, bool withMetadata = true)
    {
        if (string.IsNullOrEmpty(ownerAddress))
        {
            throw new ArgumentNullException(nameof(ownerAddress));
        }

        if (chainIds.Length == 0)
        {
            throw new ArgumentException("At least one chain ID must be provided.", nameof(chainIds));
        }

        var erc20Tokens = await this.GetTokens_ERC20(ownerAddress, chainIds).ConfigureAwait(false);
        var erc721Tokens = await this.GetTokens_ERC721(ownerAddress, chainIds, withMetadata: withMetadata).ConfigureAwait(false);
        var erc1155Tokens = await this.GetTokens_ERC1155(ownerAddress, chainIds, withMetadata: withMetadata).ConfigureAwait(false);
        return (erc20Tokens, erc721Tokens, erc1155Tokens);
    }

    /// <summary>
    /// Get the ERC20 tokens of an address.
    /// </summary>
    /// <param name="ownerAddress">The address to get the ERC20 tokens of.</param>
    /// <param name="chainIds">The chain IDs to get the ERC20 tokens from.</param>
    /// <param name="limit">The number of tokens to return. (Default: 50)</param>
    /// <param name="page">The page number to return. (Default: 0)</param>
    /// <returns>An array of ERC20 tokens.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the owner address is null or empty.</exception>
    /// /// <exception cref="ArgumentException">Thrown when no chain IDs are provided.</exception>
    public async Task<Token_ERC20[]> GetTokens_ERC20(string ownerAddress, BigInteger[] chainIds, int limit = 50, int page = 0)
    {
        if (string.IsNullOrEmpty(ownerAddress))
        {
            throw new ArgumentNullException(nameof(ownerAddress));
        }

        if (chainIds.Length == 0)
        {
            throw new ArgumentException("At least one chain ID must be provided.", nameof(chainIds));
        }

        var url = AppendChains($"{Constants.INSIGHT_API_URL}/v1/tokens/erc20/{ownerAddress}", chainIds);
        url += $"&limit={limit}";
        url += $"&page={page}";
        var response = await this._httpClient.GetAsync(url).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonConvert.DeserializeObject<ResponseModel<Token_ERC20>>(responseContent).Data;
    }

    /// <summary>
    /// Get the ERC721 tokens of an address.
    /// </summary>
    /// <param name="ownerAddress">The address to get the ERC721 tokens of.</param>
    /// <param name="chainIds">The chain IDs to get the ERC721 tokens from.</param>
    /// <param name="limit">The number of tokens to return. (Default: 50)</param>
    /// <param name="page">The page number to return. (Default: 0)</param>
    /// <param name="withMetadata">Whether to include NFT metadata in the response. (Default: true)</param>
    /// <returns>An array of ERC721 tokens.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the owner address is null or empty.</exception>
    /// /// <exception cref="ArgumentException">Thrown when no chain IDs are provided.</exception>
    public async Task<Token_ERC721[]> GetTokens_ERC721(string ownerAddress, BigInteger[] chainIds, int limit = 50, int page = 0, bool withMetadata = true)
    {
        if (string.IsNullOrEmpty(ownerAddress))
        {
            throw new ArgumentNullException(nameof(ownerAddress));
        }

        if (chainIds.Length == 0)
        {
            throw new ArgumentException("At least one chain ID must be provided.", nameof(chainIds));
        }

        var url = AppendChains($"{Constants.INSIGHT_API_URL}/v1/tokens/erc721/{ownerAddress}", chainIds);
        url += $"&limit={limit}";
        url += $"&page={page}";
        url += $"&metadata={withMetadata.ToString().ToLower()}";
        var response = await this._httpClient.GetAsync(url).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonConvert.DeserializeObject<ResponseModel<Token_ERC721>>(responseContent).Data;
    }

    /// <summary>
    /// Get the ERC1155 tokens of an address.
    /// </summary>
    /// <param name="ownerAddress">The address to get the ERC1155 tokens of.</param>
    /// <param name="chainIds">The chain IDs to get the ERC1155 tokens from.</param>
    /// <param name="limit">The number of tokens to return. (Default: 50)</param>
    /// <param name="page">The page number to return. (Default: 0)</param>
    /// <param name="withMetadata">Whether to include NFT metadata in the response. (Default: true)</param>
    /// <returns>An array of ERC1155 tokens.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the owner address is null or empty.</exception>
    /// /// <exception cref="ArgumentException">Thrown when no chain IDs are provided.</exception>
    public async Task<Token_ERC1155[]> GetTokens_ERC1155(string ownerAddress, BigInteger[] chainIds, int limit = 50, int page = 0, bool withMetadata = true)
    {
        if (string.IsNullOrEmpty(ownerAddress))
        {
            throw new ArgumentNullException(nameof(ownerAddress));
        }

        if (chainIds.Length == 0)
        {
            throw new ArgumentException("At least one chain ID must be provided.", nameof(chainIds));
        }

        var url = AppendChains($"{Constants.INSIGHT_API_URL}/v1/tokens/erc1155/{ownerAddress}", chainIds);
        url += $"&limit={limit}";
        url += $"&page={page}";
        url += $"&metadata={withMetadata.ToString().ToLower()}";
        var response = await this._httpClient.GetAsync(url).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonConvert.DeserializeObject<ResponseModel<Token_ERC1155>>(responseContent).Data;
    }

    /// <summary>
    /// Get events, optionally filtered by contract address, event signature, and more.
    /// </summary>
    /// <param name="chainIds">The chain IDs to get the events from.</param>
    /// <param name="contractAddress">The contract address to get the events from. (Optional)</param>
    /// <param name="eventSignature">The event signature to get the events from. (Optional)</param>
    /// <param name="fromBlock">The starting block number to get the events from. (Optional, if provided, said block is included in query)</param>
    /// <param name="toBlock">The ending block number to get the events from. (Optional, if provided, said block is included in query)</param>
    /// <param name="fromTimestamp">The starting block timestamp to get the events from. (Optional, if provided, said block is included in query)</param>
    /// <param name="toTimestamp">The ending block timestamp to get the events from. (Optional, if provided, said block is included in query)</param>
    /// <param name="sortBy">The field to sort the events by. (Default: BlockNumber)</param>
    /// <param name="sortOrder">The order to sort the events by. (Default: Desc)</param>
    /// <param name="limit">The number of events to return. (Default: 20)</param>
    /// <param name="page">The page number to return. (Default: 0)</param>
    /// <param name="decode">Whether to decode the events. (Default: true)</param>
    /// <returns>The events and metadata as an instance of <see cref="InsightEvents"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when an event signature is provided without a contract address.</exception>
    /// /// <exception cref="ArgumentException">Thrown when no chain IDs are provided.</exception>
    public async Task<InsightEvents> GetEvents(
        BigInteger[] chainIds,
        string contractAddress = null,
        string eventSignature = null,
        BigInteger? fromBlock = null,
        BigInteger? toBlock = null,
        BigInteger? fromTimestamp = null,
        BigInteger? toTimestamp = null,
        SortBy sortBy = SortBy.BlockNumber,
        SortOrder sortOrder = SortOrder.Desc,
        int limit = 20,
        int page = 0,
        bool decode = true
    )
    {
        if (!string.IsNullOrEmpty(eventSignature) && string.IsNullOrEmpty(contractAddress))
        {
            throw new ArgumentException("Contract address must be provided when event signature is provided.");
        }

        if (chainIds.Length == 0)
        {
            throw new ArgumentException("At least one chain ID must be provided.", nameof(chainIds));
        }

        var baseUrl = $"{Constants.INSIGHT_API_URL}/v1/events";
        var url = AppendChains(
            !string.IsNullOrEmpty(contractAddress)
                ? !string.IsNullOrEmpty(eventSignature)
                    ? $"{baseUrl}/{contractAddress}/{eventSignature}"
                    : $"{baseUrl}/{contractAddress}"
                : baseUrl,
            chainIds
        );

        url += $"&sort_by={SortByToString(sortBy)}";
        url += $"&sort_order={SortOrderToString(sortOrder)}";
        url += $"&limit={limit}";
        url += $"&page={page}";
        url += $"&decode={decode}";

        if (fromBlock.HasValue)
        {
            url += $"&filter_block_number_gte={fromBlock}";
        }

        if (toBlock.HasValue)
        {
            url += $"&filter_block_number_lte={toBlock}";
        }

        if (fromTimestamp.HasValue)
        {
            url += $"&filter_block_timestamp_gte={fromTimestamp}";
        }

        if (toTimestamp.HasValue)
        {
            url += $"&filter_block_timestamp_lte={toTimestamp}";
        }

        var response = await this._httpClient.GetAsync(url).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var result = JsonConvert.DeserializeObject<ResponseModel<Event>>(responseContent);
        return new InsightEvents { Events = result.Data, Meta = result.Meta, };
    }

    /// <summary>
    /// Get transactions, optionally filtered by contract address, signature, and more.
    /// </summary>
    /// <param name="chainIds">The chain IDs to get the transactions from.</param>
    /// <param name="contractAddress">The contract address to get the transactions from. (Optional)</param>
    /// <param name="signature">The signature to filter transactions by. (Optional)</param>
    /// <param name="fromBlock">The starting block number to get the transactions from. (Optional, if provided, said block is included in query)</param>
    /// <param name="toBlock">The ending block number to get the transactions from. (Optional, if provided, said block is included in query)</param>
    /// <param name="fromTimestamp">The starting block timestamp to get the transactions from. (Optional, if provided, said block is included in query)</param>
    /// <param name="toTimestamp">The ending block timestamp to get the transactions from. (Optional, if provided, said block is included in query)</param>
    /// <param name="sortBy">The field to sort the transactions by. (Default: BlockNumber)</param>
    /// <param name="sortOrder">The order to sort the transactions by. (Default: Desc)</param>
    /// <param name="limit">The number of transactions to return. (Default: 20)</param>
    /// <param name="page">The page number to return. (Default: 0)</param>
    /// <param name="decode">Whether to decode the transactions. (Default: true)</param>
    /// <returns>The transactions and metadata as an instance of <see cref="InsightTransactions"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when a signature is provided without a contract address.</exception>
    /// /// <exception cref="ArgumentException">Thrown when no chain IDs are provided.</exception>
    public async Task<InsightTransactions> GetTransactions(
        BigInteger[] chainIds,
        string contractAddress = null,
        string signature = null,
        BigInteger? fromBlock = null,
        BigInteger? toBlock = null,
        BigInteger? fromTimestamp = null,
        BigInteger? toTimestamp = null,
        SortBy sortBy = SortBy.BlockNumber,
        SortOrder sortOrder = SortOrder.Desc,
        int limit = 20,
        int page = 0,
        bool decode = true
    )
    {
        if (!string.IsNullOrEmpty(signature) && string.IsNullOrEmpty(contractAddress))
        {
            throw new ArgumentException("Contract address must be provided when signature is provided.");
        }

        if (chainIds.Length == 0)
        {
            throw new ArgumentException("At least one chain ID must be provided.", nameof(chainIds));
        }

        var baseUrl = $"{Constants.INSIGHT_API_URL}/v1/transactions";
        var url = AppendChains(
            !string.IsNullOrEmpty(contractAddress)
                ? !string.IsNullOrEmpty(signature)
                    ? $"{baseUrl}/{contractAddress}/{signature}"
                    : $"{baseUrl}/{contractAddress}"
                : baseUrl,
            chainIds
        );

        url += $"&sort_by={SortByToString(sortBy)}";
        url += $"&sort_order={SortOrderToString(sortOrder)}";
        url += $"&limit={limit}";
        url += $"&page={page}";
        url += $"&decode={decode}";

        if (fromBlock.HasValue)
        {
            url += $"&filter_block_number_gte={fromBlock}";
        }

        if (toBlock.HasValue)
        {
            url += $"&filter_block_number_lte={toBlock}";
        }

        if (fromTimestamp.HasValue)
        {
            url += $"&filter_block_timestamp_gte={fromTimestamp}";
        }

        if (toTimestamp.HasValue)
        {
            url += $"&filter_block_timestamp_lte={toTimestamp}";
        }

        var response = await this._httpClient.GetAsync(url).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var result = JsonConvert.DeserializeObject<ResponseModel<Transaction>>(responseContent);
        return new InsightTransactions { Transactions = result.Data, Meta = result.Meta, };
    }

    private static string AppendChains(string url, BigInteger[] chainIds)
    {
        return $"{url}?chain={string.Join("&chain=", chainIds)}";
    }

    private static string SortByToString(SortBy sortBy)
    {
        return sortBy switch
        {
            SortBy.BlockNumber => "block_number",
            SortBy.BlockTimestamp => "block_timestamp",
            SortBy.TransactionIndex => "transaction_index",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null),
        };
    }

    private static string SortOrderToString(SortOrder sortOrder)
    {
        return sortOrder switch
        {
            SortOrder.Asc => "asc",
            SortOrder.Desc => "desc",
            _ => throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, null),
        };
    }
}
