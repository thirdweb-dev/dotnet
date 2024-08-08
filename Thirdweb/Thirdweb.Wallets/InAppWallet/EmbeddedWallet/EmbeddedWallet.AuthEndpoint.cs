using System.Threading.Tasks;

namespace Thirdweb.EWS
{
    internal partial class EmbeddedWallet
    {
        public async Task<VerifyResult> SignInWithAuthEndpointAsync(string payload, string encryptionKey)
        {
            var result = await server.VerifyAuthEndpointAsync(payload).ConfigureAwait(false);
            return await PostAuthSetup(result, encryptionKey, "AuthEndpoint").ConfigureAwait(false);
        }
    }
}
