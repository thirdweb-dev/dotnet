using System.Net;

namespace Thirdweb;

/// <summary>
/// Represents an in-app browser for handling wallet login.
/// </summary>
public class InAppWalletBrowser : IThirdwebBrowser
{
    private TaskCompletionSource<BrowserResult> _taskCompletionSource;
    private static readonly HttpListener _httpListener = new();

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
        this._taskCompletionSource = new TaskCompletionSource<BrowserResult>();

        _ = cancellationToken.Register(() =>
        {
            _ = (this._taskCompletionSource?.TrySetCanceled());

            StopHttpListener();
        });

        try
        {
            redirectUrl = AddForwardSlashIfNecessary(redirectUrl);
            if (_httpListener.Prefixes.Count == 0 || !_httpListener.Prefixes.Contains(redirectUrl))
            {
                _httpListener.Prefixes.Clear();
                _httpListener.Prefixes.Add(redirectUrl);
            }
            _httpListener.Start();
            _ = _httpListener.BeginGetContext(this.IncomingHttpRequest, _httpListener);

            browserOpenAction.Invoke(loginUrl);

            var completedTask = await Task.WhenAny(this._taskCompletionSource.Task, Task.Delay(TimeSpan.FromSeconds(120), cancellationToken));
            return completedTask == this._taskCompletionSource.Task ? await this._taskCompletionSource.Task : new BrowserResult(BrowserStatus.Timeout, null, "The operation timed out.");
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
    private static void StopHttpListener()
    {
        if (_httpListener != null && _httpListener.IsListening)
        {
            _httpListener.Stop();
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
        var buffer = System.Text.Encoding.UTF8.GetBytes(Constants.REDIRECT_HTML);

        httpResponse.ContentLength64 = buffer.Length;
        var output = httpResponse.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();

        this._taskCompletionSource.SetResult(new BrowserResult(BrowserStatus.Success, httpRequest.Url.ToString()));
    }

    /// <summary>
    /// Adds a forward slash to the URL if necessary.
    /// </summary>
    /// <param name="url">The URL to check.</param>
    /// <returns>The URL with a forward slash added if necessary.</returns>
    private static string AddForwardSlashIfNecessary(string url)
    {
        var forwardSlash = "/";
        if (!url.EndsWith(forwardSlash))
        {
            url += forwardSlash;
        }
        return url;
    }
}
