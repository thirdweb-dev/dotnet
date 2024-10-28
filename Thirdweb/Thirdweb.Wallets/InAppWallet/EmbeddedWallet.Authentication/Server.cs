using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Thirdweb.EWS;

internal abstract class ServerBase
{
    internal abstract Task<List<Server.LinkedAccount>> LinkAccountAsync(string currentAccountToken, string authTokenToConnect);
    internal abstract Task<List<Server.LinkedAccount>> GetLinkedAccountsAsync(string currentAccountToken);

    internal abstract Task<(string authShare, string recoveryShare)> FetchAuthAndRecoverySharesAsync(string authToken);
    internal abstract Task<string> FetchAuthShareAsync(string authToken);

    internal abstract Task<LoginPayloadData> FetchSiwePayloadAsync(string address, string chainId);
    internal abstract Task<Server.VerifyResult> VerifySiweAsync(LoginPayloadData payload, string signature);

    internal abstract Task<Server.VerifyResult> VerifyGuestAsync(string sessionId);

    internal abstract Task<string> SendEmailOtpAsync(string emailAddress);
    internal abstract Task<Server.VerifyResult> VerifyEmailOtpAsync(string emailAddress, string otp);

    internal abstract Task<string> SendPhoneOtpAsync(string phoneNumber);
    internal abstract Task<Server.VerifyResult> VerifyPhoneOtpAsync(string phoneNumber, string otp);

    internal abstract Task<Server.VerifyResult> VerifyJwtAsync(string jwtToken);

    internal abstract Task<string> FetchHeadlessOauthLoginLinkAsync(string authProvider, string platform);
    internal abstract Task<Server.VerifyResult> VerifyOAuthAsync(string authResultStr);

    internal abstract Task<Server.VerifyResult> VerifyAuthEndpointAsync(string payload);

    internal abstract Task<JToken> GenerateEncryptedKeyResultAsync(string authToken);
}

internal partial class Server : ServerBase
{
    private const string ROOT_URL = "https://embedded-wallet.thirdweb.com";
    private const string API_ROOT_PATH_2024 = "/api/2024-05-05";
    private const string API_ROOT_PATH_2023 = "/api/2023-10-20";

    private static readonly MediaTypeHeaderValue _jsonContentType = MediaTypeHeaderValue.Parse("application/json");
    private readonly IThirdwebHttpClient _httpClient;

    private readonly string _clientId;

    internal Server(ThirdwebClient client, IThirdwebHttpClient httpClient)
    {
        this._clientId = client.ClientId;
        this._httpClient = httpClient;
    }

    // account/connect
    internal override async Task<List<LinkedAccount>> LinkAccountAsync(string currentAccountToken, string authTokenToConnect)
    {
        var uri = MakeUri2024("/account/connect");
        var request = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = MakeHttpContent(new { accountAuthTokenToConnect = authTokenToConnect })
        };
        var response = await this.SendHttpWithAuthAsync(request, currentAccountToken).ConfigureAwait(false);
        await CheckStatusCodeAsync(response).ConfigureAwait(false);

