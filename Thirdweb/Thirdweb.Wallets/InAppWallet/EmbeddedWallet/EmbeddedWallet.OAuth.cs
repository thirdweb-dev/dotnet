using Newtonsoft.Json;

namespace Thirdweb.EWS
{
    internal partial class EmbeddedWallet
    {
        public async Task<Server.VerifyResult> SignInWithOauthAsync(string authProvider, string authResult)
        {
            return await server.VerifyOAuthAsync(authResult).ConfigureAwait(false);
        }

        public async Task<string> FetchHeadlessOauthLoginLinkAsync(string authProvider, string platform)
        {
            return await server.FetchHeadlessOauthLoginLinkAsync(authProvider, platform).ConfigureAwait(false);
        }
    }
}
