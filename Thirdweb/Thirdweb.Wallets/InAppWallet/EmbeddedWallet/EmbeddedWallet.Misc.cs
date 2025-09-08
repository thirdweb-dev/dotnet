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
