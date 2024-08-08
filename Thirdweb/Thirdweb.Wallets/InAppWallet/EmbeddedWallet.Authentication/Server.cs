using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Thirdweb.EWS
{
    internal abstract class ServerBase
    {
        internal abstract Task<string> VerifyThirdwebClientIdAsync(string domain);
        internal abstract Task<Server.UserWallet> FetchUserDetailsAsync(string emailAddress, string authToken);
        internal abstract Task StoreAddressAndSharesAsync(string walletAddress, string authShare, string encryptedRecoveryShare, string authToken);

        internal abstract Task<(string authShare, string recoveryShare)> FetchAuthAndRecoverySharesAsync(string authToken);
        internal abstract Task<string> FetchAuthShareAsync(string authToken);

        internal abstract Task<string> FetchHeadlessOauthLoginLinkAsync(string authProvider, string platform);

        internal abstract Task<string> SendEmailOtpAsync(string emailAddress);
        internal abstract Task<Server.VerifyResult> VerifyEmailOtpAsync(string emailAddress, string otp);

        internal abstract Task<string> SendPhoneOtpAsync(string phoneNumber);
        internal abstract Task<Server.VerifyResult> VerifyPhoneOtpAsync(string phoneNumber, string otp);

        internal abstract Task<Server.VerifyResult> VerifyJwtAsync(string jwtToken);

        internal abstract Task<Server.VerifyResult> VerifyOAuthAsync(string authResultStr);

        internal abstract Task<Server.VerifyResult> VerifyAuthEndpointAsync(string payload);
    }

    internal partial class Server : ServerBase
    {
        private const string ROOT_URL = "https://embedded-wallet.thirdweb.com";
        private const string API_ROOT_PATH_2024 = "/api/2024-05-05";
        private const string API_ROOT_PATH_2023 = "/api/2023-10-20";

        private static readonly MediaTypeHeaderValue jsonContentType = MediaTypeHeaderValue.Parse("application/json");
        private readonly IThirdwebHttpClient httpClient;

        private readonly string clientId;

        private static Type thirdwebHttpClientType = typeof(ThirdwebHttpClient);

        internal Server(ThirdwebClient client, IThirdwebHttpClient httpClient)
        {
            this.clientId = client.ClientId;
            this.httpClient = httpClient;

            thirdwebHttpClientType = httpClient.GetType();
        }

        // embedded-wallet/verify-thirdweb-client-id
        internal override async Task<string> VerifyThirdwebClientIdAsync(string parentDomain)
        {
            Dictionary<string, string> queryParams = new() { { "clientId", clientId }, { "parentDomain", parentDomain } };
            var uri = MakeUri2023("/embedded-wallet/verify-thirdweb-client-id", queryParams);
            var content = MakeHttpContent(new { clientId, parentDomain });
            var response = await httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);
            var error = await DeserializeAsync<HttpErrorWithMessage>(response).ConfigureAwait(false);
            return error.Error;
        }

        // embedded-wallet/embedded-wallet-user-details
        internal override async Task<UserWallet> FetchUserDetailsAsync(string emailAddress, string authToken)
        {
            Dictionary<string, string> queryParams = new();
            if (emailAddress == null && authToken == null)
            {
                throw new InvalidOperationException("Must provide either email address or auth token");
            }

            queryParams.Add("email", emailAddress ?? "uninitialized");
            queryParams.Add("clientId", clientId);

            var uri = MakeUri2023("/embedded-wallet/embedded-wallet-user-details", queryParams);
            var response = await SendHttpWithAuthAsync(uri, authToken ?? "").ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);
            var rv = await DeserializeAsync<UserWallet>(response).ConfigureAwait(false);
            return rv;
        }

        // embedded-wallet/embedded-wallet-shares POST
        internal override async Task StoreAddressAndSharesAsync(string walletAddress, string authShare, string encryptedRecoveryShare, string authToken)
        {
            var encryptedRecoveryShares = new[] { new { share = encryptedRecoveryShare, isClientEncrypted = "true" } };

            HttpRequestMessage httpRequestMessage =
                new(HttpMethod.Post, MakeUri2023("/embedded-wallet/embedded-wallet-shares"))
                {
                    Content = MakeHttpContent(
                        new
                        {
                            authShare,
                            maybeEncryptedRecoveryShares = encryptedRecoveryShares,
                            walletAddress,
                        }
                    ),
                };
            var response = await SendHttpWithAuthAsync(httpRequestMessage, authToken).ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);
        }

        // embedded-wallet/embedded-wallet-shares GET
        internal override async Task<(string authShare, string recoveryShare)> FetchAuthAndRecoverySharesAsync(string authToken)
        {
            var sharesGetResponse = await FetchRemoteSharesAsync(authToken, true).ConfigureAwait(false);
            var authShare = sharesGetResponse.AuthShare ?? throw new InvalidOperationException("Server failed to return auth share");
            var encryptedRecoveryShare = sharesGetResponse.MaybeEncryptedRecoveryShares?.FirstOrDefault() ?? throw new InvalidOperationException("Server failed to return recovery share");
            return (authShare, encryptedRecoveryShare);
        }

        // embedded-wallet/embedded-wallet-shares GET
        internal override async Task<string> FetchAuthShareAsync(string authToken)
        {
            var sharesGetResponse = await FetchRemoteSharesAsync(authToken, false).ConfigureAwait(false);
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
            var response = await SendHttpWithAuthAsync(uri, authToken).ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);
            var rv = await DeserializeAsync<SharesGetResponse>(response).ConfigureAwait(false);
            return rv;
        }

        // login/web-token-exchange
        private async Task<IdTokenResponse> FetchCognitoIdTokenAsync(string authToken)
        {
            var uri = MakeUri2024("/login/web-token-exchange");
            var response = await SendHttpWithAuthAsync(uri, authToken).ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);
            return await DeserializeAsync<IdTokenResponse>(response).ConfigureAwait(false);
        }

        // embedded-wallet/headless-oauth-login-link
        internal override Task<string> FetchHeadlessOauthLoginLinkAsync(string authProvider, string platform)
        {
            return Task.FromResult(MakeUri2024($"/login/{authProvider}", new Dictionary<string, string> { { "clientId", clientId }, { "platform", platform } }).ToString());
        }

        // login/email
        internal override async Task<string> SendEmailOtpAsync(string emailAddress)
        {
            var uri = MakeUri2024("/login/email");
            var content = MakeHttpContent(new { email = emailAddress });
            var response = await httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);

            var result = await DeserializeAsync<SendEmailOtpReturnType>(response).ConfigureAwait(false);
            return result.Email;
        }

        // login/email/callback
        internal override async Task<VerifyResult> VerifyEmailOtpAsync(string emailAddress, string otp)
        {
            var uri = MakeUri2024("/login/email/callback");
            var content = MakeHttpContent(new { email = emailAddress, code = otp });
            var response = await httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);

            var authResult = await DeserializeAsync<AuthResultType>(response).ConfigureAwait(false);
            return await InvokeAuthResultLambdaAsync(authResult).ConfigureAwait(false);
        }

        // login/phone
        internal override async Task<string> SendPhoneOtpAsync(string phoneNumber)
        {
            var uri = MakeUri2024("/login/phone");
            var content = MakeHttpContent(new { phone = phoneNumber });
            var response = await httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);

            var result = await DeserializeAsync<SendPhoneOtpReturnType>(response).ConfigureAwait(false);
            return result.Phone;
        }

        // login/phone/callback
        internal override async Task<VerifyResult> VerifyPhoneOtpAsync(string phoneNumber, string otp)
        {
            var uri = MakeUri2024("/login/phone/callback");
            var content = MakeHttpContent(new { phone = phoneNumber, code = otp });
            var response = await httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);

            var authResult = await DeserializeAsync<AuthResultType>(response).ConfigureAwait(false);
            return await InvokeAuthResultLambdaAsync(authResult).ConfigureAwait(false);
        }

        // embedded-wallet/validate-custom-jwt
        internal override async Task<VerifyResult> VerifyJwtAsync(string jwtToken)
        {
            var requestContent = new { jwt = jwtToken, developerClientId = clientId };
            var content = MakeHttpContent(requestContent);
            var uri = MakeUri2023("/embedded-wallet/validate-custom-jwt");
            var response = await httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);

            var authVerifiedToken = await DeserializeAsync<AuthVerifiedTokenReturnType>(response).ConfigureAwait(false);
            return new VerifyResult(
                authVerifiedToken.VerifiedToken.IsNewUser,
                authVerifiedToken.VerifiedTokenJwtString,
                authVerifiedToken.VerifiedToken.AuthDetails.UserWalletId,
                authVerifiedToken.VerifiedToken.AuthDetails.RecoveryCode,
                authVerifiedToken.VerifiedToken.AuthDetails.Email,
                authVerifiedToken.VerifiedToken.AuthDetails.PhoneNumber
            );
        }

        // embedded-wallet/validate-custom-auth-endpoint
        internal override async Task<VerifyResult> VerifyAuthEndpointAsync(string payload)
        {
            var requestContent = new { payload, developerClientId = clientId };
            var content = MakeHttpContent(requestContent);
            var uri = MakeUri2023("/embedded-wallet/validate-custom-auth-endpoint");
            var response = await httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);

            var authVerifiedToken = await DeserializeAsync<AuthVerifiedTokenReturnType>(response).ConfigureAwait(false);
            return new VerifyResult(
                authVerifiedToken.VerifiedToken.IsNewUser,
                authVerifiedToken.VerifiedTokenJwtString,
                authVerifiedToken.VerifiedToken.AuthDetails.UserWalletId,
                authVerifiedToken.VerifiedToken.AuthDetails.RecoveryCode,
                authVerifiedToken.VerifiedToken.AuthDetails.Email,
                authVerifiedToken.VerifiedToken.AuthDetails.PhoneNumber
            );
        }

        internal override async Task<VerifyResult> VerifyOAuthAsync(string authResultStr)
        {
            var authResult = JsonConvert.DeserializeObject<AuthResultType>(authResultStr);
            return await InvokeAuthResultLambdaAsync(authResult).ConfigureAwait(false);
        }

        #region Misc

        private async Task<VerifyResult> InvokeAuthResultLambdaAsync(AuthResultType authResult)
        {
            var authToken = authResult.StoredToken.CookieString;
            var idTokenResponse = await FetchCognitoIdTokenAsync(authToken).ConfigureAwait(false);

            var invokePayload = Serialize(new { token = idTokenResponse.LambdaToken });
            var responsePayload = await AWS.InvokeRecoverySharePasswordLambdaAsync(idTokenResponse.IdentityId, idTokenResponse.Token, invokePayload, thirdwebHttpClientType).ConfigureAwait(false);

            var jsonSerializer = new JsonSerializer();
            var payload = jsonSerializer.Deserialize<RecoverySharePasswordResponse>(new JsonTextReader(new StreamReader(responsePayload)));
            payload = jsonSerializer.Deserialize<RecoverySharePasswordResponse>(new JsonTextReader(new StringReader(payload.Body)));
            return new VerifyResult(
                authResult.StoredToken.IsNewUser,
                authToken,
                authResult.StoredToken.AuthDetails.UserWalletId,
                payload.RecoverySharePassword,
                authResult.StoredToken.AuthDetails.Email,
                authResult.StoredToken.AuthDetails.PhoneNumber
            );
        }

        private async Task<ThirdwebHttpResponseMessage> SendHttpWithAuthAsync(HttpRequestMessage httpRequestMessage, string authToken)
        {
            httpClient.AddHeader("Authorization", $"Bearer embedded-wallet-token:{authToken}");

            try
            {
                if (httpRequestMessage.Method == HttpMethod.Get)
                {
                    return await httpClient.GetAsync(httpRequestMessage.RequestUri.ToString()).ConfigureAwait(false);
                }
                else if (httpRequestMessage.Method == HttpMethod.Post)
                {
                    return await httpClient.PostAsync(httpRequestMessage.RequestUri.ToString(), httpRequestMessage.Content).ConfigureAwait(false);
                }
                else if (httpRequestMessage.Method == HttpMethod.Put)
                {
                    return await httpClient.PutAsync(httpRequestMessage.RequestUri.ToString(), httpRequestMessage.Content).ConfigureAwait(false);
                }
                else if (httpRequestMessage.Method == HttpMethod.Delete)
                {
                    return await httpClient.DeleteAsync(httpRequestMessage.RequestUri.ToString()).ConfigureAwait(false);
                }
                else
                {
                    throw new InvalidOperationException("Unsupported HTTP method");
                }
            }
            finally
            {
                httpClient.RemoveHeader("Authorization");
            }
        }

        private async Task<ThirdwebHttpResponseMessage> SendHttpWithAuthAsync(Uri uri, string authToken)
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, uri);
            return await SendHttpWithAuthAsync(httpRequestMessage, authToken).ConfigureAwait(false);
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
            stringContent.Headers.ContentType = jsonContentType;
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
}
