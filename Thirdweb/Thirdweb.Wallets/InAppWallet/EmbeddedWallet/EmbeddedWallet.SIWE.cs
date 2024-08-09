using System.Numerics;

namespace Thirdweb.EWS
{
    internal partial class EmbeddedWallet
    {
        public async Task<VerifyResult> SignInWithSiweAsync(IThirdwebWallet signer, BigInteger chainId)
        {
            var address = await signer.GetAddress().ConfigureAwait(false);
            var payload = await server.FetchSiwePayloadAsync(address, chainId.ToString()).ConfigureAwait(false);
            var payloadMsg = Utils.GenerateSIWE(payload);
            var signature = await signer.PersonalSign(payloadMsg).ConfigureAwait(false);

            var result = await server.VerifySiweAsync(payload, signature).ConfigureAwait(false);
            return await PostAuthSetup(result, address, null).ConfigureAwait(false);
        }
    }
}
