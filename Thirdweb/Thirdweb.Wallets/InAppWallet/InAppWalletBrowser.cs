using System.Net;

namespace Thirdweb
{
    /// <summary>
    /// Represents an in-app browser for handling wallet login.
    /// </summary>
    public class InAppWalletBrowser : IThirdwebBrowser
    {
        private TaskCompletionSource<BrowserResult> _taskCompletionSource;
        private static readonly HttpListener httpListener = new();

        private readonly string closePageResponse =
            @"
            <html>
            <head>
                <style>
                    body {
                        font-family: Arial, sans-serif;
                        background-color: #2c2c2c;
                        color: #ffffff;
                        display: flex;
                        justify-content: center;
                        align-items: center;
                        height: 100vh;
                        flex-direction: column;
                    }
                    .container {
                        background-color: #3c3c3c;
                        padding: 20px;
                        border-radius: 10px;
                        box-shadow: 0 0 10px rgba(0,0,0,0.3);
                        text-align: center;
                    }
                    .instruction {
                        margin-top: 20px;
                        font-size: 18px;
                    }
                </style>
            </head>
            <body>
                <div class='container'>
                    <b>DONE!</b>
                    <div class='instruction'>
                        You can close this tab/window now.
                    </div>
                </div>
            </body>
            </html>";

        /// <summary>
        /// Initiates a login process using the in-app browser.
        /// </summary>
        /// <param name="client">The Thirdweb client instance.</param>
        /// <param name="loginUrl">The URL to initiate the login process.</param>
        /// <param name="redirectUrl">The URL to redirect to after login.</param>
        /// <param name="browserOpenAction">An action to open the browser with the login URL.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the login result.</returns>
        public async Task<BrowserResult> Login(ThirdwebClient client, string loginUrl, string redirectUrl, Action<string> browserOpenAction, CancellationToken cancellationToken = default)
        {
            _taskCompletionSource = new TaskCompletionSource<BrowserResult>();

            cancellationToken.Register(() =>
            {
                _taskCompletionSource?.TrySetCanceled();
                StopHttpListener();
            });

            try
            {
                redirectUrl = AddForwardSlashIfNecessary(redirectUrl);
                if (httpListener.Prefixes.Count == 0 || !httpListener.Prefixes.Contains(redirectUrl))
                {
                    httpListener.Prefixes.Clear();
                    httpListener.Prefixes.Add(redirectUrl);
                }
                httpListener.Start();
                _ = httpListener.BeginGetContext(IncomingHttpRequest, httpListener);

                browserOpenAction.Invoke(loginUrl);

                var completedTask = await Task.WhenAny(_taskCompletionSource.Task, Task.Delay(TimeSpan.FromSeconds(120), cancellationToken));
                return completedTask == _taskCompletionSource.Task ? await _taskCompletionSource.Task : new BrowserResult(BrowserStatus.Timeout, null, "The operation timed out.");
            }
            catch (TaskCanceledException)
            {
                return new BrowserResult(BrowserStatus.UserCanceled, null, "The operation was cancelled.");
            }
            catch (Exception ex)
            {
                return new BrowserResult(BrowserStatus.UnknownError, null, $"An error occurred: {ex.Message}");
            }
            finally
            {
                StopHttpListener();
            }
        }

        /// <summary>
        /// Stops the HTTP listener.
        /// </summary>
        private void StopHttpListener()
        {
            if (httpListener != null && httpListener.IsListening)
            {
                httpListener.Stop();
            }
        }

        /// <summary>
        /// Handles incoming HTTP requests.
        /// </summary>
        /// <param name="result">The result of the asynchronous operation.</param>
        private void IncomingHttpRequest(IAsyncResult result)
        {
            var httpListener = (HttpListener)result.AsyncState;
            if (!httpListener.IsListening)
            {
                return;
            }

            var httpContext = httpListener.EndGetContext(result);
            var httpRequest = httpContext.Request;
            var httpResponse = httpContext.Response;
            var buffer = System.Text.Encoding.UTF8.GetBytes(closePageResponse);

            httpResponse.ContentLength64 = buffer.Length;
            var output = httpResponse.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();

            _taskCompletionSource.SetResult(new BrowserResult(BrowserStatus.Success, httpRequest.Url.ToString()));
        }

        /// <summary>
        /// Adds a forward slash to the URL if necessary.
        /// </summary>
        /// <param name="url">The URL to check.</param>
        /// <returns>The URL with a forward slash added if necessary.</returns>
        private string AddForwardSlashIfNecessary(string url)
        {
            string forwardSlash = "/";
            if (!url.EndsWith(forwardSlash))
            {
                url += forwardSlash;
            }
            return url;
        }
    }
}
