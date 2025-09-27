namespace Thirdweb.EWS;

internal partial class EmbeddedWallet
{
    public Server.VerifyResult SignInWithOauthAsync(string authResult)
    {
        return this._server.VerifyOAuthAsync(authResult);
    }

    public async Task<string> FetchHeadlessOauthLoginLinkAsync(string authProvider, string platform)
    {
        return await this._server.FetchHeadlessOauthLoginLinkAsync(authProvider, platform).ConfigureAwait(false);
    }
}
