namespace Thirdweb.EWS;

internal partial class EmbeddedWallet
{
    public async Task<Server.VerifyResult> SignInWithBackendAsync(string walletSecret)
    {
        return await this._server.VerifyBackendAsync(walletSecret).ConfigureAwait(false);
    }
}
