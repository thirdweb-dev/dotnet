using System.Numerics;
using System.Web;
using Nethereum.Signer;
using Newtonsoft.Json;
using Thirdweb.EWS;

namespace Thirdweb;

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
    Telegram,
    Siwe,
    Line,
    Guest,
    X,
    Coinbase,
    Github,
    Twitch
}

public struct LinkedAccount
{
    public string Type { get; set; }
    public LinkedAccountDetails Details { get; set; }

    public struct LinkedAccountDetails
    {
        public string Email { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Id { get; set; }
    }

    public override readonly string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}

/// <summary>
/// Represents an in-app wallet that extends the functionality of a private key wallet.
/// </summary>
public class InAppWallet : PrivateKeyWallet
{
    internal EmbeddedWallet EmbeddedWallet;
    internal string Email;
    internal string PhoneNumber;
    internal string AuthProvider;
    internal IThirdwebWallet SiweSigner;

    internal InAppWallet(ThirdwebClient client, string email, string phoneNumber, string authProvider, EmbeddedWallet embeddedWallet, EthECKey ecKey, IThirdwebWallet siweSigner)
        : base(client, ecKey)
    {
        this.Email = email?.ToLower();
        this.PhoneNumber = phoneNumber;
        this.EmbeddedWallet = embeddedWallet;
        this.AuthProvider = authProvider;
        this.SiweSigner = siweSigner;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="InAppWallet"/> class.
    /// </summary>
    /// <param name="client">The Thirdweb client instance.</param>
    /// <param name="email">The email address for Email OTP authentication.</param>
    /// <param name="phoneNumber">The phone number for Phone OTP authentication.</param>
    /// <param name="authProvider">The authentication provider to use.</param>
    /// <param name="storageDirectoryPath">The path to the storage directory.</param>
    /// <param name="siweSigner">The SIWE signer wallet for SIWE authentication.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created in-app wallet.</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are not provided.</exception>
    public static async Task<InAppWallet> Create(
        ThirdwebClient client,
        string email = null,
        string phoneNumber = null,
        AuthProvider authProvider = Thirdweb.AuthProvider.Default,
        string storageDirectoryPath = null,
        IThirdwebWallet siweSigner = null
    )
    {
        if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(phoneNumber) && authProvider == Thirdweb.AuthProvider.Default)
        {
            throw new ArgumentException("Email, Phone Number, or OAuth Provider must be provided to login.");
        }

        var authproviderStr = authProvider switch
        {
            Thirdweb.AuthProvider.Google => "Google",
            Thirdweb.AuthProvider.Apple => "Apple",
            Thirdweb.AuthProvider.Facebook => "Facebook",
            Thirdweb.AuthProvider.JWT => "JWT",
            Thirdweb.AuthProvider.AuthEndpoint => "AuthEndpoint",
            Thirdweb.AuthProvider.Discord => "Discord",
            Thirdweb.AuthProvider.Farcaster => "Farcaster",
            Thirdweb.AuthProvider.Telegram => "Telegram",
            Thirdweb.AuthProvider.Siwe => "Siwe",
            Thirdweb.AuthProvider.Line => "Line",
            Thirdweb.AuthProvider.Guest => "Guest",
            Thirdweb.AuthProvider.X => "X",
            Thirdweb.AuthProvider.Coinbase => "Coinbase",
            Thirdweb.AuthProvider.Github => "Github",
            Thirdweb.AuthProvider.Twitch => "Twitch",
            Thirdweb.AuthProvider.Default => string.IsNullOrEmpty(email) ? "Phone" : "Email",
            _ => throw new ArgumentException("Invalid AuthProvider"),
        };

        storageDirectoryPath ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Thirdweb", "InAppWallet");
        var embeddedWallet = new EmbeddedWallet(client, storageDirectoryPath);
        EthECKey ecKey;
        try
        {
            var user = await embeddedWallet.GetUserAsync(email, phoneNumber, authproviderStr).ConfigureAwait(false);
            ecKey = new EthECKey(user.Account.PrivateKey);
        }
        catch
        {
            ecKey = null;
        }
        return new InAppWallet(client, email, phoneNumber, authproviderStr, embeddedWallet, ecKey, siweSigner);
    }