        var res = await DeserializeAsync<AccountConnectResponse>(response).ConfigureAwait(false);
        return res == null || res.LinkedAccounts == null || res.LinkedAccounts.Count == 0 ? throw new InvalidOperationException("No linked accounts returned") : res.LinkedAccounts;
    }

    // accounts GET
    internal override async Task<List<LinkedAccount>> GetLinkedAccountsAsync(string currentAccountToken)
    {
        var uri = MakeUri2024("/accounts");
        var response = await this.SendHttpWithAuthAsync(uri, currentAccountToken).ConfigureAwait(false);
        await CheckStatusCodeAsync(response).ConfigureAwait(false);

        var res = await DeserializeAsync<AccountConnectResponse>(response).ConfigureAwait(false);
        return res == null || res.LinkedAccounts == null || res.LinkedAccounts.Count == 0 ? [] : res.LinkedAccounts;
    }

    // embedded-wallet/embedded-wallet-shares GET
    internal override async Task<(string authShare, string recoveryShare)> FetchAuthAndRecoverySharesAsync(string authToken)
    {
        var sharesGetResponse = await this.FetchRemoteSharesAsync(authToken, true).ConfigureAwait(false);
        var authShare = sharesGetResponse.AuthShare ?? throw new InvalidOperationException("Server failed to return auth share");
        var encryptedRecoveryShare = sharesGetResponse.MaybeEncryptedRecoveryShares?.FirstOrDefault() ?? throw new InvalidOperationException("Server failed to return recovery share");
        return (authShare, encryptedRecoveryShare);
    }

    // embedded-wallet/embedded-wallet-shares GET
    internal override async Task<string> FetchAuthShareAsync(string authToken)
    {
        var sharesGetResponse = await this.FetchRemoteSharesAsync(authToken, false).ConfigureAwait(false);
        return sharesGetResponse.AuthShare ?? throw new InvalidOperationException("Server failed to return auth share");
    }

    // embedded-wallet/embedded-wallet-shares GET
    private async Task<SharesGetResponse> FetchRemoteSharesAsync(string authToken, bool wantsRecoveryShare)
    {
        Dictionary<string, string> queryParams =
            new()
            {
                { "getEncryptedAuthShare", "true" },
                { "getEncryptedRecoveryShare", wantsRecoveryShare ? "true" : "false" },
                { "useSealedSecret", "false" }
            };
        var uri = MakeUri2023("/embedded-wallet/embedded-wallet-shares", queryParams);
        var response = await this.SendHttpWithAuthAsync(uri, authToken).ConfigureAwait(false);
        await CheckStatusCodeAsync(response).ConfigureAwait(false);
        var rv = await DeserializeAsync<SharesGetResponse>(response).ConfigureAwait(false);
        return rv;
    }

    // login/web-token-exchange
    private async Task<IdTokenResponse> FetchCognitoIdTokenAsync(string authToken)
    {
        var uri = MakeUri2024("/login/web-token-exchange");
        var response = await this.SendHttpWithAuthAsync(uri, authToken).ConfigureAwait(false);
        await CheckStatusCodeAsync(response).ConfigureAwait(false);
        return await DeserializeAsync<IdTokenResponse>(response).ConfigureAwait(false);
    }

    // login/siwe
    internal override async Task<LoginPayloadData> FetchSiwePayloadAsync(string address, string chainId)
    {
        var uri = MakeUri2024("/login/siwe", new Dictionary<string, string> { { "address", address }, { "chainId", chainId } });
        var response = await this._httpClient.GetAsync(uri.ToString()).ConfigureAwait(false);
        await CheckStatusCodeAsync(response).ConfigureAwait(false);

        return await DeserializeAsync<LoginPayloadData>(response).ConfigureAwait(false);
    }

    internal override async Task<VerifyResult> VerifySiweAsync(LoginPayloadData payload, string signature)
    {
        var uri = MakeUri2024("/login/siwe/callback");
        var content = MakeHttpContent(new { signature, payload });
        var response = await this._httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
        await CheckStatusCodeAsync(response).ConfigureAwait(false);

        var authResult = await DeserializeAsync<AuthResultType>(response).ConfigureAwait(false);
        return await this.InvokeAuthResultLambdaAsync(authResult).ConfigureAwait(false);
    }

    // login/guest

    internal override async Task<VerifyResult> VerifyGuestAsync(string sessionId)
    {
        var uri = MakeUri2024("/login/guest/callback");
        var content = MakeHttpContent(new { sessionId });
        var response = await this._httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
        await CheckStatusCodeAsync(response).ConfigureAwait(false);

        var authResult = await DeserializeAsync<AuthResultType>(response).ConfigureAwait(false);
        authResult.StoredToken.AuthDetails.AuthIdentifier = sessionId;
        return await this.InvokeAuthResultLambdaAsync(authResult).ConfigureAwait(false);
    }

    // login/oauthprovider
    internal override Task<string> FetchHeadlessOauthLoginLinkAsync(string authProvider, string platform)
    {
        return Task.FromResult(MakeUri2024($"/login/{authProvider}", new Dictionary<string, string> { { "clientId", this._clientId }, { "platform", platform } }).ToString());
    }

    // login/email
    internal override async Task<string> SendEmailOtpAsync(string emailAddress)
    {
        var uri = MakeUri2024("/login/email");
        var content = MakeHttpContent(new { email = emailAddress });
        var response = await this._httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
        await CheckStatusCodeAsync(response).ConfigureAwait(false);

        var result = await DeserializeAsync<SendEmailOtpReturnType>(response).ConfigureAwait(false);
        return result.Email;
    }

    // login/email/callback
    internal override async Task<VerifyResult> VerifyEmailOtpAsync(string emailAddress, string otp)
    {
        var uri = MakeUri2024("/login/email/callback");
        var content = MakeHttpContent(new { email = emailAddress, code = otp });
        var response = await this._httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
        await CheckStatusCodeAsync(response).ConfigureAwait(false);

        var authResult = await DeserializeAsync<AuthResultType>(response).ConfigureAwait(false);
        return await this.InvokeAuthResultLambdaAsync(authResult).ConfigureAwait(false);
    }

    // login/phone
    internal override async Task<string> SendPhoneOtpAsync(string phoneNumber)
    {
        var uri = MakeUri2024("/login/phone");
        var content = MakeHttpContent(new { phone = phoneNumber });
        var response = await this._httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
        await CheckStatusCodeAsync(response).ConfigureAwait(false);

        var result = await DeserializeAsync<SendPhoneOtpReturnType>(response).ConfigureAwait(false);
        return result.Phone;
    }

    // login/phone/callback
    internal override async Task<VerifyResult> VerifyPhoneOtpAsync(string phoneNumber, string otp)
    {
        var uri = MakeUri2024("/login/phone/callback");
        var content = MakeHttpContent(new { phone = phoneNumber, code = otp });
        var response = await this._httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
        await CheckStatusCodeAsync(response).ConfigureAwait(false);

        var authResult = await DeserializeAsync<AuthResultType>(response).ConfigureAwait(false);
        return await this.InvokeAuthResultLambdaAsync(authResult).ConfigureAwait(false);
    }

    // embedded-wallet/validate-custom-jwt
    internal override async Task<VerifyResult> VerifyJwtAsync(string jwtToken)
    {
        var requestContent = new { jwt = jwtToken, developerClientId = this._clientId };
        var content = MakeHttpContent(requestContent);
        var uri = MakeUri2023("/embedded-wallet/validate-custom-jwt");
        var response = await this._httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
        await CheckStatusCodeAsync(response).ConfigureAwait(false);

        var authVerifiedToken = await DeserializeAsync<AuthVerifiedTokenReturnType>(response).ConfigureAwait(false);
        return new VerifyResult(
            authVerifiedToken.VerifiedToken.AuthProvider,
            authVerifiedToken.VerifiedToken.IsNewUser,
            authVerifiedToken.VerifiedTokenJwtString,
            authVerifiedToken.VerifiedToken.AuthDetails.UserWalletId,
            authVerifiedToken.VerifiedToken.AuthDetails.RecoveryCode,
            authVerifiedToken.VerifiedToken.AuthDetails.Email,
            authVerifiedToken.VerifiedToken.AuthDetails.PhoneNumber,
            authVerifiedToken.VerifiedToken.AuthDetails.AuthIdentifier
        );
    }

    // embedded-wallet/validate-custom-auth-endpoint
    internal override async Task<VerifyResult> VerifyAuthEndpointAsync(string payload)
    {
        var requestContent = new { payload, developerClientId = this._clientId };
        var content = MakeHttpContent(requestContent);
        var uri = MakeUri2023("/embedded-wallet/validate-custom-auth-endpoint");
        var response = await this._httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
        await CheckStatusCodeAsync(response).ConfigureAwait(false);

        var authVerifiedToken = await DeserializeAsync<AuthVerifiedTokenReturnType>(response).ConfigureAwait(false);
        return new VerifyResult(
            authVerifiedToken.VerifiedToken.AuthProvider,
            authVerifiedToken.VerifiedToken.IsNewUser,
            authVerifiedToken.VerifiedTokenJwtString,
            authVerifiedToken.VerifiedToken.AuthDetails.UserWalletId,
            authVerifiedToken.VerifiedToken.AuthDetails.RecoveryCode,
            authVerifiedToken.VerifiedToken.AuthDetails.Email,
            authVerifiedToken.VerifiedToken.AuthDetails.PhoneNumber,
            authVerifiedToken.VerifiedToken.AuthDetails.AuthIdentifier
        );
    }

    internal override async Task<VerifyResult> VerifyOAuthAsync(string authResultStr)
    {
        var authResult = JsonConvert.DeserializeObject<AuthResultType>(authResultStr);
        return await this.InvokeAuthResultLambdaAsync(authResult).ConfigureAwait(false);
    }

    #region Misc

    internal override async Task<JToken> GenerateEncryptedKeyResultAsync(string authToken)
    {
        var webExchangeResult = await this.FetchCognitoIdTokenAsync(authToken).ConfigureAwait(false);
        return await AWS.GenerateDataKey(webExchangeResult.IdentityId, webExchangeResult.Token, this._httpClient).ConfigureAwait(false);
    }

    private async Task<VerifyResult> InvokeAuthResultLambdaAsync(AuthResultType authResult)
    {
        var authToken = authResult.StoredToken.CookieString;
        var idTokenResponse = await this.FetchCognitoIdTokenAsync(authToken).ConfigureAwait(false);

        var invokePayload = Serialize(new { token = idTokenResponse.LambdaToken });
        var responsePayload = await AWS.InvokeRecoverySharePasswordLambdaAsync(idTokenResponse.IdentityId, idTokenResponse.Token, invokePayload, this._httpClient).ConfigureAwait(false);

        var jsonSerializer = new JsonSerializer();
        var payload = jsonSerializer.Deserialize<RecoverySharePasswordResponse>(new JsonTextReader(new StreamReader(responsePayload)));
        payload = jsonSerializer.Deserialize<RecoverySharePasswordResponse>(new JsonTextReader(new StringReader(payload.Body)));
        return new VerifyResult(
            authResult.StoredToken.AuthProvider,
            authResult.StoredToken.IsNewUser,
            authToken,
            authResult.StoredToken.AuthDetails.UserWalletId,
            payload.RecoverySharePassword,
            authResult.StoredToken.AuthDetails.Email,
            authResult.StoredToken.AuthDetails.PhoneNumber,
            authResult.StoredToken.AuthDetails.AuthIdentifier
        );
    }

    private async Task<ThirdwebHttpResponseMessage> SendHttpWithAuthAsync(HttpRequestMessage httpRequestMessage, string authToken)
    {
        this._httpClient.AddHeader("Authorization", $"Bearer embedded-wallet-token:{authToken}");

        try
        {
            if (httpRequestMessage.Method == HttpMethod.Get)
            {
                return await this._httpClient.GetAsync(httpRequestMessage.RequestUri.ToString()).ConfigureAwait(false);
            }
            else if (httpRequestMessage.Method == HttpMethod.Post)
            {
                return await this._httpClient.PostAsync(httpRequestMessage.RequestUri.ToString(), httpRequestMessage.Content).ConfigureAwait(false);
            }
            else if (httpRequestMessage.Method == HttpMethod.Put)
            {
                return await this._httpClient.PutAsync(httpRequestMessage.RequestUri.ToString(), httpRequestMessage.Content).ConfigureAwait(false);
            }
            else
            {
                return httpRequestMessage.Method == HttpMethod.Delete
                    ? await this._httpClient.DeleteAsync(httpRequestMessage.RequestUri.ToString()).ConfigureAwait(false)
                    : throw new InvalidOperationException("Unsupported HTTP method");
            }
        }
        finally
        {
            this._httpClient.RemoveHeader("Authorization");
        }
    }

    private async Task<ThirdwebHttpResponseMessage> SendHttpWithAuthAsync(Uri uri, string authToken)
    {
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, uri);
        return await this.SendHttpWithAuthAsync(httpRequestMessage, authToken).ConfigureAwait(false);
    }

    private static async Task CheckStatusCodeAsync(ThirdwebHttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await DeserializeAsync<HttpErrorWithMessage>(response).ConfigureAwait(false);
            throw new InvalidOperationException(string.IsNullOrEmpty(error.Error) ? error.Message : error.Error);
        }
    }

    private static async Task<T> DeserializeAsync<T>(ThirdwebHttpResponseMessage response)
    {
        JsonSerializer jsonSerializer = new();
        TextReader textReader = new StreamReader(await response.Content.ReadAsStreamAsync().ConfigureAwait(false));
        var rv = jsonSerializer.Deserialize<T>(new JsonTextReader(textReader));
        return rv;
    }

    private static Uri MakeUri2024(string path, IDictionary<string, string> parameters = null)
    {
        UriBuilder b = new(ROOT_URL) { Path = API_ROOT_PATH_2024 + path, };
        if (parameters != null && parameters.Any())
        {
            var queryString = string.Join('&', parameters.Select((p) => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
            b.Query = queryString;
        }
        return b.Uri;
    }

    private static Uri MakeUri2023(string path, IDictionary<string, string> parameters = null)
    {
        UriBuilder b = new(ROOT_URL) { Path = API_ROOT_PATH_2023 + path, };
        if (parameters != null && parameters.Any())
        {
            var queryString = string.Join('&', parameters.Select((p) => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
            b.Query = queryString;
        }
        return b.Uri;
    }

    private static StringContent MakeHttpContent(object data)
    {
        StringContent stringContent = new(Serialize(data));
        stringContent.Headers.ContentType = _jsonContentType;
        return stringContent;
    }

    private static string Serialize(object data)
    {
        JsonSerializer jsonSerializer = new() { NullValueHandling = NullValueHandling.Ignore, };
        StringWriter stringWriter = new();
        jsonSerializer.Serialize(stringWriter, data);
        var rv = stringWriter.ToString();

        return rv;
    }

    #endregion
}
