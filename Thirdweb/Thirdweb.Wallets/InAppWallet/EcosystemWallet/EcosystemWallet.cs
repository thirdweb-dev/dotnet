using System.Numerics;
using System.Text;
using System.Web;
using Nethereum.ABI.EIP712;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;
using Newtonsoft.Json;
using Thirdweb.EWS;

namespace Thirdweb;

/// <summary>
/// Enclave based secure cross ecosystem wallet.
/// </summary>
public partial class EcosystemWallet : IThirdwebWallet
{
    public ThirdwebClient Client { get; }
    public ThirdwebAccountType AccountType => ThirdwebAccountType.PrivateKeyAccount;

    internal readonly EmbeddedWallet EmbeddedWallet;
    internal readonly IThirdwebHttpClient HttpClient;
    internal readonly IThirdwebWallet SiweSigner;
    internal readonly string Email;
    internal readonly string PhoneNumber;
    internal readonly string AuthProvider;
    internal readonly string LegacyEncryptionKey;

    internal string Address;

    private readonly string _ecosystemId;
    private readonly string _ecosystemPartnerId;

    private const string EMBEDDED_WALLET_BASE_PATH = "https://embedded-wallet.thirdweb.com/api";
    private const string EMBEDDED_WALLET_PATH_2024 = $"{EMBEDDED_WALLET_BASE_PATH}/2024-05-05";
    private const string EMBEDDED_WALLET_PATH_V1 = $"{EMBEDDED_WALLET_BASE_PATH}/v1";
    private const string ENCLAVE_PATH = $"{EMBEDDED_WALLET_PATH_V1}/enclave-wallet";

    internal EcosystemWallet(
        string ecosystemId,
        string ecosystemPartnerId,
        ThirdwebClient client,
        EmbeddedWallet embeddedWallet,
        IThirdwebHttpClient httpClient,
        string email,
        string phoneNumber,
        string authProvider,
        IThirdwebWallet siweSigner,
        string legacyEncryptionKey
    )
    {
        this.Client = client;
        this._ecosystemId = ecosystemId;
        this._ecosystemPartnerId = ecosystemPartnerId;
        this.LegacyEncryptionKey = legacyEncryptionKey;
        this.EmbeddedWallet = embeddedWallet;
        this.HttpClient = httpClient;
        this.Email = email;
        this.PhoneNumber = phoneNumber;
        this.AuthProvider = authProvider;
        this.SiweSigner = siweSigner;
    }

    #region Creation

