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
        while ((DateTime.UtcNow - startTime).TotalMilliseconds < millisecondsDelay && !cancellationToken.IsCancellationRequested)
        {
            // Yield to avoid blocking the main thread, especially in WebGL
            await Task.Yield();

            // Introduce a minimal delay to check again
            await MinimalDelay(10);
        }
    }

    /// <summary>
    /// Provides a minimal delay by looping for a specified number of milliseconds.
    /// </summary>
    /// <param name="milliseconds">The number of milliseconds to delay.</param>
    /// <returns>A task that completes after the specified minimal delay.</returns>
    private static async Task MinimalDelay(int milliseconds)
    {
        var startTime = DateTime.UtcNow;
        while ((DateTime.UtcNow - startTime).TotalMilliseconds < milliseconds)
        {
            await Task.Yield();
        }
    }
}