    /// <summary>
    /// Disconnects the wallet.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task Disconnect()
    {
        await base.Disconnect().ConfigureAwait(false);
        await this.EmbeddedWallet.SignOutAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the email associated with the in-app wallet.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The task result contains the email address.</returns>
    public Task<string> GetEmail()
    {
        return Task.FromResult(this.Email);
    }

    /// <summary>
    /// Gets the phone number associated with the in-app wallet.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The task result contains the phone number.</returns>
    public Task<string> GetPhoneNumber()
    {
        return Task.FromResult(this.PhoneNumber);
    }

    #region Account Linking

    public override async Task<List<LinkedAccount>> LinkAccount(
        IThirdwebWallet walletToLink,
        string otp = null,
        bool? isMobile = null,
        Action<string> browserOpenAction = null,
        string mobileRedirectScheme = "thirdweb://",
        IThirdwebBrowser browser = null,
        BigInteger? chainId = null,
        string jwt = null,
        string payload = null
    )
    {
        if (!await this.IsConnected().ConfigureAwait(false))
        {
            throw new InvalidOperationException("Cannot link account with a wallet that is not connected. Please login to the wallet before linking other wallets.");
        }

        if (walletToLink == null)
        {
            throw new ArgumentNullException(nameof(walletToLink), "Wallet to link cannot be null.");
        }

        if (walletToLink is not InAppWallet inAppWallet)
        {
            throw new ArgumentException("Cannot link account with a wallet that is not an InAppWallet.");
        }

        if (await inAppWallet.IsConnected().ConfigureAwait(false))
        {
            throw new ArgumentException("Cannot link account with a wallet that is already created and connected.");
        }

        Server.VerifyResult serverRes = null;
        switch (inAppWallet.AuthProvider)
        {
            case "Email":
                if (string.IsNullOrEmpty(inAppWallet.Email))
                {
                    throw new ArgumentException("Cannot link account with an email wallet that does not have an email address.");
                }
                serverRes = await inAppWallet.PreAuth_Otp(otp).ConfigureAwait(false);
                break;
            case "Phone":
                if (string.IsNullOrEmpty(inAppWallet.PhoneNumber))
                {
                    throw new ArgumentException("Cannot link account with a phone wallet that does not have a phone number.");
                }
                serverRes = await inAppWallet.PreAuth_Otp(otp).ConfigureAwait(false);
                break;
            case "Siwe":
                if (inAppWallet.SiweSigner == null || chainId == null)
                {
                    throw new ArgumentException("Cannot link account with a Siwe wallet without a signer and chain ID.");
                }
                serverRes = await inAppWallet.PreAuth_Siwe(inAppWallet.SiweSigner, chainId.Value).ConfigureAwait(false);
                break;
            case "JWT":
                if (string.IsNullOrEmpty(jwt))
                {
                    throw new ArgumentException("Cannot link account with a JWT wallet without a JWT.");
                }
                serverRes = await inAppWallet.PreAuth_JWT(jwt).ConfigureAwait(false);
                break;
            case "AuthEndpoint":
                if (string.IsNullOrEmpty(payload))
                {
                    throw new ArgumentException("Cannot link account with an AuthEndpoint wallet without a payload.");
                }
                serverRes = await inAppWallet.PreAuth_AuthEndpoint(payload).ConfigureAwait(false);
                break;
            case "Guest":
                serverRes = await inAppWallet.PreAuth_Guest().ConfigureAwait(false);
                break;
            case "Google":
            case "Apple":
            case "Facebook":
            case "Discord":
            case "Farcaster":
            case "Telegram":
            case "Line":
            case "X":
            case "Coinbase":
            case "Github":
            case "Twitch":
                serverRes = await inAppWallet.PreAuth_OAuth(isMobile ?? false, browserOpenAction, mobileRedirectScheme, browser).ConfigureAwait(false);
                break;
            default:
                throw new ArgumentException($"Cannot link account with an unsupported authentication provider:", inAppWallet.AuthProvider);
        }

        var currentAccountToken = this.EmbeddedWallet.GetSessionData()?.AuthToken;
        var authTokenToConnect = serverRes.AuthToken;

        var serverLinkedAccounts = await this.EmbeddedWallet.LinkAccountAsync(currentAccountToken, authTokenToConnect).ConfigureAwait(false);
        var linkedAccounts = new List<LinkedAccount>();
        foreach (var linkedAccount in serverLinkedAccounts)
        {
            linkedAccounts.Add(
                new LinkedAccount
                {
                    Type = linkedAccount.Type,
                    Details = new LinkedAccount.LinkedAccountDetails
                    {
                        Email = linkedAccount.Details?.Email,
                        Address = linkedAccount.Details?.Address,
                        Phone = linkedAccount.Details?.Phone,
                        Id = linkedAccount.Details?.Id
                    }
                }
            );
        }
        return linkedAccounts;
    }

    public override async Task<List<LinkedAccount>> GetLinkedAccounts()
    {
        var currentAccountToken = this.EmbeddedWallet.GetSessionData()?.AuthToken;
        var serverLinkedAccounts = await this.EmbeddedWallet.GetLinkedAccountsAsync(currentAccountToken).ConfigureAwait(false);
        var linkedAccounts = new List<LinkedAccount>();
        foreach (var linkedAccount in serverLinkedAccounts)
        {
            linkedAccounts.Add(
                new LinkedAccount
                {
                    Type = linkedAccount.Type,
                    Details = new LinkedAccount.LinkedAccountDetails
                    {
                        Email = linkedAccount.Details?.Email,
                        Address = linkedAccount.Details?.Address,
                        Phone = linkedAccount.Details?.Phone,
                        Id = linkedAccount.Details?.Id
                    }
                }
            );
        }
        return linkedAccounts;
    }

    #endregion

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
        var serverRes = await this.PreAuth_OAuth(isMobile, browserOpenAction, mobileRedirectScheme, browser, cancellationToken).ConfigureAwait(false);
        return await this.PostAuth(serverRes, null, this.AuthProvider).ConfigureAwait(false);
    }

