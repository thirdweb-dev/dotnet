namespace Thirdweb.EWS
{
    internal partial class EmbeddedWallet
    {
        public async Task<List<Server.LinkedAccount>> LinkAccountAsync(string currentAccountToken, string authTokenToConnect)
        {
            return await server.LinkAccountAsync(currentAccountToken, authTokenToConnect).ConfigureAwait(false);
        }
    }
}
