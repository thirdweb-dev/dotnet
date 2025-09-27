namespace Thirdweb.EWS;

internal partial class EmbeddedWallet
{
    public async Task<Server.VerifyResult> SignInWithGuestAsync(string sessionId)
    {
        return await this._server.VerifyGuestAsync(sessionId).ConfigureAwait(false);
    }
}
