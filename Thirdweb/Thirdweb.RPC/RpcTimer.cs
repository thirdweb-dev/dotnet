using System.Diagnostics;

namespace Thirdweb
{
    public class RpcTimer
    {
        private Stopwatch stopwatch;
        private readonly TimeSpan interval;

        public RpcTimer(TimeSpan interval)
        {
            this.interval = interval;
            this.stopwatch = Stopwatch.StartNew();
        }

        public bool CheckTick()
        {
            Console.WriteLine($"Elapsed time: {stopwatch.ElapsedMilliseconds} ms");

            if (stopwatch.Elapsed >= interval)
            {
                stopwatch.Restart();
                return true;
            }
            return false;
        }
    }
}
