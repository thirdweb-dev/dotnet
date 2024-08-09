using Newtonsoft.Json;

namespace Thirdweb.EWS
{
    internal partial class EmbeddedWallet
    {
        public async Task<VerifyResult> SignInWithOauthAsync(string authProvider, string authResult)
        {
            var result = await server.VerifyOAuthAsync(authResult).ConfigureAwait(false);
            return await PostAuthSetup(result, null, authProvider).ConfigureAwait(false);
        }

        public async Task<string> FetchHeadlessOauthLoginLinkAsync(string authProvider, string platform)
        {
            return await server.FetchHeadlessOauthLoginLinkAsync(authProvider, platform).ConfigureAwait(false);
        }
    }
}
