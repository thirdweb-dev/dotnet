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

        public async Task<bool> IsRecoveryCodeNeededAsync(string authResultStr)
        {
            var authResult = JsonConvert.DeserializeObject<Server.AuthResultType_OAuth>(authResultStr);
            var userWallet = await server.FetchUserDetailsAsync(authResult.StoredToken.AuthDetails.Email, null).ConfigureAwait(false);
            return userWallet.RecoveryShareManagement == "USER_MANAGED" && !userWallet.IsNewUser && localStorage.Data?.DeviceShare == null;
        }
    }
}
