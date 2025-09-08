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
}
