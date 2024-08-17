namespace Thirdweb;

/// <summary>
/// Specifies the type of timeout for various operations.
/// </summary>
public enum TimeoutType
{
    /// <summary>
    /// Timeout for storage operations.
    /// </summary>
    Storage,

    /// <summary>
    /// Timeout for RPC operations.
    /// </summary>
    Rpc,

    /// <summary>
    /// Timeout for other types of operations.
    /// </summary>
    Other,
}
