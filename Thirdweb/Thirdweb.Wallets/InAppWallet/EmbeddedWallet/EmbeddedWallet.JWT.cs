namespace Thirdweb.EWS
{
    internal partial class EmbeddedWallet
    {
        public async Task<VerifyResult> SignInWithJwtAsync(string jwt, string encryptionKey)
        {
            var result = await server.VerifyJwtAsync(jwt).ConfigureAwait(false);
            return await PostAuthSetup(result, encryptionKey, "JWT").ConfigureAwait(false);
        }
    }
}
