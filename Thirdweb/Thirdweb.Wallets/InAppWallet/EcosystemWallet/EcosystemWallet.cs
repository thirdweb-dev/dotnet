using System.Numerics;
using System.Text;
using System.Web;
using Nethereum.ABI.EIP712;
using Newtonsoft.Json;
using Thirdweb.EWS;

namespace Thirdweb;

/// <summary>
/// Enclave based secure cross ecosystem wallet.
/// </summary>
public partial class EcosystemWallet : PrivateKeyWallet
{
    private readonly EmbeddedWallet _embeddedWallet;
    private readonly IThirdwebHttpClient _httpClient;
    private readonly IThirdwebWallet _siweSigner;
    private readonly string _email;
    private readonly string _phoneNumber;
    private readonly string _authProvider;

    private string _address;
    private string _authToken;

    private const string EnclavePath = "https://embedded-wallet.thirdweb-dev.com/api/v1/enclave-wallet";

    private EcosystemWallet(
        ThirdwebClient client,
        EmbeddedWallet embeddedWallet,
        IThirdwebHttpClient httpClient,
        string email,
        string phoneNumber,
        string authProvider,
        IThirdwebWallet siweSigner,
        string authToken
    )
        : base(client, null)
    {
        this._embeddedWallet = embeddedWallet;
        this._httpClient = httpClient;
        this._email = email;
        this._phoneNumber = phoneNumber;
        this._authProvider = authProvider;
        this._siweSigner = siweSigner;
        this._authToken = authToken;
    }

    #region Creation

