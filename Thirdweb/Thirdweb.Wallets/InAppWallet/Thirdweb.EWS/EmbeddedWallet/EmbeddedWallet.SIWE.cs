using System.Numerics;

namespace Thirdweb.EWS;

internal partial class EmbeddedWallet
{
    public async Task<Server.VerifyResult> SignInWithSiweAsync(IThirdwebWallet signer, BigInteger chainId)
    {
        var address = await signer.GetAddress().ConfigureAwait(false);
        var payload = await this._server.FetchSiwePayloadAsync(address, chainId.ToString()).ConfigureAwait(false);
        var payloadMsg = Utils.GenerateSIWE(payload);
        var signature = await signer.PersonalSign(payloadMsg).ConfigureAwait(false);

        return await this._server.VerifySiweAsync(payload, signature).ConfigureAwait(false);
    }

    public async Task<Server.VerifyResult> SignInWithSiweExternalRawAsync(LoginPayloadData payload, string signature)
    {
        return await this._server.VerifySiweExternalAsync(payload, signature).ConfigureAwait(false);
    }
}
