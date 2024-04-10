using System.Threading.Tasks;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;

namespace Thirdweb.EWS
{
    internal partial class EmbeddedWallet
    {
        public async Task<VerifyResult> SignInWithOauthAsync(string authProvider, string authResult, string recoveryCode)
        {
            Server.VerifyResult result = await server.VerifyOAuthAsync(authResult);
            return await PostAuthSetup(result, recoveryCode, null, authProvider);
        }

        public async Task<string> FetchHeadlessOauthLoginLinkAsync(string authProvider)
        {
            return await server.FetchHeadlessOauthLoginLinkAsync(authProvider);
        }

        public async Task<bool> IsRecoveryCodeNeededAsync(string authResultStr)
        {
            var authResult = JsonConvert.DeserializeObject<Server.AuthResultType_OAuth>(authResultStr);
            Server.UserWallet userWallet = await server.FetchUserDetailsAsync(authResult.StoredToken.AuthDetails.Email, null);
            return userWallet.RecoveryShareManagement == "USER_MANAGED" && !userWallet.IsNewUser && localStorage.Data?.DeviceShare == null;
        }
    }
}
