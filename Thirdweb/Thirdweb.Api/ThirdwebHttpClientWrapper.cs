namespace Thirdweb.Api;

/// <summary>
/// Wrapper class that adapts IThirdwebHttpClient to work with System.Net.Http.HttpClient expectations
/// </summary>
public class ThirdwebHttpClientWrapper : HttpClient
{
    private readonly IThirdwebHttpClient _thirdwebClient;

    public ThirdwebHttpClientWrapper(IThirdwebHttpClient thirdwebClient)
    {
        this._thirdwebClient = thirdwebClient ?? throw new ArgumentNullException(nameof(thirdwebClient));
    }

    public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return await this.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);
    }

    public new async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
    {
        _ = completionOption;

        var content = request.Content;

        ThirdwebHttpResponseMessage thirdwebResponse;

        switch (request.Method.Method.ToUpperInvariant())
        {
            case "GET":
                thirdwebResponse = await this._thirdwebClient.GetAsync(request.RequestUri.ToString(), cancellationToken);
                break;
            case "POST":
                thirdwebResponse = await this._thirdwebClient.PostAsync(request.RequestUri.ToString(), content, cancellationToken);
                break;
            case "PUT":
                thirdwebResponse = await this._thirdwebClient.PutAsync(request.RequestUri.ToString(), content, cancellationToken);
                break;
            case "DELETE":
                thirdwebResponse = await this._thirdwebClient.DeleteAsync(request.RequestUri.ToString(), cancellationToken);
                break;
            default:
                throw new NotSupportedException($"HTTP method {request.Method} is not supported");
        }

        var response = new HttpResponseMessage((System.Net.HttpStatusCode)thirdwebResponse.StatusCode) { Content = new StringContent(await thirdwebResponse.Content.ReadAsStringAsync()) };

        return response;
    }
}
