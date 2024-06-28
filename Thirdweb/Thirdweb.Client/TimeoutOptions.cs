namespace Thirdweb
{
    /// <summary>
    /// Represents the timeout options for different types of operations.
    /// </summary>
    public class TimeoutOptions : ITimeoutOptions
    {
        internal int? Storage { get; private set; }
        internal int? Rpc { get; private set; }
        internal int? Other { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeoutOptions"/> class.
        /// </summary>
        /// <param name="storage">The timeout for storage operations (optional).</param>
        /// <param name="rpc">The timeout for RPC operations (optional).</param>
        /// <param name="other">The timeout for other operations (optional).</param>
        public TimeoutOptions(int? storage = null, int? rpc = null, int? other = null)
        {
            Storage = storage;
            Rpc = rpc;
            Other = other;
        }

        /// <summary>
        /// Gets the timeout value for the specified operation type.
        /// </summary>
        /// <param name="type">The type of operation.</param>
        /// <param name="fallback">The fallback timeout value if none is specified (default is <see cref="Constants.DEFAULT_FETCH_TIMEOUT"/>).</param>
        /// <returns>The timeout value for the specified operation type.</returns>
        public int GetTimeout(TimeoutType type, int fallback = Constants.DEFAULT_FETCH_TIMEOUT)
        {
            return type switch
            {
                TimeoutType.Storage => Storage ?? fallback,
                TimeoutType.Rpc => Rpc ?? fallback,
                TimeoutType.Other => Other ?? fallback,
                _ => fallback,
            };
        }
    }
}