    /// <summary>
    /// Creates a new instance of the <see cref="EcosystemWallet"/> class.
    /// </summary>
    /// <param name="ecosystemId">Your ecosystem ID (see thirdweb dashboard e.g. ecosystem.the-bonfire).</param>
    /// <param name="ecosystemPartnerId">Your ecosystem partner ID (required if you are integrating someone else's ecosystem).</param>
    /// <param name="client">The Thirdweb client instance.</param>
    /// <param name="email">The email address for Email OTP authentication.</param>
    /// <param name="phoneNumber">The phone number for Phone OTP authentication.</param>
    /// <param name="authProvider">The authentication provider to use.</param>
    /// <param name="storageDirectoryPath">The path to the storage directory.</param>
    /// <param name="siweSigner">The SIWE signer wallet for SIWE authentication.</param>
    /// <param name="legacyEncryptionKey">The encryption key that is no longer required but was used in the past. Only pass this if you had used custom auth before this was deprecated.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created in-app wallet.</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are not provided.</exception>
    public static async Task<EcosystemWallet> Create(
        ThirdwebClient client,
        string ecosystemId,
        string ecosystemPartnerId = null,
        string email = null,
        string phoneNumber = null,
        AuthProvider authProvider = Thirdweb.AuthProvider.Default,
        string storageDirectoryPath = null,
        IThirdwebWallet siweSigner = null,
        string legacyEncryptionKey = null
    )
    {
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client), "Client cannot be null.");
        }

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
            Thirdweb.AuthProvider.Steam => "Steam",
            Thirdweb.AuthProvider.Default => string.IsNullOrEmpty(email) ? "Phone" : "Email",
            _ => throw new ArgumentException("Invalid AuthProvider"),
        };

        var headers = client.HttpClient.Headers.ToDictionary(entry => entry.Key, entry => entry.Value);
        var platform = client.HttpClient.Headers["x-sdk-platform"];
        var version = client.HttpClient.Headers["x-sdk-version"];
        if (!string.IsNullOrEmpty(client.ClientId))
        {
            headers.Add("x-thirdweb-client-id", client.ClientId);
        }
        if (!string.IsNullOrEmpty(client.SecretKey))
        {
            headers.Add("x-thirdweb-secret-key", client.SecretKey);
        }
        headers.Add("x-session-nonce", Guid.NewGuid().ToString());
        headers.Add("x-embedded-wallet-version", $"{platform}:{version}");
        if (!string.IsNullOrEmpty(ecosystemId))
        {
            headers.Add("x-ecosystem-id", ecosystemId);
            if (!string.IsNullOrEmpty(ecosystemPartnerId))
            {
                headers.Add("x-ecosystem-partner-id", ecosystemPartnerId);
            }
        }
        var enclaveHttpClient = Utils.ReconstructHttpClient(client.HttpClient, headers);

        storageDirectoryPath ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Thirdweb", "EcosystemWallet");
        var embeddedWallet = new EmbeddedWallet(client, storageDirectoryPath, ecosystemId, ecosystemPartnerId);

        try
        {
            var userAddress = await ResumeEnclaveSession(enclaveHttpClient, embeddedWallet, email, phoneNumber, authproviderStr).ConfigureAwait(false);
            return new EcosystemWallet(ecosystemId, ecosystemPartnerId, client, embeddedWallet, enclaveHttpClient, email, phoneNumber, authproviderStr, siweSigner, legacyEncryptionKey)
            {
                Address = userAddress
            };
        }
        catch
        {
            enclaveHttpClient.RemoveHeader("Authorization");
            return new EcosystemWallet(ecosystemId, ecosystemPartnerId, client, embeddedWallet, enclaveHttpClient, email, phoneNumber, authproviderStr, siweSigner, legacyEncryptionKey)
            {
                Address = null
            };
        }
    }

    private static async Task<string> ResumeEnclaveSession(IThirdwebHttpClient httpClient, EmbeddedWallet embeddedWallet, string email, string phone, string authProvider)
    {
        email = email?.ToLower();

        var sessionData = embeddedWallet.GetSessionData();

        if (string.IsNullOrEmpty(sessionData.AuthToken))
        {
            throw new InvalidOperationException("User is not signed in");
        }

        if (sessionData.EmailAddress != email || sessionData.PhoneNumber != phone || sessionData.AuthProvider != authProvider)
        {
            throw new InvalidOperationException("Saved session data does not match provided details");
        }

        httpClient.AddHeader("Authorization", $"Bearer embedded-wallet-token:{sessionData.AuthToken}");

        var userStatus = await GetUserStatus(httpClient).ConfigureAwait(false);
        if (userStatus.Wallets[0].Type == "enclave")
        {
            return userStatus.Wallets[0].Address.ToChecksumAddress();
        }
        else
        {
            await embeddedWallet.SignOutAsync().ConfigureAwait(false);
            throw new InvalidOperationException("Must auth again to perform migration.");
        }
    }

    private static void CreateEnclaveSession(EmbeddedWallet embeddedWallet, string authToken, string email, string phone, string authProvider, string authIdentifier)
    {
        var data = new LocalStorage.DataStorage(authToken, null, email, phone, null, authProvider, authIdentifier);
        embeddedWallet.UpdateSessionData(data);
    }

    private static async Task<UserStatusResponse> GetUserStatus(IThirdwebHttpClient httpClient)
    {
        var url = $"{EMBEDDED_WALLET_PATH_2024}/accounts";
        var response = await httpClient.GetAsync(url).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var userStatus = JsonConvert.DeserializeObject<UserStatusResponse>(content);
        return userStatus;
    }

    private static async Task<string> GenerateWallet(IThirdwebHttpClient httpClient)
    {
        var url = $"{ENCLAVE_PATH}/generate";
        var requestContent = new StringContent("", Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(url, requestContent).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var enclaveResponse = JsonConvert.DeserializeObject<EnclaveGenerateResponse>(content);
        return enclaveResponse.Wallet.Address.ToChecksumAddress();
    }

    private async Task<string> PostAuth(Server.VerifyResult result)
    {
        this.HttpClient.AddHeader("Authorization", $"Bearer embedded-wallet-token:{result.AuthToken}");

        string address;
        if (result.IsNewUser)
        {
            address = await GenerateWallet(this.HttpClient).ConfigureAwait(false);
        }
        else
        {
            var userStatus = await GetUserStatus(this.HttpClient).ConfigureAwait(false);
            if (userStatus.Wallets[0].Type == "enclave")
            {
                address = userStatus.Wallets[0].Address;
            }
            else
            {
                address = await this.MigrateShardToEnclave(result).ConfigureAwait(false);
            }
        }

        if (string.IsNullOrEmpty(address))
        {
            throw new InvalidOperationException("Failed to get user address from enclave wallet.");
        }
        else
        {
            CreateEnclaveSession(this.EmbeddedWallet, result.AuthToken, this.Email, this.PhoneNumber, this.AuthProvider, result.AuthIdentifier);
            this.Address = address.ToChecksumAddress();
            return this.Address;
        }
    }

    private async Task<string> MigrateShardToEnclave(Server.VerifyResult authResult)
    {
        // TODO: For recovery code, allow old encryption keys as overrides to migrate sharded custom auth?
        var (address, encryptedPrivateKeyB64, ivB64, kmsCiphertextB64) = await this.EmbeddedWallet
            .GenerateEncryptionDataAsync(authResult.AuthToken, this.LegacyEncryptionKey ?? authResult.RecoveryCode)
            .ConfigureAwait(false);

        var url = $"{ENCLAVE_PATH}/migrate";
        var payload = new
        {
            address,
            encryptedPrivateKeyB64,
            ivB64,
            kmsCiphertextB64
        };
        var requestContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

        var response = await this.HttpClient.PostAsync(url, requestContent).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        var userStatus = await GetUserStatus(this.HttpClient).ConfigureAwait(false);
        return userStatus.Wallets[0].Address;
    }

    #endregion

    #region Wallet Specific

    /// <summary>
    /// Gets the user details from the enclave wallet.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user details.</returns>
    public async Task<UserStatusResponse> GetUserDetails()
    {
        return await GetUserStatus(this.HttpClient).ConfigureAwait(false);
    }

    [Obsolete("Use GetUserDetails instead.")]
    public string GetEmail()
    {
        return this.Email;
    }

    [Obsolete("Use GetUserDetails instead.")]
    public string GetPhoneNumber()
    {
        return this.PhoneNumber;
    }

    public async Task<EcosystemDetails> GetEcosystemDetails()
    {
        var url = $"{EMBEDDED_WALLET_PATH_2024}/ecosystem-wallet";
        var response = await this.HttpClient.GetAsync(url).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonConvert.DeserializeObject<EcosystemDetails>(content);
    }

    public string GenerateExternalLoginLink(string redirectUrl)
    {
        var authProvider = HttpUtility.UrlEncode(this.AuthProvider.ToLower());
        var walletId = this.GetType().Name.Contains("InAppWallet") ? "inApp" : this._ecosystemId;
        var authCookie = HttpUtility.UrlEncode(this.EmbeddedWallet.GetSessionData()?.AuthToken ?? "");

        if (string.IsNullOrEmpty(authCookie))
        {
            throw new InvalidOperationException("Cannot generate external login link without an active session.");
        }

        var queryString = redirectUrl.Contains('?') ? "&" : "?" + $"walletId={walletId}&authProvider={authProvider}&authCookie={authCookie}";
        return $"{redirectUrl}{queryString}";
    }

    #endregion

    #region Account Linking

    public async Task<List<LinkedAccount>> LinkAccount(
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

        if (walletToLink is not EcosystemWallet ecosystemWallet)
        {
            throw new ArgumentException("Cannot link account with a non-EcosystemWallet wallet.");
        }

        if (await ecosystemWallet.IsConnected().ConfigureAwait(false))
        {
            throw new ArgumentException("Cannot link account with a wallet that is already created and connected.");
        }

        Server.VerifyResult serverRes = null;
        switch (ecosystemWallet.AuthProvider)
        {
            case "Email":
                if (string.IsNullOrEmpty(ecosystemWallet.Email))
                {
                    throw new ArgumentException("Cannot link account with an email wallet that does not have an email address.");
                }
                serverRes = await ecosystemWallet.PreAuth_Otp(otp).ConfigureAwait(false);
                break;
            case "Phone":
                if (string.IsNullOrEmpty(ecosystemWallet.PhoneNumber))
                {
                    throw new ArgumentException("Cannot link account with a phone wallet that does not have a phone number.");
                }
                serverRes = await ecosystemWallet.PreAuth_Otp(otp).ConfigureAwait(false);
                break;
            case "Siwe":
                if (ecosystemWallet.SiweSigner == null || chainId == null)
                {
                    throw new ArgumentException("Cannot link account with a Siwe wallet without a signer and chain ID.");
                }
                serverRes = await ecosystemWallet.PreAuth_Siwe(ecosystemWallet.SiweSigner, chainId.Value).ConfigureAwait(false);
                break;
            case "JWT":
                if (string.IsNullOrEmpty(jwt))
                {
                    throw new ArgumentException("Cannot link account with a JWT wallet without a JWT.");
                }
                serverRes = await ecosystemWallet.PreAuth_JWT(jwt).ConfigureAwait(false);
                break;
            case "AuthEndpoint":
                if (string.IsNullOrEmpty(payload))
                {
                    throw new ArgumentException("Cannot link account with an AuthEndpoint wallet without a payload.");
                }
                serverRes = await ecosystemWallet.PreAuth_AuthEndpoint(payload).ConfigureAwait(false);
                break;
            case "Guest":
                serverRes = await ecosystemWallet.PreAuth_Guest().ConfigureAwait(false);
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
            case "Steam":
                serverRes = await ecosystemWallet.PreAuth_OAuth(isMobile ?? false, browserOpenAction, mobileRedirectScheme, browser).ConfigureAwait(false);
                break;
            default:
                throw new ArgumentException($"Cannot link account with an unsupported authentication provider:", ecosystemWallet.AuthProvider);
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

    public async Task<List<LinkedAccount>> GetLinkedAccounts()
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

    #region OTP Auth

    public async Task SendOTP()
    {
        if (string.IsNullOrEmpty(this.Email) && string.IsNullOrEmpty(this.PhoneNumber))
        {
            throw new Exception("Email or Phone Number is required for OTP login");
        }

        try
        {
            if (this.Email == null)
            {
                await this.EmbeddedWallet.SendPhoneOtpAsync(this.PhoneNumber).ConfigureAwait(false);
            }
            else
            {
                await this.EmbeddedWallet.SendEmailOtpAsync(this.Email).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            throw new Exception("Failed to send OTP", e);
        }
    }

    private async Task<Server.VerifyResult> PreAuth_Otp(string otp)
    {
        if (string.IsNullOrEmpty(otp))
        {
            throw new ArgumentNullException(nameof(otp), "OTP cannot be null or empty.");
        }

        var serverRes =
            string.IsNullOrEmpty(this.Email) && string.IsNullOrEmpty(this.PhoneNumber)
                ? throw new Exception("Email or Phone Number is required for OTP login")
                : this.Email == null
                    ? await this.EmbeddedWallet.VerifyPhoneOtpAsync(this.PhoneNumber, otp).ConfigureAwait(false)
                    : await this.EmbeddedWallet.VerifyEmailOtpAsync(this.Email, otp).ConfigureAwait(false);

        return serverRes;
    }

    public async Task<string> LoginWithOtp(string otp)
    {
        var serverRes = await this.PreAuth_Otp(otp).ConfigureAwait(false);
        return await this.PostAuth(serverRes).ConfigureAwait(false);
    }

    #endregion

    #region OAuth

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

        var platform = this.HttpClient?.Headers?["x-sdk-name"] == "UnitySDK_WebGL" ? "web" : "dotnet";
        var redirectUrl = isMobile ? mobileRedirectScheme : "http://localhost:8789/";
        var loginUrl = await this.EmbeddedWallet.FetchHeadlessOauthLoginLinkAsync(this.AuthProvider, platform).ConfigureAwait(false);
        loginUrl = platform == "web" ? loginUrl : $"{loginUrl}&redirectUrl={redirectUrl}&developerClientId={this.Client.ClientId}&authOption={this.AuthProvider}";
        if (!string.IsNullOrEmpty(this._ecosystemId))
        {
            loginUrl = $"{loginUrl}&ecosystemId={this._ecosystemId}";
            if (!string.IsNullOrEmpty(this._ecosystemPartnerId))
            {
                loginUrl = $"{loginUrl}&ecosystemPartnerId={this._ecosystemPartnerId}";
            }
        }

        browser ??= new InAppWalletBrowser();
        var browserResult = await browser.Login(this.Client, loginUrl, redirectUrl, browserOpenAction, cancellationToken).ConfigureAwait(false);
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

        var serverRes = await this.EmbeddedWallet.SignInWithOauthAsync(authResultJson).ConfigureAwait(false);
        return serverRes;
    }

    public async Task<string> LoginWithOauth(
        bool isMobile,
        Action<string> browserOpenAction,
        string mobileRedirectScheme = "thirdweb://",
        IThirdwebBrowser browser = null,
        CancellationToken cancellationToken = default
    )
    {
        var serverRes = await this.PreAuth_OAuth(isMobile, browserOpenAction, mobileRedirectScheme, browser, cancellationToken).ConfigureAwait(false);
        return await this.PostAuth(serverRes).ConfigureAwait(false);
    }

    #endregion

    #region Siwe

    private async Task<Server.VerifyResult> PreAuth_Siwe(IThirdwebWallet siweSigner, BigInteger chainId)
    {
        if (this.SiweSigner == null)
        {
            throw new ArgumentNullException(nameof(siweSigner), "SIWE Signer wallet cannot be null.");
        }

        if (!await this.SiweSigner.IsConnected().ConfigureAwait(false))
        {
            throw new InvalidOperationException("SIWE Signer wallet must be connected as this operation requires it to sign a message.");
        }

        var serverRes =
            chainId <= 0 ? throw new ArgumentException("Chain ID must be greater than 0.", nameof(chainId)) : await this.EmbeddedWallet.SignInWithSiweAsync(siweSigner, chainId).ConfigureAwait(false);

        return serverRes;
    }

    public async Task<string> LoginWithSiwe(BigInteger chainId)
    {
        var serverRes = await this.PreAuth_Siwe(this.SiweSigner, chainId).ConfigureAwait(false);
        return await this.PostAuth(serverRes).ConfigureAwait(false);
    }

    #endregion

    #region Guest

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

    public async Task<string> LoginWithGuest()
    {
        var serverRes = await this.PreAuth_Guest().ConfigureAwait(false);
        return await this.PostAuth(serverRes).ConfigureAwait(false);
    }

    #endregion

    #region JWT

    private async Task<Server.VerifyResult> PreAuth_JWT(string jwt)
    {
        return string.IsNullOrEmpty(jwt) ? throw new ArgumentException(nameof(jwt), "JWT cannot be null or empty.") : await this.EmbeddedWallet.SignInWithJwtAsync(jwt).ConfigureAwait(false);
    }

    public async Task<string> LoginWithJWT(string jwt)
    {
        var serverRes = string.IsNullOrEmpty(jwt) ? throw new ArgumentException("JWT cannot be null or empty.", nameof(jwt)) : await this.EmbeddedWallet.SignInWithJwtAsync(jwt).ConfigureAwait(false);

        return await this.PostAuth(serverRes).ConfigureAwait(false);
    }

    #endregion

    #region AuthEndpoint

    private async Task<Server.VerifyResult> PreAuth_AuthEndpoint(string payload)
    {
        var serverRes = string.IsNullOrEmpty(payload)
            ? throw new ArgumentNullException(nameof(payload), "Payload cannot be null or empty.")
            : await this.EmbeddedWallet.SignInWithAuthEndpointAsync(payload).ConfigureAwait(false);

        return serverRes;
    }

    public async Task<string> LoginWithAuthEndpoint(string payload)
    {
        var serverRes = await this.PreAuth_AuthEndpoint(payload).ConfigureAwait(false);
        return await this.PostAuth(serverRes).ConfigureAwait(false);
    }

    #endregion

    #region IThirdwebWallet

    public Task<string> GetAddress()
    {
        if (!string.IsNullOrEmpty(this.Address))
        {
            return Task.FromResult(this.Address.ToChecksumAddress());
        }
        else
        {
            return Task.FromResult(this.Address);
        }
    }

    public Task<string> EthSign(byte[] rawMessage)
    {
        if (rawMessage == null)
        {
            throw new ArgumentNullException(nameof(rawMessage), "Message to sign cannot be null.");
        }

        throw new NotImplementedException();
    }

    public Task<string> EthSign(string message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message), "Message to sign cannot be null.");
        }

        throw new NotImplementedException();
    }

    public async Task<string> PersonalSign(byte[] rawMessage)
    {
        if (rawMessage == null)
        {
            throw new ArgumentNullException(nameof(rawMessage), "Message to sign cannot be null.");
        }

        var url = $"{ENCLAVE_PATH}/sign-message";
        var payload = new { messagePayload = new { message = rawMessage.BytesToHex(), isRaw = true } };

        var requestContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

        var response = await this.HttpClient.PostAsync(url, requestContent).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var res = JsonConvert.DeserializeObject<EnclaveSignResponse>(content);
        return res.Signature;
    }

    public async Task<string> PersonalSign(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentNullException(nameof(message), "Message to sign cannot be null.");
        }

        var url = $"{ENCLAVE_PATH}/sign-message";
        var payload = new { messagePayload = new { message, isRaw = false } };

        var requestContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

        var response = await this.HttpClient.PostAsync(url, requestContent).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var res = JsonConvert.DeserializeObject<EnclaveSignResponse>(content);
        return res.Signature;
    }

    public async Task<string> SignTypedDataV4(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            throw new ArgumentNullException(nameof(json), "Json to sign cannot be null.");
        }

        var processedJson = Utils.PreprocessTypedDataJson(json);

        var url = $"{ENCLAVE_PATH}/sign-typed-data";

        var requestContent = new StringContent(processedJson, Encoding.UTF8, "application/json");

        var response = await this.HttpClient.PostAsync(url, requestContent).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var res = JsonConvert.DeserializeObject<EnclaveSignResponse>(content);
        return res.Signature;
    }

    public async Task<string> SignTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData)
        where TDomain : IDomain
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data), "Data to sign cannot be null.");
        }

        var safeJson = Utils.ToJsonExternalWalletFriendly(typedData, data);
        return await this.SignTypedDataV4(safeJson).ConfigureAwait(false);
    }

    public async Task<string> SignTransaction(ThirdwebTransactionInput transaction)
    {
        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        if (transaction.Nonce == null || transaction.Gas == null || transaction.To == null)
        {
            throw new ArgumentException("Nonce, Gas, and To fields are required for transaction signing.");
        }

        if (transaction.GasPrice == null && (transaction.MaxFeePerGas == null || transaction.MaxPriorityFeePerGas == null))
        {
            throw new ArgumentException("GasPrice or MaxFeePerGas and MaxPriorityFeePerGas are required for transaction signing.");
        }

        object payload = new { transactionPayload = transaction };

        var url = $"{ENCLAVE_PATH}/sign-transaction";

        var requestContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

        var response = await this.HttpClient.PostAsync(url, requestContent).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var res = JsonConvert.DeserializeObject<EnclaveSignResponse>(content);
        return res.Signature;
    }

    public Task<bool> IsConnected()
    {
        return Task.FromResult(this.Address != null);
    }

    public Task<string> SendTransaction(ThirdwebTransactionInput transaction)
    {
        throw new InvalidOperationException("SendTransaction is not supported for Ecosystem Wallets, please use the unified Contract or ThirdwebTransaction APIs.");
    }

    public Task<ThirdwebTransactionReceipt> ExecuteTransaction(ThirdwebTransactionInput transactionInput)
    {
        throw new InvalidOperationException("ExecuteTransaction is not supported for Ecosystem Wallets, please use the unified Contract or ThirdwebTransaction APIs.");
    }

    public async Task Disconnect()
    {
        this.Address = null;
        await this.EmbeddedWallet.SignOutAsync().ConfigureAwait(false);
    }

    public virtual Task<string> RecoverAddressFromEthSign(string message, string signature)
    {
        throw new InvalidOperationException();
    }

    public virtual Task<string> RecoverAddressFromPersonalSign(string message, string signature)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentNullException(nameof(message), "Message to sign cannot be null.");
        }

        if (string.IsNullOrEmpty(signature))
        {
            throw new ArgumentNullException(nameof(signature), "Signature cannot be null.");
        }

        var signer = new EthereumMessageSigner();
        var address = signer.EncodeUTF8AndEcRecover(message, signature);
        return Task.FromResult(address);
    }

    public virtual Task<string> RecoverAddressFromTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData, string signature)
        where TDomain : IDomain
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data), "Data to sign cannot be null.");
        }

        if (typedData == null)
        {
            throw new ArgumentNullException(nameof(typedData), "Typed data cannot be null.");
        }

        if (signature == null)
        {
            throw new ArgumentNullException(nameof(signature), "Signature cannot be null.");
        }

        var signer = new Eip712TypedDataSigner();
        var address = signer.RecoverFromSignatureV4(data, typedData, signature);
        return Task.FromResult(address);
    }

    #endregion
}
