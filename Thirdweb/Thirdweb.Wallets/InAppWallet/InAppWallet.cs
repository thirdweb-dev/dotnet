using System.Web;
using Nethereum.Signer;
using Thirdweb.EWS;

namespace Thirdweb
{
    public enum AuthProvider
    {
        Default,
        Google,
        Apple,
        Facebook,
        JWT,
        AuthEndpoint
    }

    public class InAppWallet : PrivateKeyWallet
    {
        internal EmbeddedWallet _embeddedWallet;
        internal string _email;
        internal string _phoneNumber;
        internal string _authProvider;

        internal InAppWallet(ThirdwebClient client, string email, string phoneNumber, string authProvider, EmbeddedWallet embeddedWallet, EthECKey ecKey)
            : base(client, ecKey)
        {
            _email = email;
            _phoneNumber = phoneNumber;
            _embeddedWallet = embeddedWallet;
            _authProvider = authProvider;
        }

        public static async Task<InAppWallet> Create(
            ThirdwebClient client,
            string email = null,
            string phoneNumber = null,
            AuthProvider authprovider = AuthProvider.Default,
            string storageDirectoryPath = null
        )
        {
            if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(phoneNumber) && authprovider == AuthProvider.Default)
            {
                throw new ArgumentException("Email, Phone Number, or OAuth Provider must be provided to login.");
            }

            var authproviderStr = authprovider switch
            {
                AuthProvider.Google => "Google",
                AuthProvider.Apple => "Apple",
                AuthProvider.Facebook => "Facebook",
                AuthProvider.JWT => "JWT",
                AuthProvider.AuthEndpoint => "AuthEndpoint",
                AuthProvider.Default => string.IsNullOrEmpty(email) ? "PhoneOTP" : "EmailOTP",
                _ => throw new ArgumentException("Invalid AuthProvider"),
            };

            var embeddedWallet = new EmbeddedWallet(client, storageDirectoryPath);
            EthECKey ecKey;
            try
            {
                if (!string.IsNullOrEmpty(authproviderStr)) { }
                var user = await embeddedWallet.GetUserAsync(email, authproviderStr);
                ecKey = new EthECKey(user.Account.PrivateKey);
            }
            catch
            {
                ecKey = null;
            }
            return new InAppWallet(client, email, phoneNumber, authproviderStr, embeddedWallet, ecKey);
        }

        public override async Task Disconnect()
        {
            await base.Disconnect();
            await _embeddedWallet.SignOutAsync();
        }

        #region OAuth2 Flow

