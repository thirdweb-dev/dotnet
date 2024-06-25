using System.Net;

namespace Thirdweb
{
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

                var completedTask = await Task.WhenAny(_taskCompletionSource.Task, Task.Delay(TimeSpan.FromSeconds(60), cancellationToken));
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

        private void StopHttpListener()
        {
            if (httpListener != null && httpListener.IsListening)
            {
                httpListener.Stop();
                httpListener.Close();
            }
        }

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
