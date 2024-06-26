using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Thirdweb.EWS
{
    internal abstract class ServerBase
    {
        internal abstract Task<string> VerifyThirdwebClientIdAsync(string domain);
        internal abstract Task<string> FetchDeveloperWalletSettings();
        internal abstract Task<Server.UserWallet> FetchUserDetailsAsync(string emailAddress, string authToken);
        internal abstract Task StoreAddressAndSharesAsync(string walletAddress, string authShare, string encryptedRecoveryShare, string authToken, string[] backupRecoveryShares);

        internal abstract Task<(string authShare, string recoveryShare)> FetchAuthAndRecoverySharesAsync(string authToken);
        internal abstract Task<string> FetchAuthShareAsync(string authToken);
        internal abstract Task<string> FetchHeadlessOauthLoginLinkAsync(string authProvider, string platform);

        internal abstract Task<bool> CheckIsEmailKmsOtpValidAsync(string userName, string otp);
        internal abstract Task<bool> CheckIsEmailUserOtpValidAsync(string emailAddress, string otp);

        internal abstract Task SendUserOtpEmailAsync(string emailAddress);
        internal abstract Task SendRecoveryCodeEmailAsync(string authToken, string recoveryCode, string email);
        internal abstract Task<Server.VerifyResult> VerifyUserOtpAsync(string emailAddress, string otp);

        internal abstract Task<string> SendKmsOtpEmailAsync(string emailAddress);
        internal abstract Task<Server.VerifyResult> VerifyKmsOtpAsync(string emailAddress, string otp, string sessionId);

        internal abstract Task<string> SendKmsPhoneOtpAsync(string phoneNumber);
        internal abstract Task<Server.VerifyResult> VerifyKmsPhoneOtpAsync(string phoneNumber, string otp, string sessionId);

        internal abstract Task<Server.VerifyResult> VerifyJwtAsync(string jwtToken);

        internal abstract Task<Server.VerifyResult> VerifyOAuthAsync(string authVerifiedToken);

        internal abstract Task<Server.VerifyResult> VerifyAuthEndpointAsync(string payload);
    }

    internal partial class Server : ServerBase
    {
        private const string ROOT_URL = "https://embedded-wallet.thirdweb.com";
        private const string ROOT_URL_LEGACY = "https://ews.thirdweb.com";
        private const string API_ROOT_PATH = "/api/2023-10-20";
        private const string API_ROOT_PATH_LEGACY = "/api/2022-08-12";

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
            var uri = MakeUri("/embedded-wallet/verify-thirdweb-client-id", queryParams);
            var content = MakeHttpContent(new { clientId, parentDomain });
            var response = await httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);
            var error = await DeserializeAsync<HttpErrorWithMessage>(response).ConfigureAwait(false);
            return error.Error;
        }

        // embedded-wallet/developer-wallet-settings
        internal override async Task<string> FetchDeveloperWalletSettings()
        {
            try
            {
                Dictionary<string, string> queryParams = new() { { "clientId", clientId }, };
                var uri = MakeUri("/embedded-wallet/developer-wallet-settings", queryParams);
                var response = await httpClient.GetAsync(uri.ToString()).ConfigureAwait(false);
                var responseContent = await DeserializeAsync<RecoveryShareManagementResponse>(response).ConfigureAwait(false);
                return responseContent.Value ?? "AWS_MANAGED";
            }
            catch
            {
                return "AWS_MANAGED";
            }
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

            var uri = MakeUri("/embedded-wallet/embedded-wallet-user-details", queryParams);
            var response = await SendHttpWithAuthAsync(uri, authToken ?? "").ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);
            var rv = await DeserializeAsync<UserWallet>(response).ConfigureAwait(false);
            return rv;
        }

        // embedded-wallet/embedded-wallet-shares POST
        internal override async Task StoreAddressAndSharesAsync(string walletAddress, string authShare, string encryptedRecoveryShare, string authToken, string[] backupRecoveryShares)
        {
            var encryptedRecoveryShares =
                backupRecoveryShares == null
                    ? new[] { new { share = encryptedRecoveryShare, isClientEncrypted = "true" } }
                    : new[] { new { share = encryptedRecoveryShare, isClientEncrypted = "true" } }.Concat(backupRecoveryShares.Select((s) => new { share = s, isClientEncrypted = "true" })).ToArray();

            HttpRequestMessage httpRequestMessage =
                new(HttpMethod.Post, MakeUri("/embedded-wallet/embedded-wallet-shares"))
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
            var uri = MakeUri("/embedded-wallet/embedded-wallet-shares", queryParams);
            var response = await SendHttpWithAuthAsync(uri, authToken).ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);
            var rv = await DeserializeAsync<SharesGetResponse>(response).ConfigureAwait(false);
            return rv;
        }

        // embedded-wallet/cognito-id-token
        private async Task<IdTokenResponse> FetchCognitoIdTokenAsync(string authToken)
        {
            var uri = MakeUri("/embedded-wallet/cognito-id-token");
            var response = await SendHttpWithAuthAsync(uri, authToken).ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);
            return await DeserializeAsync<IdTokenResponse>(response).ConfigureAwait(false);
        }

        // embedded-wallet/headless-oauth-login-link
        internal override async Task<string> FetchHeadlessOauthLoginLinkAsync(string authProvider, string platform)
        {
            var uri = MakeUri(
                "/embedded-wallet/headless-oauth-login-link",
                new Dictionary<string, string>
                {
                    { "platform", platform },
                    { "authProvider", authProvider },
                    { "baseUrl", "https://embedded-wallet.thirdweb.com" }
                }
            );

            var response = await httpClient.GetAsync(uri.ToString()).ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);
            var rv = await DeserializeAsync<HeadlessOauthLoginLinkResponse>(response).ConfigureAwait(false);
            return rv.PlatformLoginLink;
        }

        // /embedded-wallet/is-cognito-otp-valid
        internal override async Task<bool> CheckIsEmailKmsOtpValidAsync(string email, string otp)
        {
            var uri = MakeUriLegacy(
                "/embedded-wallet/is-cognito-otp-valid",
                new Dictionary<string, string>
                {
                    { "email", email },
                    { "code", otp },
                    { "clientId", clientId }
                }
            );
            var response = await httpClient.GetAsync(uri.ToString()).ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);
            var result = await DeserializeAsync<IsEmailKmsOtpValidResponse>(response).ConfigureAwait(false);
            return result.IsOtpValid;
        }

        // embedded-wallet/is-thirdweb-email-otp-valid
        internal override async Task<bool> CheckIsEmailUserOtpValidAsync(string email, string otp)
        {
            var uri = MakeUri("/embedded-wallet/is-thirdweb-email-otp-valid");
            var content = MakeHttpContent(
                new
                {
                    email,
                    otp,
                    clientId,
                }
            );
            var response = await httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);
            var result = await DeserializeAsync<IsEmailUserOtpValidResponse>(response).ConfigureAwait(false);
            return result.IsValid;
        }

        // embedded-wallet/send-user-managed-email-otp
        internal override async Task SendUserOtpEmailAsync(string emailAddress)
        {
            var uri = MakeUri("/embedded-wallet/send-user-managed-email-otp");
            var content = MakeHttpContent(new { clientId, email = emailAddress });
            var response = await httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);
        }

        // embedded-wallet/send-wallet-recovery-code
        internal override async Task SendRecoveryCodeEmailAsync(string authToken, string recoveryCode, string email)
        {
            HttpRequestMessage httpRequestMessage =
                new(HttpMethod.Post, MakeUri("/embedded-wallet/send-wallet-recovery-code"))
                {
                    Content = MakeHttpContent(
                        new
                        {
                            strategy = "email",
                            clientId,
                            email,
                            recoveryCode
                        }
                    ),
                };
            try
            {
                var response = await SendHttpWithAuthAsync(httpRequestMessage, authToken).ConfigureAwait(false);
                await CheckStatusCodeAsync(response).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error sending recovery code email", ex);
            }
        }

        // embedded-wallet/validate-thirdweb-email-otp
        internal override async Task<VerifyResult> VerifyUserOtpAsync(string emailAddress, string otp)
        {
            var uri = MakeUri("/embedded-wallet/validate-thirdweb-email-otp");
            var content = MakeHttpContent(
                new
                {
                    clientId,
                    email = emailAddress,
                    otp
                }
            );
            var response = await httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);
            var authVerifiedToken = await DeserializeAsync<AuthVerifiedTokenReturnType>(response).ConfigureAwait(false);
            return new VerifyResult(
                authVerifiedToken.VerifiedToken.IsNewUser,
                authVerifiedToken.VerifiedTokenJwtString,
                authVerifiedToken.VerifiedToken.AuthDetails.WalletUserId,
                authVerifiedToken.VerifiedToken.AuthDetails.RecoveryCode,
                authVerifiedToken.VerifiedToken.AuthDetails.Email
            );
        }

        // KMS Send
        internal override async Task<string> SendKmsOtpEmailAsync(string emailAddress)
        {
            var userName = MakeCognitoUserName(emailAddress, "email");
            var sessionId = await AWS.StartCognitoUserAuth(userName, thirdwebHttpClientType).ConfigureAwait(false);
            if (sessionId == null)
            {
                await AWS.SignUpCognitoUserAsync(emailAddress, userName, thirdwebHttpClientType).ConfigureAwait(false);
                for (var i = 0; i < 3; ++i)
                {
                    await Task.Delay(3333 * i).ConfigureAwait(false);
                    sessionId = await AWS.StartCognitoUserAuth(userName, thirdwebHttpClientType).ConfigureAwait(false);
                    if (sessionId != null)
                    {
                        break;
                    }
                }
                if (sessionId == null)
                {
                    throw new InvalidOperationException("Cannot find user within timeout period");
                }
            }
            return sessionId;
        }

        // embedded-wallet/validate-cognito-email-otp
        internal override async Task<VerifyResult> VerifyKmsOtpAsync(string emailAddress, string otp, string sessionId)
        {
            var userName = MakeCognitoUserName(emailAddress, "email");
            var tokens = await AWS.FinishCognitoUserAuth(userName, otp, sessionId, thirdwebHttpClientType).ConfigureAwait(false);
            var uri = MakeUri("/embedded-wallet/validate-cognito-email-otp");
            ByteArrayContent content = MakeHttpContent(
                new
                {
                    developerClientId = clientId,
                    access_token = tokens.AccessToken,
                    id_token = tokens.IdToken,
                    refresh_token = tokens.RefreshToken,
                    otpMethod = "email",
                }
            );
            var response = await httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);
            var authVerifiedToken = await DeserializeAsync<AuthVerifiedTokenReturnType>(response).ConfigureAwait(false);
            var isNewUser = authVerifiedToken.VerifiedToken.IsNewUser;
            var authToken = authVerifiedToken.VerifiedTokenJwtString;
            var walletUserId = authVerifiedToken.VerifiedToken.AuthDetails.WalletUserId;
            var idTokenResponse = await FetchCognitoIdTokenAsync(authToken).ConfigureAwait(false);
            var idToken = idTokenResponse.IdToken;
            var invokePayload = Serialize(new { accessToken = idTokenResponse.AccessToken, idToken = idTokenResponse.IdToken });
            var responsePayload = await AWS.InvokeRecoverySharePasswordLambdaAsync(idToken, invokePayload, thirdwebHttpClientType).ConfigureAwait(false);
            JsonSerializer jsonSerializer = new();
            var payload = jsonSerializer.Deserialize<RecoverySharePasswordResponse>(new JsonTextReader(new StreamReader(responsePayload)));
            payload = jsonSerializer.Deserialize<RecoverySharePasswordResponse>(new JsonTextReader(new StringReader(payload.Body)));
            return new VerifyResult(isNewUser, authToken, walletUserId, payload.RecoverySharePassword, authVerifiedToken.VerifiedToken.AuthDetails.Email);
        }

        internal override async Task<string> SendKmsPhoneOtpAsync(string phoneNumber)
        {
            var userName = MakeCognitoUserName(phoneNumber, "sms");
            var sessionId = await AWS.StartCognitoUserAuth(userName, thirdwebHttpClientType).ConfigureAwait(false);
            if (sessionId == null)
            {
                await AWS.SignUpCognitoUserAsync(null, userName, thirdwebHttpClientType).ConfigureAwait(false);
                for (var i = 0; i < 3; ++i)
                {
                    await Task.Delay(3333 * i).ConfigureAwait(false);
                    sessionId = await AWS.StartCognitoUserAuth(userName, thirdwebHttpClientType).ConfigureAwait(false);
                    if (sessionId != null)
                    {
                        break;
                    }
                }
                if (sessionId == null)
                {
                    throw new InvalidOperationException("Cannot find user within timeout period");
                }
            }
            return sessionId;
        }

        // embedded-wallet/validate-cognito-email-otp
        internal override async Task<VerifyResult> VerifyKmsPhoneOtpAsync(string phoneNumber, string otp, string sessionId)
        {
            var userName = MakeCognitoUserName(phoneNumber, "sms");
            var tokens = await AWS.FinishCognitoUserAuth(userName, otp, sessionId, thirdwebHttpClientType).ConfigureAwait(false);
            var uri = MakeUri("/embedded-wallet/validate-cognito-email-otp");
            ByteArrayContent content = MakeHttpContent(
                new
                {
                    developerClientId = clientId,
                    access_token = tokens.AccessToken,
                    id_token = tokens.IdToken,
                    refresh_token = tokens.RefreshToken,
                    otpMethod = "email",
                }
            );
            var response = await httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);
            var authVerifiedToken = await DeserializeAsync<AuthVerifiedTokenReturnType>(response).ConfigureAwait(false);
            var isNewUser = authVerifiedToken.VerifiedToken.IsNewUser;
            var authToken = authVerifiedToken.VerifiedTokenJwtString;
            var walletUserId = authVerifiedToken.VerifiedToken.AuthDetails.WalletUserId;
            var idTokenResponse = await FetchCognitoIdTokenAsync(authToken).ConfigureAwait(false);
            var idToken = idTokenResponse.IdToken;
            var invokePayload = Serialize(new { accessToken = idTokenResponse.AccessToken, idToken = idTokenResponse.IdToken });
            var responsePayload = await AWS.InvokeRecoverySharePasswordLambdaAsync(idToken, invokePayload, thirdwebHttpClientType).ConfigureAwait(false);
            JsonSerializer jsonSerializer = new();
            var payload = jsonSerializer.Deserialize<RecoverySharePasswordResponse>(new JsonTextReader(new StreamReader(responsePayload)));
            payload = jsonSerializer.Deserialize<RecoverySharePasswordResponse>(new JsonTextReader(new StringReader(payload.Body)));
            return new VerifyResult(isNewUser, authToken, walletUserId, payload.RecoverySharePassword, authVerifiedToken.VerifiedToken.AuthDetails.Email);
        }

        // embedded-wallet/validate-custom-jwt
        internal override async Task<VerifyResult> VerifyJwtAsync(string jwtToken)
        {
            var requestContent = new { jwt = jwtToken, developerClientId = clientId };
            var content = MakeHttpContent(requestContent);
            var uri = MakeUri("/embedded-wallet/validate-custom-jwt");
            var response = await httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);
            var authVerifiedToken = await DeserializeAsync<AuthVerifiedTokenReturnType>(response).ConfigureAwait(false);
            var isNewUser = authVerifiedToken.VerifiedToken.IsNewUser;
            var authToken = authVerifiedToken.VerifiedTokenJwtString;
            var walletUserId = authVerifiedToken.VerifiedToken.AuthDetails.WalletUserId;
            var email = authVerifiedToken.VerifiedToken.AuthDetails.Email;
            var recoveryCode = authVerifiedToken.VerifiedToken.AuthDetails.RecoveryCode;
            return new VerifyResult(isNewUser, authToken, walletUserId, recoveryCode, email);
        }

        // embedded-wallet/validate-custom-auth-endpoint
        internal override async Task<VerifyResult> VerifyAuthEndpointAsync(string payload)
        {
            var requestContent = new { payload, developerClientId = clientId };
            var content = MakeHttpContent(requestContent);
            var uri = MakeUri("/embedded-wallet/validate-custom-auth-endpoint");
            var response = await httpClient.PostAsync(uri.ToString(), content).ConfigureAwait(false);
            await CheckStatusCodeAsync(response).ConfigureAwait(false);
            var authVerifiedToken = await DeserializeAsync<AuthVerifiedTokenReturnType>(response).ConfigureAwait(false);
            var isNewUser = authVerifiedToken.VerifiedToken.IsNewUser;
            var authToken = authVerifiedToken.VerifiedTokenJwtString;
            var walletUserId = authVerifiedToken.VerifiedToken.AuthDetails.WalletUserId;
            var email = authVerifiedToken.VerifiedToken.AuthDetails.Email;
            var recoveryCode = authVerifiedToken.VerifiedToken.AuthDetails.RecoveryCode;
            return new VerifyResult(isNewUser, authToken, walletUserId, recoveryCode, email);
        }

        internal override async Task<VerifyResult> VerifyOAuthAsync(string authResultStr)
        {
            var authResult = JsonConvert.DeserializeObject<AuthResultType_OAuth>(authResultStr);
            var isNewUser = authResult.StoredToken.IsNewUser;
            var authToken = authResult.StoredToken.CookieString;
            var walletUserId = authResult.StoredToken.AuthDetails.UserWalletId;
            var isUserManaged = (await FetchUserDetailsAsync(authResult.StoredToken.AuthDetails.Email, authToken).ConfigureAwait(false)).RecoveryShareManagement == "USER_MANAGED";
            string recoveryCode = null;
            if (!isUserManaged)
            {
                var idTokenResponse = await FetchCognitoIdTokenAsync(authToken).ConfigureAwait(false);
                var idToken = idTokenResponse.IdToken;
                var invokePayload = Serialize(new { accessToken = idTokenResponse.AccessToken, idToken = idTokenResponse.IdToken });
                var responsePayload = await AWS.InvokeRecoverySharePasswordLambdaAsync(idToken, invokePayload, thirdwebHttpClientType).ConfigureAwait(false);
                JsonSerializer jsonSerializer = new();
                var payload = jsonSerializer.Deserialize<RecoverySharePasswordResponse>(new JsonTextReader(new StreamReader(responsePayload)));
                payload = jsonSerializer.Deserialize<RecoverySharePasswordResponse>(new JsonTextReader(new StringReader(payload.Body)));
                recoveryCode = payload.RecoverySharePassword;
            }
            return new VerifyResult(isNewUser, authToken, walletUserId, recoveryCode, authResult.StoredToken.AuthDetails.Email);
        }

        #region Misc

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

        private static Uri MakeUri(string path, IDictionary<string, string> parameters = null)
        {
            UriBuilder b = new(ROOT_URL) { Path = API_ROOT_PATH + path, };
            if (parameters != null && parameters.Any())
            {
                var queryString = string.Join('&', parameters.Select((p) => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
                b.Query = queryString;
            }
            return b.Uri;
        }

        private static Uri MakeUriLegacy(string path, IDictionary<string, string> parameters = null)
        {
            UriBuilder b = new(ROOT_URL_LEGACY) { Path = API_ROOT_PATH_LEGACY + path, };
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

        private string MakeCognitoUserName(string userData, string type)
        {
            return $"{userData}:{type}:{clientId}";
        }

        #endregion
    }
}
