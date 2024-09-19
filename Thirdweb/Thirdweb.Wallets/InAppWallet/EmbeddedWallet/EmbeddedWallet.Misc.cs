using System.Security.Cryptography;
using Nethereum.Web3.Accounts;

namespace Thirdweb.EWS;

internal partial class EmbeddedWallet
{
    internal LocalStorage.DataStorage GetSessionData()
    {
        return this._localStorage.Data ?? null;
    }

    internal async void UpdateSessionData(LocalStorage.DataStorage data)
    {
        await this._localStorage.SaveDataAsync(data).ConfigureAwait(false);
    }

    internal async Task<VerifyResult> PostAuthSetup(Server.VerifyResult result, string twManagedRecoveryCodeOverride, string authProvider)
    {
        var mainRecoveryCode = (twManagedRecoveryCodeOverride ?? result.RecoveryCode) ?? throw new InvalidOperationException("Server failed to return recovery code.");

        (var account, var deviceShare) = result.IsNewUser
            ? await this.CreateAccountAsync(result.AuthToken, mainRecoveryCode).ConfigureAwait(false)
            : await this.RecoverAccountAsync(result.AuthToken, mainRecoveryCode).ConfigureAwait(false);
        var user = this.MakeUserAsync(result.Email, result.PhoneNumber, account, result.AuthToken, result.WalletUserId, deviceShare, authProvider, result.AuthIdentifier);
        return new VerifyResult(user, mainRecoveryCode);
    }

    public async Task SignOutAsync()
    {
        this._user = null;
        await this._localStorage.SaveDataAsync(new LocalStorage.DataStorage(null, null, null, null, null, null, null)).ConfigureAwait(false);
    }

    public async Task<User> GetUserAsync(string email, string phone, string authProvider)
    {
        email = email?.ToLower();

        if (this._user != null)
        {
            return this._user;
        }
        else if (this._localStorage.Data?.AuthToken == null)
        {
            throw new InvalidOperationException("User is not signed in");
        }

        var userWallet = await this._server.FetchUserDetailsAsync(null, this._localStorage.Data.AuthToken).ConfigureAwait(false);
        switch (userWallet.Status)
        {
            case "Logged Out":
                throw new InvalidOperationException("User is logged out");
            case "Logged In, Wallet Uninitialized":
                throw new InvalidOperationException("User is logged in but wallet is uninitialized");
            case "Logged In, Wallet Initialized":
                if (string.IsNullOrEmpty(this._localStorage.Data?.DeviceShare))
                {
                    throw new InvalidOperationException("User is logged in but wallet is uninitialized");
                }

                var authShare = await this._server.FetchAuthShareAsync(this._localStorage.Data.AuthToken).ConfigureAwait(false);
                var emailAddress = userWallet.StoredToken?.AuthDetails.Email;
                var phoneNumber = userWallet.StoredToken?.AuthDetails.PhoneNumber;

                if ((email != null && email != emailAddress) || (phone != null && phone != phoneNumber))
                {
                    throw new InvalidOperationException("User email or phone number do not match");
                }
                else if (email == null && this._localStorage.Data.AuthProvider != authProvider)
                {
                    throw new InvalidOperationException($"User auth provider does not match. Expected {this._localStorage.Data.AuthProvider}, got {authProvider}");
                }
                else if (authShare == null)
                {
                    throw new InvalidOperationException("Server failed to return auth share");
                }

                this._user = new User(MakeAccountFromShares(new[] { authShare, this._localStorage.Data.DeviceShare }), emailAddress, phoneNumber);
                return this._user;
            default:
                break;
        }
        throw new InvalidOperationException($"Unexpected user status '{userWallet.Status}'");
    }

    private User MakeUserAsync(string emailAddress, string phoneNumber, Account account, string authToken, string walletUserId, string deviceShare, string authProvider, string authIdentifier)
    {
        var data = new LocalStorage.DataStorage(authToken, deviceShare, emailAddress, phoneNumber, walletUserId, authProvider, authIdentifier);
        this.UpdateSessionData(data);
        this._user = new User(account, emailAddress, phoneNumber);
        return this._user;
    }

    private async Task<(Account account, string deviceShare)> CreateAccountAsync(string authToken, string recoveryCode)
    {
        var secret = Secrets.Random(KEY_SIZE);

        (var deviceShare, var recoveryShare, var authShare) = CreateShares(secret);
        var encryptedRecoveryShare = await this.EncryptShareAsync(recoveryShare, recoveryCode).ConfigureAwait(false);
        Account account = new(secret);
        await this._server.StoreAddressAndSharesAsync(account.Address, authShare, encryptedRecoveryShare, authToken).ConfigureAwait(false);
        return (account, deviceShare);
    }

    internal async Task<(Account account, string deviceShare)> RecoverAccountAsync(string authToken, string recoveryCode)
    {
        (var authShare, var encryptedRecoveryShare) = await this._server.FetchAuthAndRecoverySharesAsync(authToken).ConfigureAwait(false);

        var recoveryShare = await DecryptShareAsync(encryptedRecoveryShare, recoveryCode).ConfigureAwait(false);

        var account = MakeAccountFromShares(authShare, recoveryShare);
        Secrets secrets = new();
        var deviceShare = secrets.NewShare(DEVICE_SHARE_ID, new[] { authShare, recoveryShare });
        return (account, deviceShare);
    }

    internal async Task<(string address, string encryptedPrivateKeyB64, string ivB64, string kmsCiphertextB64)> GenerateEncryptionDataAsync(string authToken, string recoveryCode)
    {
        var (account, _) = await this.RecoverAccountAsync(authToken, recoveryCode).ConfigureAwait(false);
        var address = account.Address;

        var encryptedKeyResult = await this._server.GenerateEncryptedKeyResultAsync(authToken).ConfigureAwait(false);

        var plainTextBase64 = encryptedKeyResult["Plaintext"]?.ToString();
        var cipherTextBlobBase64 = encryptedKeyResult["CiphertextBlob"]?.ToString();

        if (string.IsNullOrEmpty(plainTextBase64) || string.IsNullOrEmpty(cipherTextBlobBase64))
        {
            throw new InvalidOperationException("No migration key found. Please try again.");
        }

        var iv = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(iv);
        }

        var privateKey = account.PrivateKey;
        var utf8WithoutBom = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        var privateKeyBytes = utf8WithoutBom.GetBytes(privateKey);

        byte[] encryptedPrivateKeyBytes;
        try
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Key = Convert.FromBase64String(plainTextBase64);
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            encryptedPrivateKeyBytes = encryptor.TransformFinalBlock(privateKeyBytes, 0, privateKeyBytes.Length);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Encryption failed.", ex);
        }

        var encryptedData = new byte[iv.Length + encryptedPrivateKeyBytes.Length];
        iv.CopyTo(encryptedData, 0);
        encryptedPrivateKeyBytes.CopyTo(encryptedData, iv.Length);

        var encryptedDataB64 = Convert.ToBase64String(encryptedData);
        var ivB64 = Convert.ToBase64String(iv);

        return (address, encryptedDataB64, ivB64, cipherTextBlobBase64);
    }

    public class VerifyResult
    {
        public User User { get; }
        public bool CanRetry { get; }
        public string MainRecoveryCode { get; }
        public bool? WasEmailed { get; }

        public VerifyResult(User user, string mainRecoveryCode)
        {
            this.User = user;
            this.MainRecoveryCode = mainRecoveryCode;
        }

        public VerifyResult(bool canRetry)
        {
            this.CanRetry = canRetry;
        }
    }
}
