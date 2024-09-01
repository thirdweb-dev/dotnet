namespace Thirdweb;

/// <summary>
/// Represents a timer for RPC batching.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ThirdwebRPCTimer"/> class with the specified interval.
/// </remarks>
/// <param name="interval">The interval at which the timer elapses.</param>
public class ThirdwebRPCTimer(TimeSpan interval) : IDisposable
{
    private readonly TimeSpan _interval = interval;
    private bool _isRunning;
    private readonly object _lock = new();

    /// <summary>
    /// Occurs when the timer interval has elapsed.
    /// </summary>
    public event Action Elapsed;

    /// <summary>
    /// Starts the timer.
    /// </summary>
    public void Start()
    {
        lock (this._lock)
        {
            if (this._isRunning)
            {
                return;
            }

            this._isRunning = true;
            this.RunTimer();
        }
    }

    /// <summary>
    /// Stops the timer.
    /// </summary>
    public void Stop()
    {
        lock (this._lock)
        {
            if (!this._isRunning)
            {
                return;
            }

            this._isRunning = false;
        }
    }

    /// <summary>
    /// Disposes the timer, stopping its execution.
    /// </summary>
    public void Dispose()
    {
        this._isRunning = false;
    }

    private async void RunTimer()
    {
        while (this._isRunning)
        {
            var startTime = DateTime.UtcNow;
            while ((DateTime.UtcNow - startTime) < this._interval)
            {
                if (!this._isRunning)
                {
                    return;
                }

                await Task.Delay(10).ConfigureAwait(false);
            }
            Elapsed?.Invoke();
        }
    }
}