    private async Task<Server.VerifyResult> PreAuth_OAuth(
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

        var platform = this.Client.HttpClient?.Headers?["x-sdk-name"] == "UnitySDK_WebGL" ? "web" : "dotnet";
        var redirectUrl = isMobile ? mobileRedirectScheme : "http://localhost:8789/";
        var loginUrl = await this.EmbeddedWallet.FetchHeadlessOauthLoginLinkAsync(this.AuthProvider, platform);
        loginUrl = platform == "web" ? loginUrl : $"{loginUrl}&redirectUrl={redirectUrl}&developerClientId={this.Client.ClientId}&authOption={this.AuthProvider}";

        browser ??= new InAppWalletBrowser();
        var browserResult = await browser.Login(this.Client, loginUrl, redirectUrl, browserOpenAction, cancellationToken);
        switch (browserResult.Status)
        {
            case BrowserStatus.Success:
                break;
            case BrowserStatus.UserCanceled:
                throw new TaskCanceledException(browserResult.Error ?? "LoginWithOauth was cancelled.");
            case BrowserStatus.Timeout:
                throw new TimeoutException(browserResult.Error ?? "LoginWithOauth timed out.");
            case BrowserStatus.UnknownError:
            default:
                throw new Exception($"Failed to login with {this.AuthProvider}: {browserResult.Status} | {browserResult.Error}");
        }
        var callbackUrl =
            browserResult.Status != BrowserStatus.Success
                ? throw new Exception($"Failed to login with {this.AuthProvider}: {browserResult.Status} | {browserResult.Error}")
                : browserResult.CallbackUrl;

        while (string.IsNullOrEmpty(callbackUrl))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException("LoginWithOauth was cancelled.");
            }
            await ThirdwebTask.Delay(100, cancellationToken).ConfigureAwait(false);
        }

