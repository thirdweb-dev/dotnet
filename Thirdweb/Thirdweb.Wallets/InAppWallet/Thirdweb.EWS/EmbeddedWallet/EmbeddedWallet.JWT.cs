namespace Thirdweb.EWS;

internal partial class EmbeddedWallet
{
    public async Task<Server.VerifyResult> SignInWithJwtAsync(string jwt)
    {
        return await this._server.VerifyJwtAsync(jwt).ConfigureAwait(false);
    }
}
