namespace Thirdweb
{
    /// <summary>
    /// Represents a timer for RPC batching.
    /// </summary>
    public class ThirdwebRPCTimer : IDisposable
    {
        private readonly TimeSpan _interval;
        private bool _isRunning;
        private readonly object _lock = new object();

        /// <summary>
        /// Occurs when the timer interval has elapsed.
        /// </summary>
        public event Action Elapsed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThirdwebRPCTimer"/> class with the specified interval.
        /// </summary>
        /// <param name="interval">The interval at which the timer elapses.</param>
        public ThirdwebRPCTimer(TimeSpan interval)
        {
            _interval = interval;
            _isRunning = false;
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public void Start()
        {
            lock (_lock)
            {
                if (_isRunning)
                {
                    return;
                }

                _isRunning = true;
                RunTimer();
            }
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void Stop()
        {
            lock (_lock)
            {
                if (!_isRunning)
                {
                    return;
                }

                _isRunning = false;
            }
        }

        /// <summary>
        /// Disposes the timer, stopping its execution.
        /// </summary>
        public void Dispose()
        {
            _isRunning = false;
        }

        private async void RunTimer()
        {
            while (_isRunning)
            {
                var startTime = DateTime.UtcNow;
                while ((DateTime.UtcNow - startTime) < _interval)
                {
                    if (!_isRunning)
                    {
                        return;
                    }

                    await Task.Delay(10).ConfigureAwait(false);
                }
                Elapsed?.Invoke();
            }
        }
    }
}
