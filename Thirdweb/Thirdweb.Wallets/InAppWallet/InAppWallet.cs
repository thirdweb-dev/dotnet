using System.Web;
using Nethereum.Signer;
using Thirdweb.EWS;

namespace Thirdweb
{
    /// <summary>
    /// Specifies the authentication providers available for the in-app wallet.
    /// </summary>
    public enum AuthProvider
    {
        Default,
        Google,
        Apple,
        Facebook,
        JWT,
        AuthEndpoint,
        Discord,
        Farcaster,
        Telegram
    }

    /// <summary>
    /// Represents an in-app wallet that extends the functionality of a private key wallet.
    /// </summary>
    public class InAppWallet : PrivateKeyWallet
    {
        internal EmbeddedWallet _embeddedWallet;
        internal string _email;
        internal string _phoneNumber;
        internal string _authProvider;

        internal InAppWallet(ThirdwebClient client, string email, string phoneNumber, string authProvider, EmbeddedWallet embeddedWallet, EthECKey ecKey)
            : base(client, ecKey)
        {
            _email = email?.ToLower();
            _phoneNumber = phoneNumber;
            _embeddedWallet = embeddedWallet;
            _authProvider = authProvider;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="InAppWallet"/> class.
        /// </summary>
        /// <param name="client">The Thirdweb client instance.</param>
        /// <param name="email">The email address for authentication.</param>
        /// <param name="phoneNumber">The phone number for authentication.</param>
        /// <param name="authProvider">The authentication provider to use.</param>
        /// <param name="storageDirectoryPath">The path to the storage directory.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created in-app wallet.</returns>
        /// <exception cref="ArgumentException">Thrown when required parameters are not provided.</exception>
        public static async Task<InAppWallet> Create(
            ThirdwebClient client,
            string email = null,
            string phoneNumber = null,
            AuthProvider authProvider = AuthProvider.Default,
            string storageDirectoryPath = null
        )
        {
            if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(phoneNumber) && authProvider == AuthProvider.Default)
            {
                throw new ArgumentException("Email, Phone Number, or OAuth Provider must be provided to login.");
            }

            var authproviderStr = authProvider switch
            {
                AuthProvider.Google => "Google",
                AuthProvider.Apple => "Apple",
                AuthProvider.Facebook => "Facebook",
                AuthProvider.JWT => "JWT",
                AuthProvider.AuthEndpoint => "AuthEndpoint",
                AuthProvider.Discord => "Discord",
                AuthProvider.Farcaster => "Farcaster",
                AuthProvider.Telegram => "Telegram",
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

        /// <summary>
        /// Disconnects the wallet.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task Disconnect()
        {
            await base.Disconnect();
            await _embeddedWallet.SignOutAsync();
        }

        #region OAuth2 Flow

        /// <summary>
        /// Logs in with OAuth2.
        /// </summary>
        /// <param name="isMobile">Indicates if the login is from a mobile device.</param>
        /// <param name="browserOpenAction">The action to open the browser.</param>
        /// <param name="mobileRedirectScheme">The mobile redirect scheme.</param>
        /// <param name="browser">The browser instance.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the login result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when required parameters are not provided.</exception>
        /// <exception cref="TaskCanceledException">Thrown when the operation is canceled.</exception>
        /// <exception cref="TimeoutException">Thrown when the operation times out.</exception>
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

            var res = await _embeddedWallet.SignInWithOauthAsync(_authProvider, authResultJson);
            if (res.User == null)
            {
                throw new Exception("Failed to login with OAuth2");
            }
            _ecKey = new EthECKey(res.User.Account.PrivateKey);
            return await GetAddress();
        }

        #endregion

        #region OTP Flow

        /// <summary>
        /// Sends an OTP to the user's email or phone number.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when email or phone number is not provided.</exception>
        public async Task SendOTP()
        {
            if (string.IsNullOrEmpty(_email) && string.IsNullOrEmpty(_phoneNumber))
            {
                throw new Exception("Email or Phone Number is required for OTP login");
            }

            try
            {
                (var isNewUser, var isNewDevice) = _email == null ? await _embeddedWallet.SendPhoneOtpAsync(_phoneNumber) : await _embeddedWallet.SendEmailOtpAsync(_email);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to send OTP email", e);
            }
        }

        /// <summary>
        /// Submits the OTP for verification.
        /// </summary>
        /// <param name="otp">The OTP to submit.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the address and a boolean indicating if retry is possible.</returns>
        /// <exception cref="ArgumentNullException">Thrown when OTP is not provided.</exception>
        /// <exception cref="Exception">Thrown when email or phone number is not provided.</exception>
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

            var res = _email == null ? await _embeddedWallet.VerifyPhoneOtpAsync(_phoneNumber, otp) : await _embeddedWallet.VerifyEmailOtpAsync(_email, otp);
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

        /// <summary>
        /// Gets the email associated with the in-app wallet.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. The task result contains the email address.</returns>
        public Task<string> GetEmail()
        {
            return Task.FromResult(_email);
        }

        /// <summary>
        /// Gets the phone number associated with the in-app wallet.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. The task result contains the phone number.</returns>
        public Task<string> GetPhoneNumber()
        {
            return Task.FromResult(_phoneNumber);
        }

        #endregion

        #region JWT Flow

        /// <summary>
        /// Logs in with a JWT.
        /// </summary>
        /// <param name="jwt">The JWT to use for authentication.</param>
        /// <param name="encryptionKey">The encryption key to use.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the login result.</returns>
        /// <exception cref="ArgumentException">Thrown when JWT or encryption key is not provided.</exception>
        /// <exception cref="Exception">Thrown when the login fails.</exception>
        public async Task<string> LoginWithJWT(string jwt, string encryptionKey)
        {
            if (string.IsNullOrEmpty(jwt))
            {
                throw new ArgumentException(nameof(jwt), "JWT cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(encryptionKey))
            {
                throw new ArgumentException(nameof(encryptionKey), "Encryption key cannot be null or empty.");
            }

            var res = await _embeddedWallet.SignInWithJwtAsync(jwt, encryptionKey);

            if (res.User == null)
            {
                throw new Exception("Failed to login with JWT");
            }

            _ecKey = new EthECKey(res.User.Account.PrivateKey);

            return await GetAddress();
        }

        #endregion

        #region Auth Endpoint Flow

        /// <summary>
        /// Logs in with an authentication endpoint.
        /// </summary>
        /// <param name="payload">The payload to use for authentication.</param>
        /// <param name="encryptionKey">The encryption key to use.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the login result.</returns>
        /// <exception cref="ArgumentException">Thrown when payload or encryption key is not provided.</exception>
        /// <exception cref="Exception">Thrown when the login fails.</exception>
        public async Task<string> LoginWithAuthEndpoint(string payload, string encryptionKey)
        {
            if (string.IsNullOrEmpty(payload))
            {
                throw new ArgumentException(nameof(payload), "Payload cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(encryptionKey))
            {
                throw new ArgumentException(nameof(encryptionKey), "Encryption key cannot be null or empty.");
            }

            var res = await _embeddedWallet.SignInWithAuthEndpointAsync(payload, encryptionKey);

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
