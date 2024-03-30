using System.Threading.Tasks;

namespace Thirdweb.EWS
{
    internal partial class EmbeddedWallet
    {
        public async Task<VerifyResult> SignInWithAuthEndpointAsync(string payload, string encryptionKey, string recoveryCode)
        {
            Server.VerifyResult result = await server.VerifyAuthEndpointAsync(payload);
            return await PostAuthSetup(result, recoveryCode, encryptionKey, "AuthEndpoint");
        }
    }
}