        public virtual async Task<string> LoginWithOauth(
            bool isMobile,
            Action<string> browserOpenAction,
            string mobileRedirectScheme = "thirdweb://",
            IThirdwebBrowser browser = null,
            CancellationToken cancellationToken = default
        )
        {
            if (isMobile && string.IsNullOrEmpty(mobileRedirectScheme))
            {
                throw new ArgumentNullException(nameof(mobileRedirectScheme), "Mobile redirect scheme cannot be null or empty on this platform.");
            }

            var platform = Client.HttpClient?.Headers?["x-sdk-name"] == "UnitySDK_WebGL" ? "web" : "dotnet";
            var redirectUrl = isMobile ? mobileRedirectScheme : "http://localhost:8789/";
            var loginUrl = await _embeddedWallet.FetchHeadlessOauthLoginLinkAsync(_authProvider, platform);
            loginUrl = platform == "web" ? loginUrl : $"{loginUrl}?platform={platform}&redirectUrl={redirectUrl}&developerClientId={Client.ClientId}&authOption={_authProvider}";

            browser ??= new InAppWalletBrowser();
            var browserResult = await browser.Login(Client, loginUrl, redirectUrl, browserOpenAction, cancellationToken);
            switch (browserResult.status)
            {
                case BrowserStatus.Success:
                    break;
                case BrowserStatus.UserCanceled:
                    throw new TaskCanceledException(browserResult.error ?? "LoginWithOauth was cancelled.");
                case BrowserStatus.Timeout:
                    throw new TimeoutException(browserResult.error ?? "LoginWithOauth timed out.");
                case BrowserStatus.UnknownError:
                default:
                    throw new Exception($"Failed to login with {_authProvider}: {browserResult.status} | {browserResult.error}");
            }
            var callbackUrl =
                browserResult.status != BrowserStatus.Success
                    ? throw new Exception($"Failed to login with {_authProvider}: {browserResult.status} | {browserResult.error}")
                    : browserResult.callbackUrl;

            while (string.IsNullOrEmpty(callbackUrl))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new TaskCanceledException("LoginWithOauth was cancelled.");
                }
                await Task.Delay(100, cancellationToken);
            }

            var authResultJson = callbackUrl;
            if (!authResultJson.StartsWith("{"))
            {
                var decodedUrl = HttpUtility.UrlDecode(callbackUrl);
                Uri uri = new(decodedUrl);
                var queryString = uri.Query;
                var queryDict = HttpUtility.ParseQueryString(queryString);
                authResultJson = queryDict["authResult"];
            }

            var res = await _embeddedWallet.SignInWithOauthAsync(_authProvider, authResultJson, null);
            if (res.User == null)
            {
                throw new Exception("Failed to login with OAuth2");
            }
            _ecKey = new EthECKey(res.User.Account.PrivateKey);
            return await GetAddress();
        }

        #endregion

        #region OTP Flow

        public async Task SendOTP()
        {
            if (string.IsNullOrEmpty(_email) && string.IsNullOrEmpty(_phoneNumber))
            {
                throw new Exception("Email or Phone Number is required for OTP login");
            }

            try
            {
                if (_email != null)
                {
                    (var isNewUser, var isNewDevice, var needsRecoveryCode) = await _embeddedWallet.SendOtpEmailAsync(_email);
                }
                else if (_phoneNumber != null)
                {
                    (var isNewUser, var isNewDevice, var needsRecoveryCode) = await _embeddedWallet.SendOtpPhoneAsync(_phoneNumber);
                }
                else
                {
                    throw new Exception("Email or Phone Number must be provided to login.");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to send OTP email", e);
            }
        }

        public async Task<(string, bool)> SubmitOTP(string otp)
        {
            if (string.IsNullOrEmpty(otp))
            {
                throw new ArgumentNullException(nameof(otp), "OTP cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(_email) && string.IsNullOrEmpty(_phoneNumber))
            {
                throw new Exception("Email or Phone Number is required for OTP login");
            }

            var res = _email == null ? await _embeddedWallet.VerifyPhoneOtpAsync(_phoneNumber, otp, null) : await _embeddedWallet.VerifyOtpAsync(_email, otp, null);
            if (res.User == null)
            {
                return (null, res.CanRetry);
            }
            else
            {
                _ecKey = new EthECKey(res.User.Account.PrivateKey);
                return (await GetAddress(), false);
            }
        }

        public Task<string> GetEmail()
        {
            return Task.FromResult(_email);
        }

        public Task<string> GetPhoneNumber()
        {
            return Task.FromResult(_phoneNumber);
        }

        #endregion

        #region JWT Flow

        public async Task<string> LoginWithJWT(string jwt, string encryptionKey, string recoveryCode = null)
        {
            if (string.IsNullOrEmpty(jwt))
            {
                throw new ArgumentException(nameof(jwt), "JWT cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(encryptionKey))
            {
                throw new ArgumentException(nameof(encryptionKey), "Encryption key cannot be null or empty.");
            }

            var res = await _embeddedWallet.SignInWithJwtAsync(jwt, encryptionKey, recoveryCode);

            if (res.User == null)
            {
                throw new Exception("Failed to login with JWT");
            }

            _ecKey = new EthECKey(res.User.Account.PrivateKey);

            return await GetAddress();
        }

        #endregion

        #region Auth Endpoint Flow

        public async Task<string> LoginWithAuthEndpoint(string payload, string encryptionKey, string recoveryCode = null)
        {
            if (string.IsNullOrEmpty(payload))
            {
                throw new ArgumentException(nameof(payload), "Payload cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(encryptionKey))
            {
                throw new ArgumentException(nameof(encryptionKey), "Encryption key cannot be null or empty.");
            }

            var res = await _embeddedWallet.SignInWithAuthEndpointAsync(payload, encryptionKey, recoveryCode);

            if (res.User == null)
            {
                throw new Exception("Failed to login with Auth Endpoint");
            }

            _ecKey = new EthECKey(res.User.Account.PrivateKey);

            return await GetAddress();
        }

        #endregion
    }
}