        var authResultJson = callbackUrl;
        if (!authResultJson.StartsWith('{'))
        {
            var decodedUrl = HttpUtility.UrlDecode(callbackUrl);
            Uri uri = new(decodedUrl);
            var queryString = uri.Query;
            var queryDict = HttpUtility.ParseQueryString(queryString);
            authResultJson = queryDict["authResult"];
        }

        return await this.EmbeddedWallet.SignInWithOauthAsync(authResultJson);
    }

    #endregion

    #region OTP Flow

    /// <summary>
    /// Sends an OTP to the user's email or phone number.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The task result contains a boolean indicating if the user is new and a boolean indicating if the device is new.</returns>
    /// <exception cref="Exception">Thrown when email or phone number is not provided.</exception>
    public async Task<(bool isNewUser, bool isNewDevice)> SendOTP()
    {
        if (string.IsNullOrEmpty(this.Email) && string.IsNullOrEmpty(this.PhoneNumber))
        {
            throw new Exception("Email or Phone Number is required for OTP login");
        }

        try
        {
            return this.Email == null
                ? await this.EmbeddedWallet.SendPhoneOtpAsync(this.PhoneNumber).ConfigureAwait(false)
                : await this.EmbeddedWallet.SendEmailOtpAsync(this.Email).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            throw new Exception("Failed to send OTP", e);
        }
    }

    /// <summary>
    /// Submits the OTP for verification.
    /// </summary>
    /// <param name="otp">The OTP to submit.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the address and a boolean indicating if retry is possible.</returns>
    /// <exception cref="ArgumentNullException">Thrown when OTP is not provided.</exception>
    /// <exception cref="Exception">Thrown when email or phone number is not provided.</exception>
    public async Task<(string address, bool canRetry)> LoginWithOtp(string otp)
    {
        if (string.IsNullOrEmpty(otp))
        {
            throw new ArgumentNullException(nameof(otp), "OTP cannot be null or empty.");
        }

        var serverRes = await this.PreAuth_Otp(otp).ConfigureAwait(false);
        try
        {
            return (await this.PostAuth(serverRes, null, this.Email == null ? "Email" : "Phone").ConfigureAwait(false), false);
        }
        catch (VerificationException e)
        {
            return (null, e.CanRetry);
        }
    }

    private async Task<Server.VerifyResult> PreAuth_Otp(string otp)
    {
        if (string.IsNullOrEmpty(otp))
        {
            throw new ArgumentNullException(nameof(otp), "OTP cannot be null or empty.");
        }

        return string.IsNullOrEmpty(this.Email) && string.IsNullOrEmpty(this.PhoneNumber)
            ? throw new Exception("Email or Phone Number is required for OTP login")
            : this.Email == null
                ? await this.EmbeddedWallet.VerifyPhoneOtpAsync(this.PhoneNumber, otp).ConfigureAwait(false)
                : await this.EmbeddedWallet.VerifyEmailOtpAsync(this.Email, otp).ConfigureAwait(false);
    }

    #endregion

    #region SIWE Flow

    /// <summary>
    /// Logs in with SIWE (Sign-In with Ethereum).
    /// </summary>
    /// <param name="chainId">The chain ID to use for signing the SIWE payload</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the address.</returns>
    /// <exception cref="ArgumentNullException">Thrown when external wallet is not provided.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the external wallet is not connected.</exception>
    /// <exception cref="ArgumentException">Thrown when chain ID is invalid.</exception>
    public async Task<string> LoginWithSiwe(BigInteger chainId)
    {
        var serverRes = await this.PreAuth_Siwe(this.SiweSigner, chainId).ConfigureAwait(false);
        return await this.PostAuth(serverRes, null, "Siwe").ConfigureAwait(false);
    }

    private async Task<Server.VerifyResult> PreAuth_Siwe(IThirdwebWallet signer, BigInteger chainId)
    {
        if (signer == null)
        {
            throw new ArgumentNullException(nameof(signer), "SIWE Signer wallet cannot be null.");
        }

        if (!await signer.IsConnected().ConfigureAwait(false))
        {
            throw new InvalidOperationException("SIWE Signer wallet must be connected as this operation requires it to sign a message.");
        }

        return chainId <= 0 ? throw new ArgumentException(nameof(chainId), "Chain ID must be greater than 0.") : await this.EmbeddedWallet.SignInWithSiweAsync(signer, chainId).ConfigureAwait(false);
    }

    #endregion

    #region Guest

    public async Task<string> LoginWithGuest()
    {
        var serverRes = await this.PreAuth_Guest().ConfigureAwait(false);
        return await this.PostAuth(serverRes, null, "Guest").ConfigureAwait(false);
    }

    private async Task<Server.VerifyResult> PreAuth_Guest()
    {
        var sessionData = this.EmbeddedWallet.GetSessionData();
        string sessionId;
        if (sessionData != null && sessionData.AuthProvider == "Guest" && !string.IsNullOrEmpty(sessionData.AuthIdentifier))
        {
            sessionId = sessionData.AuthIdentifier;
        }
        else
        {
            sessionId = Guid.NewGuid().ToString();
        }
        var serverRes = await this.EmbeddedWallet.SignInWithGuestAsync(sessionId).ConfigureAwait(false);
        return serverRes;
    }

    #endregion

    #region JWT Flow

    /// <summary>
    /// Logs in with a JWT.
    /// </summary>
    /// <param name="jwt">The JWT to use for authentication.</param>
    /// <param name="encryptionKey">The encryption key to use.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the address.</returns>
    /// <exception cref="ArgumentException">Thrown when JWT or encryption key is not provided.</exception>
    /// <exception cref="Exception">Thrown when the login fails.</exception>
    public async Task<string> LoginWithJWT(string jwt, string encryptionKey)
    {
        if (string.IsNullOrEmpty(encryptionKey))
        {
            throw new ArgumentException(nameof(encryptionKey), "Encryption key cannot be null or empty.");
        }

        var serverRes = await this.PreAuth_JWT(jwt).ConfigureAwait(false);
        return await this.PostAuth(serverRes, encryptionKey, "JWT").ConfigureAwait(false);
    }

    private async Task<Server.VerifyResult> PreAuth_JWT(string jwt)
    {
        return string.IsNullOrEmpty(jwt) ? throw new ArgumentException(nameof(jwt), "JWT cannot be null or empty.") : await this.EmbeddedWallet.SignInWithJwtAsync(jwt).ConfigureAwait(false);
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
        if (string.IsNullOrEmpty(encryptionKey))
        {
            throw new ArgumentException(nameof(encryptionKey), "Encryption key cannot be null or empty.");
        }

        var serverRes = await this.PreAuth_AuthEndpoint(payload).ConfigureAwait(false);
        return await this.PostAuth(serverRes, encryptionKey, "AuthEndpoint").ConfigureAwait(false);
    }

    private async Task<Server.VerifyResult> PreAuth_AuthEndpoint(string payload)
    {
        return string.IsNullOrEmpty(payload)
            ? throw new ArgumentException(nameof(payload), "Payload cannot be null or empty.")
            : await this.EmbeddedWallet.SignInWithAuthEndpointAsync(payload).ConfigureAwait(false);
    }

    #endregion

    private async Task<string> PostAuth(Server.VerifyResult serverRes, string encryptionKey, string authProvider)
    {
        var res = await this.EmbeddedWallet.PostAuthSetup(serverRes, encryptionKey, authProvider).ConfigureAwait(false);
        if (res.User == null)
        {
            throw new Exception($"Failed to login with {authProvider}");
        }
        this.EcKey = new EthECKey(res.User.Account.PrivateKey);
        return await this.GetAddress().ConfigureAwait(false);
    }
}
