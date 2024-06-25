namespace Thirdweb
{
    public interface IThirdwebBrowser
    {
        Task<BrowserResult> Login(ThirdwebClient client, string loginUrl, string redirectUrl, Action<string> browserOpenAction, CancellationToken cancellationToken = default);
    }

    public enum BrowserStatus
    {
        Success,
        UserCanceled,
        Timeout,
        UnknownError,
    }

    public class BrowserResult
    {
        public BrowserStatus status { get; }

        public string callbackUrl { get; }

        public string error { get; }

        public BrowserResult(BrowserStatus status, string callbackUrl)
        {
            this.status = status;
            this.callbackUrl = callbackUrl;
        }

        public BrowserResult(BrowserStatus status, string callbackUrl, string error)
        {
            this.status = status;
            this.callbackUrl = callbackUrl;
            this.error = error;
        }
    }
}
