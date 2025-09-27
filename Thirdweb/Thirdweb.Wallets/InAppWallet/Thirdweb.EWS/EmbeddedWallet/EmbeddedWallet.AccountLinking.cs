namespace Thirdweb.EWS;

internal partial class EmbeddedWallet
{
    public async Task<List<Server.LinkedAccount>> UnlinkAccountAsync(string currentAccountToken, LinkedAccount linkedAccount)
    {
        var serverLinkedAccount = new Server.LinkedAccount
        {
            Type = linkedAccount.Type,
            Details = new Server.LinkedAccount.LinkedAccountDetails
            {
                Email = linkedAccount.Details.Email,
                Address = linkedAccount.Details.Address,
                Phone = linkedAccount.Details.Phone,
                Id = linkedAccount.Details.Id,
            },
        };
        return await this._server.UnlinkAccountAsync(currentAccountToken, serverLinkedAccount).ConfigureAwait(false);
    }

    public async Task<List<Server.LinkedAccount>> LinkAccountAsync(string currentAccountToken, string authTokenToConnect)
    {
        return await this._server.LinkAccountAsync(currentAccountToken, authTokenToConnect).ConfigureAwait(false);
    }

    public async Task<List<Server.LinkedAccount>> GetLinkedAccountsAsync(string currentAccountToken)
    {
        return await this._server.GetLinkedAccountsAsync(currentAccountToken).ConfigureAwait(false);
    }
}
