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
        internal abstract Task<string> FetchHeadlessOauthLoginLinkAsync(string authProvider);

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
        private const string BUNDLE_ID_HEADER = "x-bundle-id";
        private const string THIRDWEB_CLIENT_ID_HEADER = "x-thirdweb-client-id";
        private const string THIRDWEB_SECRET_KEY_HEADER = "x-thirdweb-client-id";
        private const string SESSION_NONCE_HEADER = "x-session-nonce";
        private const string EMBEDDED_WALLET_VERSION_HEADER = "x-embedded-wallet-version";

        private static readonly MediaTypeHeaderValue jsonContentType = MediaTypeHeaderValue.Parse("application/json");
        private static readonly HttpClient httpClient = new();

        private readonly string clientId;

        internal Server(string clientId, string bundleId, string platform, string version, string secretKey)
        {
            this.clientId = clientId;

            httpClient.DefaultRequestHeaders.Clear();

            if (!string.IsNullOrEmpty(clientId))
            {
                httpClient.DefaultRequestHeaders.Add(THIRDWEB_CLIENT_ID_HEADER, clientId);
            }

            if (!string.IsNullOrEmpty(bundleId))
            {
                httpClient.DefaultRequestHeaders.Add(BUNDLE_ID_HEADER, bundleId);
            }

            if (!string.IsNullOrEmpty(secretKey))
            {
                httpClient.DefaultRequestHeaders.Add(THIRDWEB_SECRET_KEY_HEADER, secretKey);
            }

            httpClient.DefaultRequestHeaders.Add(SESSION_NONCE_HEADER, Guid.NewGuid().ToString());
            httpClient.DefaultRequestHeaders.Add(EMBEDDED_WALLET_VERSION_HEADER, $"{platform}:{version}");
        }

        // embedded-wallet/verify-thirdweb-client-id
        internal override async Task<string> VerifyThirdwebClientIdAsync(string parentDomain)
        {
            Dictionary<string, string> queryParams = new() { { "clientId", clientId }, { "parentDomain", parentDomain } };
            Uri uri = MakeUri("/embedded-wallet/verify-thirdweb-client-id", queryParams);
            StringContent content = MakeHttpContent(new { clientId, parentDomain });
            HttpResponseMessage response = await httpClient.PostAsync(uri, content);
            await CheckStatusCodeAsync(response);
            var error = await DeserializeAsync<HttpErrorWithMessage>(response);
            return error.Error;
        }

        // embedded-wallet/developer-wallet-settings
        internal override async Task<string> FetchDeveloperWalletSettings()
        {
            try
            {
                Dictionary<string, string> queryParams = new() { { "clientId", clientId }, };
                Uri uri = MakeUri("/embedded-wallet/developer-wallet-settings", queryParams);
                HttpResponseMessage response = await httpClient.GetAsync(uri);
                var responseContent = await DeserializeAsync<RecoveryShareManagementResponse>(response);
                return responseContent.Value ?? "AWS_MANAGED";
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Could not fetch recovery share management type, defaulting to managed: " + e.Message);
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

            Uri uri = MakeUri("/embedded-wallet/embedded-wallet-user-details", queryParams);
            HttpResponseMessage response = await SendHttpWithAuthAsync(uri, authToken ?? "");
            await CheckStatusCodeAsync(response);
            var rv = await DeserializeAsync<UserWallet>(response);
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
            HttpResponseMessage response = await SendHttpWithAuthAsync(httpRequestMessage, authToken);
            await CheckStatusCodeAsync(response);
        }

        // embedded-wallet/embedded-wallet-shares GET
        internal override async Task<(string authShare, string recoveryShare)> FetchAuthAndRecoverySharesAsync(string authToken)
        {
            SharesGetResponse sharesGetResponse = await FetchRemoteSharesAsync(authToken, true);
            string authShare = sharesGetResponse.AuthShare ?? throw new InvalidOperationException("Server failed to return auth share");
            string encryptedRecoveryShare = sharesGetResponse.MaybeEncryptedRecoveryShares?.FirstOrDefault() ?? throw new InvalidOperationException("Server failed to return recovery share");
            return (authShare, encryptedRecoveryShare);
        }

        // embedded-wallet/embedded-wallet-shares GET
        internal override async Task<string> FetchAuthShareAsync(string authToken)
        {
            SharesGetResponse sharesGetResponse = await FetchRemoteSharesAsync(authToken, false);
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
            Uri uri = MakeUri("/embedded-wallet/embedded-wallet-shares", queryParams);
            HttpResponseMessage response = await SendHttpWithAuthAsync(uri, authToken);
            await CheckStatusCodeAsync(response);
            var rv = await DeserializeAsync<SharesGetResponse>(response);
            return rv;
        }

        // embedded-wallet/cognito-id-token
        private async Task<IdTokenResponse> FetchCognitoIdTokenAsync(string authToken)
        {
            Uri uri = MakeUri("/embedded-wallet/cognito-id-token");
            HttpResponseMessage response = await SendHttpWithAuthAsync(uri, authToken);
            await CheckStatusCodeAsync(response);
            return await DeserializeAsync<IdTokenResponse>(response);
        }

        // embedded-wallet/headless-oauth-login-link
        internal override async Task<string> FetchHeadlessOauthLoginLinkAsync(string authProvider)
        {
            // based on above unity implementation, adapt to this class
            Uri uri = MakeUri(
                "/embedded-wallet/headless-oauth-login-link",
                new Dictionary<string, string>
                {
                    { "platform", "unity" },
                    { "authProvider", authProvider },
                    { "baseUrl", "https://embedded-wallet.thirdweb.com" }
                }
            );

            HttpResponseMessage response = await httpClient.GetAsync(uri);
            await CheckStatusCodeAsync(response);
            var rv = await DeserializeAsync<HeadlessOauthLoginLinkResponse>(response);
            return rv.PlatformLoginLink;
        }

        // /embedded-wallet/is-cognito-otp-valid
        internal override async Task<bool> CheckIsEmailKmsOtpValidAsync(string email, string otp)
        {
            Uri uri = MakeUriLegacy(
                "/embedded-wallet/is-cognito-otp-valid",
                new Dictionary<string, string>
                {
                    { "email", email },
                    { "code", otp },
                    { "clientId", clientId }
                }
            );
            HttpResponseMessage response = await httpClient.GetAsync(uri);
            await CheckStatusCodeAsync(response);
            var result = await DeserializeAsync<IsEmailKmsOtpValidResponse>(response);
            return result.IsOtpValid;
        }

        // embedded-wallet/is-thirdweb-email-otp-valid
        internal override async Task<bool> CheckIsEmailUserOtpValidAsync(string email, string otp)
        {
            Uri uri = MakeUri("/embedded-wallet/is-thirdweb-email-otp-valid");
            StringContent content = MakeHttpContent(
                new
                {
                    email,
                    otp,
                    clientId,
                }
            );
            HttpResponseMessage response = await httpClient.PostAsync(uri, content);
            await CheckStatusCodeAsync(response);
            var result = await DeserializeAsync<IsEmailUserOtpValidResponse>(response);
            return result.IsValid;
        }

        // embedded-wallet/send-user-managed-email-otp
        internal override async Task SendUserOtpEmailAsync(string emailAddress)
        {
            Uri uri = MakeUri("/embedded-wallet/send-user-managed-email-otp");
            StringContent content = MakeHttpContent(new { clientId, email = emailAddress });
            HttpResponseMessage response = await httpClient.PostAsync(uri, content);
            await CheckStatusCodeAsync(response);
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
                HttpResponseMessage response = await SendHttpWithAuthAsync(httpRequestMessage, authToken);
                await CheckStatusCodeAsync(response);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error sending recovery code email", ex);
            }
        }

        // embedded-wallet/validate-thirdweb-email-otp
        internal override async Task<VerifyResult> VerifyUserOtpAsync(string emailAddress, string otp)
        {
            Uri uri = MakeUri("/embedded-wallet/validate-thirdweb-email-otp");
            StringContent content = MakeHttpContent(
                new
                {
                    clientId,
                    email = emailAddress,
                    otp
                }
            );
            HttpResponseMessage response = await httpClient.PostAsync(uri, content);
            await CheckStatusCodeAsync(response);
            var authVerifiedToken = await DeserializeAsync<AuthVerifiedTokenReturnType>(response);
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
            string userName = MakeCognitoUserName(emailAddress, "email");
            string sessionId = await AWS.StartCognitoUserAuth(userName);
            if (sessionId == null)
            {
                await AWS.SignUpCognitoUserAsync(emailAddress, userName);
                for (int i = 0; i < 3; ++i)
                {
                    await Task.Delay(3333 * i);
                    sessionId = await AWS.StartCognitoUserAuth(userName);
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
            string userName = MakeCognitoUserName(emailAddress, "email");
            TokenCollection tokens = await AWS.FinishCognitoUserAuth(userName, otp, sessionId);
            Uri uri = MakeUri("/embedded-wallet/validate-cognito-email-otp");
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
            HttpResponseMessage response = await httpClient.PostAsync(uri, content);
            await CheckStatusCodeAsync(response);
            var authVerifiedToken = await DeserializeAsync<AuthVerifiedTokenReturnType>(response);
            bool isNewUser = authVerifiedToken.VerifiedToken.IsNewUser;
            string authToken = authVerifiedToken.VerifiedTokenJwtString;
            string walletUserId = authVerifiedToken.VerifiedToken.AuthDetails.WalletUserId;
            var idTokenResponse = await FetchCognitoIdTokenAsync(authToken);
            string idToken = idTokenResponse.IdToken;
            string invokePayload = Serialize(new { accessToken = idTokenResponse.AccessToken, idToken = idTokenResponse.IdToken });
            MemoryStream responsePayload = await AWS.InvokeRecoverySharePasswordLambdaAsync(idToken, invokePayload);
            JsonSerializer jsonSerializer = new();
            var payload = jsonSerializer.Deserialize<RecoverySharePasswordResponse>(new JsonTextReader(new StreamReader(responsePayload)));
            payload = jsonSerializer.Deserialize<RecoverySharePasswordResponse>(new JsonTextReader(new StringReader(payload.Body)));
            return new VerifyResult(isNewUser, authToken, walletUserId, payload.RecoverySharePassword, authVerifiedToken.VerifiedToken.AuthDetails.Email);
        }

        internal override async Task<string> SendKmsPhoneOtpAsync(string phoneNumber)
        {
            string userName = MakeCognitoUserName(phoneNumber, "sms");
            string sessionId = await AWS.StartCognitoUserAuth(userName);
            if (sessionId == null)
            {
                await AWS.SignUpCognitoUserAsync(null, userName);
                for (int i = 0; i < 3; ++i)
                {
                    await Task.Delay(3333 * i);
                    sessionId = await AWS.StartCognitoUserAuth(userName);
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
            string userName = MakeCognitoUserName(phoneNumber, "sms");
            TokenCollection tokens = await AWS.FinishCognitoUserAuth(userName, otp, sessionId);
            Uri uri = MakeUri("/embedded-wallet/validate-cognito-email-otp");
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
            HttpResponseMessage response = await httpClient.PostAsync(uri, content);
            await CheckStatusCodeAsync(response);
            var authVerifiedToken = await DeserializeAsync<AuthVerifiedTokenReturnType>(response);
            bool isNewUser = authVerifiedToken.VerifiedToken.IsNewUser;
            string authToken = authVerifiedToken.VerifiedTokenJwtString;
            string walletUserId = authVerifiedToken.VerifiedToken.AuthDetails.WalletUserId;
            var idTokenResponse = await FetchCognitoIdTokenAsync(authToken);
            string idToken = idTokenResponse.IdToken;
            string invokePayload = Serialize(new { accessToken = idTokenResponse.AccessToken, idToken = idTokenResponse.IdToken });
            MemoryStream responsePayload = await AWS.InvokeRecoverySharePasswordLambdaAsync(idToken, invokePayload);
            JsonSerializer jsonSerializer = new();
            var payload = jsonSerializer.Deserialize<RecoverySharePasswordResponse>(new JsonTextReader(new StreamReader(responsePayload)));
            payload = jsonSerializer.Deserialize<RecoverySharePasswordResponse>(new JsonTextReader(new StringReader(payload.Body)));
            return new VerifyResult(isNewUser, authToken, walletUserId, payload.RecoverySharePassword, authVerifiedToken.VerifiedToken.AuthDetails.Email);
        }

        // embedded-wallet/validate-custom-jwt
        internal override async Task<VerifyResult> VerifyJwtAsync(string jwtToken)
        {
            var requestContent = new { jwt = jwtToken, developerClientId = clientId };
            StringContent content = MakeHttpContent(requestContent);
            Uri uri = MakeUri("/embedded-wallet/validate-custom-jwt");
            HttpResponseMessage response = await httpClient.PostAsync(uri, content);
            await CheckStatusCodeAsync(response);
            var authVerifiedToken = await DeserializeAsync<AuthVerifiedTokenReturnType>(response);
            bool isNewUser = authVerifiedToken.VerifiedToken.IsNewUser;
            string authToken = authVerifiedToken.VerifiedTokenJwtString;
            string walletUserId = authVerifiedToken.VerifiedToken.AuthDetails.WalletUserId;
            string email = authVerifiedToken.VerifiedToken.AuthDetails.Email;
            string recoveryCode = authVerifiedToken.VerifiedToken.AuthDetails.RecoveryCode;
            return new VerifyResult(isNewUser, authToken, walletUserId, recoveryCode, email);
        }

        // embedded-wallet/validate-custom-auth-endpoint
        internal override async Task<VerifyResult> VerifyAuthEndpointAsync(string payload)
        {
            var requestContent = new { payload, developerClientId = clientId };
            StringContent content = MakeHttpContent(requestContent);
            Uri uri = MakeUri("/embedded-wallet/validate-custom-auth-endpoint");
            HttpResponseMessage response = await httpClient.PostAsync(uri, content);
            await CheckStatusCodeAsync(response);
            var authVerifiedToken = await DeserializeAsync<AuthVerifiedTokenReturnType>(response);
            bool isNewUser = authVerifiedToken.VerifiedToken.IsNewUser;
            string authToken = authVerifiedToken.VerifiedTokenJwtString;
            string walletUserId = authVerifiedToken.VerifiedToken.AuthDetails.WalletUserId;
            string email = authVerifiedToken.VerifiedToken.AuthDetails.Email;
            string recoveryCode = authVerifiedToken.VerifiedToken.AuthDetails.RecoveryCode;
            return new VerifyResult(isNewUser, authToken, walletUserId, recoveryCode, email);
        }

        internal override async Task<VerifyResult> VerifyOAuthAsync(string authResultStr)
        {
            var authResult = JsonConvert.DeserializeObject<AuthResultType_OAuth>(authResultStr);
            bool isNewUser = authResult.StoredToken.IsNewUser;
            string authToken = authResult.StoredToken.CookieString;
            string walletUserId = authResult.StoredToken.AuthDetails.UserWalletId;
            bool isUserManaged = (await FetchUserDetailsAsync(authResult.StoredToken.AuthDetails.Email, authToken)).RecoveryShareManagement == "USER_MANAGED";
            string recoveryCode = null;
            if (!isUserManaged)
            {
                var idTokenResponse = await FetchCognitoIdTokenAsync(authToken);
                string idToken = idTokenResponse.IdToken;
                string invokePayload = Serialize(new { accessToken = idTokenResponse.AccessToken, idToken = idTokenResponse.IdToken });
                MemoryStream responsePayload = await AWS.InvokeRecoverySharePasswordLambdaAsync(idToken, invokePayload);
                JsonSerializer jsonSerializer = new();
                var payload = jsonSerializer.Deserialize<RecoverySharePasswordResponse>(new JsonTextReader(new StreamReader(responsePayload)));
                payload = jsonSerializer.Deserialize<RecoverySharePasswordResponse>(new JsonTextReader(new StringReader(payload.Body)));
                recoveryCode = payload.RecoverySharePassword;
            }
            return new VerifyResult(isNewUser, authToken, walletUserId, recoveryCode, authResult.StoredToken.AuthDetails.Email);
        }

        #region Misc

        private Task<HttpResponseMessage> SendHttpWithAuthAsync(HttpRequestMessage httpRequestMessage, string authToken)
        {
            httpRequestMessage.Headers.Add("Authorization", $"Bearer embedded-wallet-token:{authToken}");
#if DEBUG
            Console.WriteLine($"Request: {JsonConvert.SerializeObject(httpRequestMessage)}");
#endif
            return httpClient.SendAsync(httpRequestMessage);
        }

        private Task<HttpResponseMessage> SendHttpWithAuthAsync(Uri uri, string authToken)
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, uri);
#if DEBUG
            Console.WriteLine($"Request: {JsonConvert.SerializeObject(httpRequestMessage)}");
#endif
            return SendHttpWithAuthAsync(httpRequestMessage, authToken);
        }

        private static async Task CheckStatusCodeAsync(HttpResponseMessage response)
        {
#if DEBUG
            Console.WriteLine($"Response: {await response.Content.ReadAsStringAsync()}");
#endif
            if (!response.IsSuccessStatusCode)
            {
                var error = await DeserializeAsync<HttpErrorWithMessage>(response);
                throw new InvalidOperationException(string.IsNullOrEmpty(error.Error) ? error.Message : error.Error);
            }
        }

        private static async Task<T> DeserializeAsync<T>(HttpResponseMessage response)
        {
            JsonSerializer jsonSerializer = new();
            TextReader textReader = new StreamReader(await response.Content.ReadAsStreamAsync());
            var rv = jsonSerializer.Deserialize<T>(new JsonTextReader(textReader));
            return rv;
        }

        private static Uri MakeUri(string path, IDictionary<string, string> parameters = null)
        {
            UriBuilder b = new(ROOT_URL) { Path = API_ROOT_PATH + path, };
            if (parameters != null && parameters.Any())
            {
                string queryString = string.Join('&', parameters.Select((p) => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
                b.Query = queryString;
            }
            return b.Uri;
        }

        private static Uri MakeUriLegacy(string path, IDictionary<string, string> parameters = null)
        {
            UriBuilder b = new(ROOT_URL_LEGACY) { Path = API_ROOT_PATH_LEGACY + path, };
            if (parameters != null && parameters.Any())
            {
                string queryString = string.Join('&', parameters.Select((p) => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
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
            string rv = stringWriter.ToString();

            return rv;
        }

        private string MakeCognitoUserName(string userData, string type)
        {
            return $"{userData}:{type}:{clientId}";
        }

        #endregion
    }
}
