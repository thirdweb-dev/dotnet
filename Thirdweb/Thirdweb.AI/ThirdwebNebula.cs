using System.Numerics;
using Newtonsoft.Json;

namespace Thirdweb.AI;

public enum NebulaChatRole
{
    User,
    Assistant
}

public class NebulaChatMessage
{
    public NebulaChatRole Role { get; set; } = NebulaChatRole.User;
    public string Message { get; set; }

    public NebulaChatMessage(string message, NebulaChatRole role = NebulaChatRole.User)
    {
        this.Message = message;
        this.Role = role;
    }
}

public class NebulaChatResult
{
    public string Message { get; set; }
    public List<ThirdwebTransaction> Transactions { get; set; }
}

public class NebulaExecuteResult
{
    public string Message { get; set; }
    public List<ThirdwebTransactionReceipt> TransactionReceipts { get; set; }
}

public class NebulaContext
{
    public List<BigInteger> ChainIds { get; set; }
    public List<string> ContractAddresses { get; set; }
    public List<string> WalletAddresses { get; set; }

    /// <summary>
    /// Represents filters for narrowing down context in which operations are performed.
    /// </summary>
    /// <param name="chainIds">The chain IDs to filter by.</param>
    /// <param name="contractAddresses">The contract addresses to filter by.</param>
    /// <param name="walletAddresses">The wallet addresses to filter by.</param>
    public NebulaContext(List<BigInteger> chainIds = null, List<string> contractAddresses = null, List<string> walletAddresses = null)
    {
        this.ChainIds = chainIds;
        this.ContractAddresses = contractAddresses;
        this.WalletAddresses = walletAddresses;
    }
}

public class ThirdwebNebula
{
    public string SessionId { get; private set; }

    internal SessionManager Sessions { get; }
    internal ChatClient ChatClient { get; }
    internal ExecutionClient ExecuteClient { get; }
    internal FeedbackClient FeedbackClient { get; }

    internal ThirdwebNebula(ThirdwebClient client)
    {
        var httpClient = client.HttpClient;
        this.Sessions = new SessionManager(httpClient);
        this.ChatClient = new ChatClient(httpClient);
        this.ExecuteClient = new ExecutionClient(httpClient);
        this.FeedbackClient = new FeedbackClient(httpClient);
    }

    public static async Task<ThirdwebNebula> Create(ThirdwebClient client, string model = Constants.NEBULA_DEFAULT_MODEL)
    {
        var nebula = new ThirdwebNebula(client);
        var session = await nebula.Sessions.CreateSessionAsync(
            new CreateSessionParams()
            {
                ModelName = model,
                Title = $"Thirdweb .NET SDK (v{Constants.VERSION}) | Nebula {model} Session | Client ID: {client.ClientId}",
                IsPublic = false
            }
        );
        nebula.SessionId = session.Id;
        return nebula;
    }

    public async Task<NebulaChatResult> Chat(string message, IThirdwebWallet wallet = null, NebulaContext context = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be null or empty.", nameof(message));
        }

        var contextFiler = await PrepareContextFilter(wallet, context);

        var result = await this.ChatClient.SendMessageAsync(
            new ChatParamsSingleMessage()
            {
                SessionId = this.SessionId,
                Message = message,
                ContextFilter = contextFiler,
                ExecuteConfig = wallet == null ? null : new ExecuteConfig() { Mode = "client", SignerWalletAddress = await wallet.GetAddress() }
            }
        );

        var transactions = await PrepareTransactions(wallet, result.Actions);

