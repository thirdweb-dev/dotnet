namespace Thirdweb
{
    /// <summary>
    /// Defines an interface for handling browser-based login for Thirdweb.
    /// </summary>
    public interface IThirdwebBrowser
    {
        /// <summary>
        /// Initiates a login process using the browser.
        /// </summary>
        /// <param name="client">The Thirdweb client instance.</param>
        /// <param name="loginUrl">The URL to initiate the login process.</param>
        /// <param name="redirectUrl">The URL to redirect to after login.</param>
        /// <param name="browserOpenAction">An action to open the browser with the login URL.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the login result.</returns>
        Task<BrowserResult> Login(ThirdwebClient client, string loginUrl, string redirectUrl, Action<string> browserOpenAction, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Enumerates the possible statuses of a browser operation.
    /// </summary>
    public enum BrowserStatus
    {
        /// <summary>
        /// The operation was successful.
        /// </summary>
        Success,

        /// <summary>
        /// The user canceled the operation.
        /// </summary>
        UserCanceled,

        /// <summary>
        /// The operation timed out.
        /// </summary>
        Timeout,

        /// <summary>
        /// An unknown error occurred during the operation.
        /// </summary>
        UnknownError,
    }

    /// <summary>
    /// Represents the result of a browser-based login operation.
    /// </summary>
    public class BrowserResult
    {
        /// <summary>
        /// Gets the status of the browser operation.
        /// </summary>
        public BrowserStatus status { get; }

        /// <summary>
        /// Gets the callback URL returned from the browser operation.
        /// </summary>
        public string callbackUrl { get; }

        /// <summary>
        /// Gets the error message, if any, from the browser operation.
        /// </summary>
        public string error { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserResult"/> class with the specified status and callback URL.
        /// </summary>
        /// <param name="status">The status of the browser operation.</param>
        /// <param name="callbackUrl">The callback URL returned from the browser operation.</param>
        public BrowserResult(BrowserStatus status, string callbackUrl)
        {
            this.status = status;
            this.callbackUrl = callbackUrl;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserResult"/> class with the specified status, callback URL, and error message.
        /// </summary>
        /// <param name="status">The status of the browser operation.</param>
        /// <param name="callbackUrl">The callback URL returned from the browser operation.</param>
        /// <param name="error">The error message from the browser operation.</param>
        public BrowserResult(BrowserStatus status, string callbackUrl, string error)
        {
            this.status = status;
            this.callbackUrl = callbackUrl;
            this.error = error;
        }
    }
}
