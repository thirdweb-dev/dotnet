using System.Numerics;
using Newtonsoft.Json;

namespace Thirdweb.AI;

/// <summary>
/// Represents the response model wrapping the result of an API call.
/// </summary>
/// <typeparam name="T">The type of the result.</typeparam>
internal class ResponseModel<T>
{
    /// <summary>
    /// The result returned by the API.
    /// </summary>
    [JsonProperty("result")]
    internal T Result { get; set; }
}

/// <summary>
/// Represents an action performed by an agent in a session.
/// </summary>
internal class AgentAction
{
    [JsonProperty("session_id")]
    internal string SessionId { get; set; }

    [JsonProperty("request_id")]
    internal string RequestId { get; set; }

    [JsonProperty("type")]
    internal string Type { get; set; }

    [JsonProperty("source")]
    internal string Source { get; set; }

    [JsonProperty("data")]
    internal string Data { get; set; }
}

/// <summary>
/// Represents a single chat message.
/// </summary>
internal class ChatMessage
{
    [JsonProperty("role")]
    internal string Role { get; set; } = "user";

    [JsonProperty("content")]
    internal string Content { get; set; }
}

/// <summary>
/// Represents parameters for sending multiple chat messages in a single request.
/// </summary>
internal class ChatParamsMultiMessages
{
    [JsonProperty("stream")]
    internal bool? Stream { get; set; } = false;

    [JsonProperty("session_id")]
    internal string SessionId { get; set; }

    [JsonProperty("context")]
    internal CompletionContext ContextFilter { get; set; }

    [JsonProperty("model_name")]
    internal string ModelName { get; set; }

    [JsonProperty("messages")]
    internal List<ChatMessage> Messages { get; set; }
}

/// <summary>
/// Represents parameters for sending a single chat message.
/// </summary>
internal class ChatParamsSingleMessage
{
    [JsonProperty("stream")]
    internal bool? Stream { get; set; } = false;

    [JsonProperty("session_id")]
    internal string SessionId { get; set; }

    [JsonProperty("context")]
    internal CompletionContext ContextFilter { get; set; }

    [JsonProperty("model_name")]
    internal string ModelName { get; set; }

    [JsonProperty("message")]
    internal string Message { get; set; }
}

/// <summary>
/// Represents the response from a chat interaction.
/// </summary>
public class ChatResponse
{
    [JsonProperty("message")]
    internal string Message { get; set; }

    [JsonProperty("actions")]
    internal List<AgentAction> Actions { get; set; }

    [JsonProperty("session_id")]
    internal string SessionId { get; set; }

    [JsonProperty("request_id")]
    internal string RequestId { get; set; }
}

/// <summary>
/// Represents filters for narrowing down context in which operations are performed.
/// </summary>
internal class CompletionContext
{
    [JsonProperty("session_id")]
    public string SessionId { get; set; } = null;

    [JsonProperty("wallet_address")]
    public string WalletAddress { get; set; } = null;

    [JsonProperty("chain_ids")]
    public List<BigInteger> ChainIds { get; set; } = null;
}

/// <summary>
/// Nebula representation of a smart contract.
/// </summary>
public class DeployedContract
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("address")]
    public string Address { get; set; }

    [JsonProperty("chain_id")]
    public BigInteger ChainId { get; set; }

    [JsonProperty("contract_type")]
    public string ContractType { get; set; } = string.Empty;
}

/// <summary>
/// Represents parameters for creating a new session.
/// </summary>
internal class CreateSessionParams
{
    [JsonProperty("model_name")]
    internal string ModelName { get; set; } = Constants.NEBULA_DEFAULT_MODEL;

    [JsonProperty("title")]
    internal string Title { get; set; }

    [JsonProperty("is_public")]
    internal bool? IsPublic { get; set; }

    [JsonProperty("context")]
    internal CompletionContext ContextFilter { get; set; }
}

/// <summary>
/// Represents session details.
/// </summary>
internal class Session
{
    [JsonProperty("id")]
    internal string Id { get; set; }

    [JsonProperty("account_id")]
    internal string AccountId { get; set; }

    [JsonProperty("model_name")]
    internal string ModelName { get; set; }

    [JsonProperty("is_public")]
    internal bool? IsPublic { get; set; }

    [JsonProperty("title")]
    internal string Title { get; set; }

    [JsonProperty("memory")]
    internal List<object> Memory { get; set; }

    [JsonProperty("history")]
    internal List<object> History { get; set; }

    [JsonProperty("action")]
    internal List<object> Action { get; set; }

    [JsonProperty("context")]
    internal CompletionContext ContextFilter { get; set; }

    [JsonProperty("archive_at")]
    internal DateTime? ArchiveAt { get; set; }

    [JsonProperty("deleted_at")]
    internal DateTime? DeletedAt { get; set; }

    [JsonProperty("created_at")]
    internal DateTime? CreatedAt { get; set; }

    [JsonProperty("updated_at")]
    internal DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Represents parameters for updating a session.
/// </summary>
internal class UpdateSessionParams
{
    [JsonProperty("title")]
    internal string Title { get; set; }

    [JsonProperty("model_name")]
    internal string ModelName { get; set; }

    [JsonProperty("is_public")]
    internal bool? IsPublic { get; set; }

    [JsonProperty("context")]
    internal CompletionContext ContextFilter { get; set; }
}

/// <summary>
/// Represents the response for deleting a session.
/// </summary>
internal class SessionDeleteResponse
{
    [JsonProperty("id")]
    internal string Id { get; set; }

    [JsonProperty("deleted_at")]
    internal DateTime DeletedAt { get; set; }
}

/// <summary>
/// Represents a session in a session list.
/// </summary>
internal class SessionList
{
    [JsonProperty("id")]
    internal string Id { get; set; }

    [JsonProperty("title")]
    internal string Title { get; set; }

    [JsonProperty("created_at")]
    internal DateTime CreatedAt { get; set; }

    [JsonProperty("updated_at")]
    internal DateTime UpdatedAt { get; set; }
}
