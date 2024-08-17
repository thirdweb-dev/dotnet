namespace Thirdweb;

/// <summary>
/// Interface for defining timeout options for different types of operations.
/// </summary>
public interface ITimeoutOptions
{
    /// <summary>
    /// Gets the timeout value for the specified operation type.
    /// </summary>
    /// <param name="type">The type of operation.</param>
    /// <param name="fallback">The fallback timeout value if none is specified.</param>
    /// <returns>The timeout value for the specified operation type.</returns>
    int GetTimeout(TimeoutType type, int fallback = Constants.DEFAULT_FETCH_TIMEOUT);
}