    public static async Task<EcosystemWallet> Create(
        ThirdwebClient client,
        string ecosystemId,
        string ecosystemPartnerId = null,
        string email = null,
        string phoneNumber = null,
        AuthProvider authProvider = AuthProvider.Default,
        string storageDirectoryPath = null,
        IThirdwebWallet siweSigner = null
    )
    {
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client), "Client cannot be null.");
        }

        if (string.IsNullOrEmpty(ecosystemId))
        {
            throw new ArgumentNullException(nameof(ecosystemId), "Ecosystem ID cannot be null or empty.");
        }

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
            AuthProvider.Siwe => "Siwe",
            AuthProvider.Default => string.IsNullOrEmpty(email) ? "Phone" : "Email",
            _ => throw new ArgumentException("Invalid AuthProvider"),
        };

        var enclaveHttpClient = client.HttpClient.GetType().GetConstructor(Type.EmptyTypes).Invoke(null) as IThirdwebHttpClient;
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
        enclaveHttpClient.SetHeaders(headers);

        storageDirectoryPath ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Thirdweb", "EcosystemWallet");
        var embeddedWallet = new EmbeddedWallet(client, storageDirectoryPath, ecosystemId, ecosystemPartnerId);

        string authToken = null;
        try
        {
            authToken = await embeddedWallet.ResumeEnclaveSession(email, phoneNumber, authproviderStr).ConfigureAwait(false);
            enclaveHttpClient.AddHeader("Authorization", $"Bearer embedded-wallet-token:{authToken}");

            return new EcosystemWallet(client, embeddedWallet, enclaveHttpClient, email, phoneNumber, authproviderStr, siweSigner, authToken)
            {
                _address = await GetAddressFromEnclave(enclaveHttpClient).ConfigureAwait(false)
            };
        }
        catch
        {
            return new EcosystemWallet(client, embeddedWallet, enclaveHttpClient, email, phoneNumber, authproviderStr, siweSigner, authToken);
        }
    }

    private static async Task<string> GetAddressFromEnclave(IThirdwebHttpClient httpClient)
    {
        var url = $"{EnclavePath}/details";
        var response = await httpClient.GetAsync(url).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var enclaveResponse = JsonConvert.DeserializeObject<List<EnclaveWalletResponse>>(content);
        return enclaveResponse[0].Wallet.Address.ToChecksumAddress();
    }

    private async Task<string> GenerateOrLoadWallet()
    {
        try
        {
            var url = $"{EnclavePath}/generate";
            var requestContent = new StringContent("", Encoding.UTF8, "application/json");
            this._httpClient.AddHeader("Authorization", $"Bearer embedded-wallet-token:{this._authToken}");
            var response = await this._httpClient.PostAsync(url, requestContent).ConfigureAwait(false);
            _ = response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var enclaveResponse = JsonConvert.DeserializeObject<EnclaveWalletResponse>(content);
            this._address = enclaveResponse.Wallet.Address.ToChecksumAddress();
        }
        catch
        {
            this._address = await GetAddressFromEnclave(this._httpClient).ConfigureAwait(false);
        }

        await this._embeddedWallet.CreateEnclaveSession(this._authToken, this._email, this._phoneNumber, this._authProvider).ConfigureAwait(false);
        return this._address;
    }

    #endregion

    #region Wallet Specific

    public string GetEmail()
    {
        return this._email;
    }

    public string GetPhoneNumber()
    {
        return this._phoneNumber;
    }

    #endregion

    #region Two Step Authentication

    public async Task<(bool isNewUser, bool isNewDevice)> SendOTP()
    {
        if (await this.IsConnected().ConfigureAwait(false))
        {
            throw new InvalidOperationException("User is already connected.");
        }

        if (string.IsNullOrEmpty(this._email) && string.IsNullOrEmpty(this._phoneNumber))
        {
            throw new Exception("Email or Phone Number is required for OTP login");
        }

        try
        {
            return this._email == null
                ? await this._embeddedWallet.SendPhoneOtpAsync(this._phoneNumber).ConfigureAwait(false)
                : await this._embeddedWallet.SendEmailOtpAsync(this._email).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            throw new Exception("Failed to send OTP", e);
        }
    }

    public async Task<string> LoginWithOtp(string otp)
    {
        if (await this.IsConnected().ConfigureAwait(false))
        {
            throw new InvalidOperationException("User is already connected.");
        }

        if (string.IsNullOrEmpty(otp))
        {
            throw new ArgumentNullException(nameof(otp), "OTP cannot be null or empty.");
        }

        var serverRes =
            string.IsNullOrEmpty(this._email) && string.IsNullOrEmpty(this._phoneNumber)
                ? throw new Exception("Email or Phone Number is required for OTP login")
                : this._email == null
                    ? await this._embeddedWallet.VerifyPhoneOtpAsync(this._phoneNumber, otp).ConfigureAwait(false)
                    : await this._embeddedWallet.VerifyEmailOtpAsync(this._email, otp).ConfigureAwait(false);

        this._authToken = serverRes.AuthToken;

        return await this.GenerateOrLoadWallet().ConfigureAwait(false);
    }

    #endregion

    #region Single Step Authentication

    public async Task<string> LoginWithOauth(
        bool isMobile,
        Action<string> browserOpenAction,
        string mobileRedirectScheme = "thirdweb://",
        IThirdwebBrowser browser = null,
        CancellationToken cancellationToken = default
    )
    {
        if (await this.IsConnected().ConfigureAwait(false))
        {
            throw new InvalidOperationException("User is already connected.");
        }

        if (isMobile && string.IsNullOrEmpty(mobileRedirectScheme))
        {
            throw new ArgumentNullException(nameof(mobileRedirectScheme), "Mobile redirect scheme cannot be null or empty on this platform.");
        }

        var platform = this._httpClient?.Headers?["x-sdk-name"] == "UnitySDK_WebGL" ? "web" : "dotnet";
        var redirectUrl = isMobile ? mobileRedirectScheme : "http://localhost:8789/";
        var loginUrl = await this._embeddedWallet.FetchHeadlessOauthLoginLinkAsync(this._authProvider, platform).ConfigureAwait(false);
        loginUrl = platform == "web" ? loginUrl : $"{loginUrl}?platform={platform}&redirectUrl={redirectUrl}&developerClientId={this.Client.ClientId}&authOption={this._authProvider}";

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
                throw new Exception($"Failed to login with {this._authProvider}: {browserResult.Status} | {browserResult.Error}");
        }
        var callbackUrl =
            browserResult.Status != BrowserStatus.Success
                ? throw new Exception($"Failed to login with {this._authProvider}: {browserResult.Status} | {browserResult.Error}")
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

        var serverRes = await this._embeddedWallet.SignInWithOauthAsync(authResultJson).ConfigureAwait(false);

        this._authToken = serverRes.AuthToken;

        var walletAddress = await this.GenerateOrLoadWallet().ConfigureAwait(false);

        return walletAddress;
    }

    public async Task<string> LoginWithSiwe(BigInteger chainId)
    {
        if (await this.IsConnected().ConfigureAwait(false))
        {
            throw new InvalidOperationException("User is already connected.");
        }

        if (this._siweSigner == null)
        {
            throw new ArgumentNullException(nameof(this._siweSigner), "SIWE Signer wallet cannot be null.");
        }

        if (!await this._siweSigner.IsConnected().ConfigureAwait(false))
        {
            throw new InvalidOperationException("SIWE Signer wallet must be connected as this operation requires it to sign a message.");
        }

        var serverRes =
            chainId <= 0
                ? throw new ArgumentException("Chain ID must be greater than 0.", nameof(chainId))
                : await this._embeddedWallet.SignInWithSiweAsync(this._siweSigner, chainId).ConfigureAwait(false);

        this._authToken = serverRes.AuthToken;

        return await this.GenerateOrLoadWallet().ConfigureAwait(false);
    }

    public async Task<string> LoginWithJWT(string jwt, string encryptionKey)
    {
        if (await this.IsConnected().ConfigureAwait(false))
        {
            throw new InvalidOperationException("User is already connected.");
        }

        if (string.IsNullOrEmpty(encryptionKey))
        {
            throw new ArgumentException("Encryption key cannot be null or empty.", nameof(encryptionKey));
        }

        var serverRes = string.IsNullOrEmpty(jwt) ? throw new ArgumentException("JWT cannot be null or empty.", nameof(jwt)) : await this._embeddedWallet.SignInWithJwtAsync(jwt).ConfigureAwait(false);
        this._authToken = serverRes.AuthToken;
        return await this.GenerateOrLoadWallet().ConfigureAwait(false);
    }

    public async Task<string> LoginWithAuthEndpoint(string payload, string encryptionKey)
    {
        if (await this.IsConnected().ConfigureAwait(false))
        {
            throw new InvalidOperationException("User is already connected.");
        }

        if (string.IsNullOrEmpty(encryptionKey))
        {
            throw new ArgumentException("Encryption key cannot be null or empty.", nameof(encryptionKey));
        }

        var serverRes = string.IsNullOrEmpty(payload)
            ? throw new ArgumentNullException(nameof(payload), "Payload cannot be null or empty.")
            : await this._embeddedWallet.SignInWithAuthEndpointAsync(payload).ConfigureAwait(false);

        this._authToken = serverRes.AuthToken;

        return await this.GenerateOrLoadWallet().ConfigureAwait(false);
    }

    #endregion

    #region IThirdwebWallet

    public override Task<string> GetAddress()
    {
        if (!string.IsNullOrEmpty(this._address))
        {
            return Task.FromResult(this._address.ToChecksumAddress());
        }
        else
        {
            return Task.FromResult(this._address);
        }
    }

    public override Task<string> EthSign(byte[] rawMessage)
    {
        if (rawMessage == null)
        {
            throw new ArgumentNullException(nameof(rawMessage), "Message to sign cannot be null.");
        }

        throw new NotImplementedException();
    }

    public override Task<string> EthSign(string message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message), "Message to sign cannot be null.");
        }

        throw new NotImplementedException();
    }

    public override async Task<string> PersonalSign(byte[] rawMessage)
    {
        if (rawMessage == null)
        {
            throw new ArgumentNullException(nameof(rawMessage), "Message to sign cannot be null.");
        }

        var url = $"{EnclavePath}/sign-message";
        var payload = new { messagePayload = new { message = rawMessage.BytesToHex(), isRaw = true } };

        var requestContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

        var response = await this._httpClient.PostAsync(url, requestContent).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var res = JsonConvert.DeserializeObject<EnclaveSignResponse>(content);
        return res.Signature;
    }

    public override async Task<string> PersonalSign(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            throw new ArgumentNullException(nameof(message), "Message to sign cannot be null.");
        }

        var url = $"{EnclavePath}/sign-message";
        var payload = new { messagePayload = new { message, isRaw = false } };

        var requestContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

        var response = await this._httpClient.PostAsync(url, requestContent).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var res = JsonConvert.DeserializeObject<EnclaveSignResponse>(content);
        return res.Signature;
    }

    public override async Task<string> SignTypedDataV4(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            throw new ArgumentNullException(nameof(json), "Json to sign cannot be null.");
        }

        var url = $"{EnclavePath}/sign-typed-data";

        var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await this._httpClient.PostAsync(url, requestContent).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var res = JsonConvert.DeserializeObject<EnclaveSignResponse>(content);
        return res.Signature;
    }

    public override async Task<string> SignTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data), "Data to sign cannot be null.");
        }

        var safeJson = Utils.ToJsonExternalWalletFriendly(typedData, data);
        return await this.SignTypedDataV4(safeJson).ConfigureAwait(false);
    }

    public override async Task<string> SignTransaction(ThirdwebTransactionInput transaction)
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

        var url = $"{EnclavePath}/sign-transaction";

        var requestContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

        var response = await this._httpClient.PostAsync(url, requestContent).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var res = JsonConvert.DeserializeObject<EnclaveSignResponse>(content);
        return res.Signature;
    }

    public override Task<bool> IsConnected()
    {
        return Task.FromResult(this._authToken != null);
    }

    public override Task<string> SendTransaction(ThirdwebTransactionInput transaction)
    {
        throw new InvalidOperationException("SendTransaction is not supported for Ecosystem Wallets, please use the unified Contract or ThirdwebTransaction APIs.");
    }

    public override Task<ThirdwebTransactionReceipt> ExecuteTransaction(ThirdwebTransactionInput transactionInput)
    {
        throw new InvalidOperationException("ExecuteTransaction is not supported for Ecosystem Wallets, please use the unified Contract or ThirdwebTransaction APIs.");
    }

    public override async Task Disconnect()
    {
        this._address = null;
        this._authToken = null;
        await this._embeddedWallet.SignOutAsync().ConfigureAwait(false);
    }

    #endregion
}
