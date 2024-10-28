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

    public async Task SignOutAsync()
    {
        await this._localStorage.SaveDataAsync(new LocalStorage.DataStorage(null, null, null, null, null, null, null)).ConfigureAwait(false);
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