        return new NebulaChatResult() { Message = result.Message, Transactions = transactions == null || transactions.Count == 0 ? null : transactions };
    }

    public async Task<NebulaChatResult> Chat(List<NebulaChatMessage> messages, IThirdwebWallet wallet = null, NebulaContext context = null)
    {
        if (messages == null || messages.Count == 0 || messages.Any(m => string.IsNullOrWhiteSpace(m.Message)))
        {
            throw new ArgumentException("Messages cannot be null or empty.", nameof(messages));
        }

        var contextFiler = await PrepareContextFilter(wallet, context);

        var result = await this.ChatClient.SendMessagesAsync(
            new ChatParamsMultiMessages()
            {
                SessionId = this.SessionId,
                Messages = messages.Select(prompt => new ChatMessage() { Content = prompt.Message, Role = prompt.Role.ToString().ToLower() }).ToList(),
                ContextFilter = contextFiler,
                ExecuteConfig = wallet == null ? null : new ExecuteConfig() { Mode = "client", SignerWalletAddress = await wallet.GetAddress() }
            }
        );

        var transactions = await PrepareTransactions(wallet, result.Actions);

        return new NebulaChatResult() { Message = result.Message, Transactions = transactions == null || transactions.Count == 0 ? null : transactions };
    }

    public async Task<NebulaExecuteResult> Execute(string message, IThirdwebWallet wallet, NebulaContext context = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be null or empty.", nameof(message));
        }

        if (wallet == null)
        {
            throw new ArgumentException("Wallet cannot be null.", nameof(wallet));
        }

        var contextFiler = await PrepareContextFilter(wallet, context);
        var result = await this.ExecuteClient.ExecuteAsync(
            new ChatParamsSingleMessage()
            {
                SessionId = this.SessionId,
                Message = message,
                ContextFilter = contextFiler,
                ExecuteConfig = new ExecuteConfig() { Mode = "client", SignerWalletAddress = await wallet.GetAddress() }
            }
        );

        var transactions = await PrepareTransactions(wallet, result.Actions);
        if (transactions == null || transactions.Count == 0)
        {
            return new NebulaExecuteResult() { Message = result.Message };
        }
        else
        {
            var receipts = await Task.WhenAll(transactions.Select(ThirdwebTransaction.SendAndWaitForTransactionReceipt));
            return new NebulaExecuteResult() { Message = result.Message, TransactionReceipts = receipts.ToList() };
        }
    }

    private static async Task<ContextFilter> PrepareContextFilter(IThirdwebWallet wallet, NebulaContext context)
    {
        context ??= new NebulaContext();

        if (wallet != null)
        {
            var walletAddress = await wallet.GetAddress();

            // Add the wallet address to the context
            if (context.WalletAddresses == null || context.WalletAddresses.Count == 0)
            {
                context.WalletAddresses = new List<string>() { walletAddress };
            }
            else if (!context.WalletAddresses.Contains(walletAddress))
            {
                context.WalletAddresses.Add(walletAddress);
            }

            // If it's a smart wallet, add the contract address and chain ID to the context
            if (wallet is SmartWallet smartWallet)
            {
                if (context.ContractAddresses == null || context.ContractAddresses.Count == 0)
                {
                    context.ContractAddresses = new List<string>() { walletAddress };
                }
                else if (!context.ContractAddresses.Contains(walletAddress))
                {
                    context.ContractAddresses.Add(walletAddress);
                }

                if (context.ChainIds == null || context.ChainIds.Count == 0)
                {
                    context.ChainIds = new List<BigInteger>() { smartWallet.ActiveChainId };
                }
                else if (!context.ChainIds.Contains(smartWallet.ActiveChainId))
                {
                    context.ChainIds.Add(smartWallet.ActiveChainId);
                }
            }
        }

        return new ContextFilter()
        {
            ChainIds = context?.ChainIds?.Select(id => id.ToString()).ToList(),
            ContractAddresses = context?.ContractAddresses,
            WalletAddresses = context?.WalletAddresses
        };
    }

    private static async Task<List<ThirdwebTransaction>> PrepareTransactions(IThirdwebWallet wallet, List<AgentAction> actions)
    {
        if (wallet != null && actions != null && actions.Count > 0)
        {
            var transactionTasks = actions
                .Select(action =>
                {
                    if (action.Type == "transaction")
                    {
                        var txInput = JsonConvert.DeserializeObject<ThirdwebTransactionInput>(action.Data.ToString());
                        return ThirdwebTransaction.Create(wallet, txInput);
                    }
                    else
                    {
                        return null;
                    }
                })
                .ToList();

            return (await Task.WhenAll(transactionTasks)).Where(tx => tx != null).ToList();
        }
        else
        {
            return null;
        }
    }
}
