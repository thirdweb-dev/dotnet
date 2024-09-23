using System.Diagnostics;

namespace Thirdweb;

public static class ThirdwebTask
{
    /// <summary>
    /// Simulates a delay without using Task.Delay or System.Threading.Timer, specifically designed to avoid clashing with WebGL threading.
    /// </summary>
    /// <param name="millisecondsDelay">The number of milliseconds to delay.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the delay.</param>
    /// <returns>A task that completes after the specified delay.</returns>
    public static async Task Delay(int millisecondsDelay, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var endTime = startTime.AddMilliseconds(millisecondsDelay);
        var currentDelay = 10;

        while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
        {
            await MinimalDelay(currentDelay);

            if (DateTime.UtcNow.AddMilliseconds(currentDelay) < endTime)
            {
                currentDelay = Math.Min(currentDelay * 2, 100);
            }
            else
            {
                currentDelay = (int)(endTime - DateTime.UtcNow).TotalMilliseconds;
            }
        }
    }

    /// <summary>
    /// Provides a minimal delay using a manual loop with short sleeps to reduce CPU usage.
    /// </summary>
    /// <param name="milliseconds">The number of milliseconds to delay.</param>
    private static async Task MinimalDelay(int milliseconds)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.ElapsedMilliseconds < milliseconds)
        {
            Thread.Sleep(1);
            await Task.Yield();
        }
    }
}
