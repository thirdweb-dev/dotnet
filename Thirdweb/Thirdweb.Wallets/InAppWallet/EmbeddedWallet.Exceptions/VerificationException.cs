namespace Thirdweb.EWS;

internal class VerificationException
(bool canRetry) : Exception
{
    internal bool CanRetry { get; } = canRetry;
}
