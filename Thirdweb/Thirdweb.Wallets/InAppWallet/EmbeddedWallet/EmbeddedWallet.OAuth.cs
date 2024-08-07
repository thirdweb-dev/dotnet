using Newtonsoft.Json;

namespace Thirdweb.EWS
{
    internal partial class EmbeddedWallet
    {
        public async Task<VerifyResult> SignInWithOauthAsync(string authProvider, string authResult, string recoveryCode)
        {
            var result = await server.VerifyOAuthAsync(authResult).ConfigureAwait(false);
            return await PostAuthSetup(result, recoveryCode, null, authProvider).ConfigureAwait(false);
        }

        public async Task<string> FetchHeadlessOauthLoginLinkAsync(string authProvider, string platform)
        {
            return await server.FetchHeadlessOauthLoginLinkAsync(authProvider, platform).ConfigureAwait(false);
        }

        public async Task<bool> IsRecoveryCodeNeededAsync(string authResultStr)
        {
            var authResult = JsonConvert.DeserializeObject<Server.AuthResultType_OAuth>(authResultStr);
            Server.UserWallet userWallet = await server.FetchUserDetailsAsync(authResult.StoredToken.AuthDetails.Email, null).ConfigureAwait(false);
            return userWallet.RecoveryShareManagement == "USER_MANAGED" && !userWallet.IsNewUser && localStorage.Data?.DeviceShare == null;
        }
    }
}
