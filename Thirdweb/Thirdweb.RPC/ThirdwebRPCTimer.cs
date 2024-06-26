namespace Thirdweb
{
    public class ThirdwebRPCTimer : IDisposable
    {
        private readonly TimeSpan _interval;
        private CancellationTokenSource _cancellationTokenSource;
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
                _cancellationTokenSource = new CancellationTokenSource();
                RunTimer(_cancellationTokenSource.Token);
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
                _cancellationTokenSource.Cancel();
            }
        }

        private async void RunTimer(CancellationToken cancellationToken)
        {
            while (_isRunning)
            {
                var startTime = DateTime.UtcNow;
                while ((DateTime.UtcNow - startTime) < _interval)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    await Task.Yield();
                }
                Elapsed?.Invoke();
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }
    }
}
