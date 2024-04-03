namespace Thirdweb
{
    public class TimeoutOptions : ITimeoutOptions
    {
        internal int? Storage { get; private set; }
        internal int? Rpc { get; private set; }
        internal int? Other { get; private set; }

        public TimeoutOptions(int? storage = null, int? rpc = null, int? other = null)
        {
            Storage = storage;
            Rpc = rpc;
            Other = other;
        }

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
