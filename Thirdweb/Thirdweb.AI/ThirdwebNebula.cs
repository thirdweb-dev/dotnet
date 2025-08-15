using System.Numerics;
using Newtonsoft.Json;

namespace Thirdweb.AI;

public enum NebulaChatRole
{
    User,
    Assistant,
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
    public string WalletAddress { get; set; }

    /// <summary>
    /// Represents filters for narrowing down context in which operations are performed.
    /// </summary>
    /// <param name="chainIds">The chain IDs to filter by.</param>
    /// <param name="walletAddress">The wallet addresses to filter by.</param>
    public NebulaContext(List<BigInteger> chainIds = null, string walletAddress = null)
    {
        this.ChainIds = chainIds;
        this.WalletAddress = walletAddress;
    }
}

public class ThirdwebNebula
{
    public string SessionId { get; private set; }

    internal SessionManager Sessions { get; }
    internal ChatClient ChatClient { get; }
    internal ExecutionClient ExecuteClient { get; }

    internal ThirdwebNebula(ThirdwebClient client)
    {
        var httpClient = client.HttpClient;
        this.Sessions = new SessionManager(httpClient);
        this.ChatClient = new ChatClient(httpClient);
        this.ExecuteClient = new ExecutionClient(httpClient);
    }

    public static async Task<ThirdwebNebula> Create(ThirdwebClient client, string sessionId = null, string model = Constants.NEBULA_DEFAULT_MODEL)
    {
        var nebula = new ThirdwebNebula(client);

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            nebula.SessionId = sessionId;
            return nebula;
        }
        else
        {
            var session = await nebula.Sessions.CreateSessionAsync(
                new CreateSessionParams()
                {
                    ModelName = model,
                    Title = $"Thirdweb .NET SDK (v{Constants.VERSION}) | Nebula {model} Session | Client ID: {client.ClientId}",
                    IsPublic = false,
                }
            );
            nebula.SessionId = session.Id;
            return nebula;
        }
    }

    public async Task<NebulaChatResult> Chat(string message, IThirdwebWallet wallet = null, NebulaContext context = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be null or empty.", nameof(message));
        }

        var contextFiler = await this.PrepareContextFilter(wallet, context);

        var result = await this.ChatClient.SendMessageAsync(
            new ChatParamsSingleMessage()
            {
                SessionId = this.SessionId,
                Message = message,
                ContextFilter = contextFiler,
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

        var contextFiler = await this.PrepareContextFilter(wallet, context);

        var result = await this.ChatClient.SendMessagesAsync(
            new ChatParamsMultiMessages()
            {
                SessionId = this.SessionId,
                Messages = messages.Select(prompt => new ChatMessage() { Content = prompt.Message, Role = prompt.Role.ToString().ToLower() }).ToList(),
                ContextFilter = contextFiler,
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

        var contextFiler = await this.PrepareContextFilter(wallet, context);
        var result = await this.ExecuteClient.ExecuteAsync(
            new ChatParamsSingleMessage()
            {
                SessionId = this.SessionId,
                Message = message,
                ContextFilter = contextFiler,
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

    public async Task<NebulaExecuteResult> Execute(List<NebulaChatMessage> messages, IThirdwebWallet wallet, NebulaContext context = null)
    {
        if (messages == null || messages.Count == 0 || messages.Any(m => string.IsNullOrWhiteSpace(m.Message)))
        {
            throw new ArgumentException("Messages cannot be null or empty.", nameof(messages));
        }

        if (wallet == null)
        {
            throw new ArgumentException("Wallet cannot be null.", nameof(wallet));
        }

        var contextFiler = await this.PrepareContextFilter(wallet, context);
        var result = await this.ExecuteClient.ExecuteBatchAsync(
            new ChatParamsMultiMessages()
            {
                SessionId = this.SessionId,
                Messages = messages.Select(prompt => new ChatMessage() { Content = prompt.Message, Role = prompt.Role.ToString().ToLower() }).ToList(),
                ContextFilter = contextFiler,
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

    private async Task<CompletionContext> PrepareContextFilter(IThirdwebWallet wallet, NebulaContext context)
    {
        context ??= new NebulaContext();

        if (wallet != null)
        {
            context.WalletAddress ??= await wallet.GetAddress();
            if (wallet is SmartWallet smartWallet)
            {
                context.ChainIds ??= new List<BigInteger>();
                if (context.ChainIds.Count == 0 || !context.ChainIds.Contains(smartWallet.ActiveChainId))
                {
                    context.ChainIds.Add(smartWallet.ActiveChainId);
                }
            }
        }

        return new CompletionContext()
        {
            SessionId = this.SessionId,
            ChainIds = context?.ChainIds?.Select(id => id).ToList(),
            WalletAddress = context?.WalletAddress,
        };
    }

    private static async Task<List<ThirdwebTransaction>> PrepareTransactions(IThirdwebWallet wallet, List<AgentAction> actions)
    {
        if (wallet != null && actions != null && actions.Count > 0)
        {
            var transactionTasks = actions
                .Select(action =>
                {
                    if (action.Type == "sign_transaction")
                    {
                        var txInput = JsonConvert.DeserializeObject<ThirdwebTransactionInput>(action.Data);
                        return ThirdwebTransaction.Create(wallet, txInput);
                    }
                    else
                    {
                        return null;
                    }
                })
                .ToList();

            if (transactionTasks == null || transactionTasks.Count == 0)
            {
                return null;
            }

            _ = transactionTasks.RemoveAll(task => task == null);

            return (await Task.WhenAll(transactionTasks)).Where(tx => tx != null).ToList();
        }
        else
        {
            return null;
        }
    }
}
