namespace Thirdweb
{
    public class ThirdwebRPCTimer : IDisposable
    {
        private readonly TimeSpan _interval;
        private bool _isRunning;
        private readonly object _lock = new object();

        public event Action Elapsed;

        public ThirdwebRPCTimer(TimeSpan interval)
        {
            _interval = interval;
            _isRunning = false;
        }

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

        public void Dispose()
        {
            _isRunning = false;
        }
    }
}
