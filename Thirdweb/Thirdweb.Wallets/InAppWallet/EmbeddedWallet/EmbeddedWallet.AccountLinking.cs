namespace Thirdweb.EWS;

internal partial class EmbeddedWallet
{
    public async Task<List<Server.LinkedAccount>> LinkAccountAsync(string currentAccountToken, string authTokenToConnect)
    {
        return await this._server.LinkAccountAsync(currentAccountToken, authTokenToConnect).ConfigureAwait(false);
    }

    public async Task<List<Server.LinkedAccount>> GetLinkedAccountsAsync(string currentAccountToken)
    {
        return await this._server.GetLinkedAccountsAsync(currentAccountToken).ConfigureAwait(false);
    }
}
