namespace Thirdweb.EWS
{
    internal class VerificationException : Exception
    {
        internal bool CanRetry { get; }

        public VerificationException(string message, bool canRetry)
            : base(message)
        {
            CanRetry = canRetry;
        }
    }
}
