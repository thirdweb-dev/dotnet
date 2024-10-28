namespace Thirdweb.EWS;

internal partial class EmbeddedWallet
{
    public async Task SendPhoneOtpAsync(string phoneNumber)
    {
        _ = await this._server.SendPhoneOtpAsync(phoneNumber).ConfigureAwait(false);
    }

    public async Task<Server.VerifyResult> VerifyPhoneOtpAsync(string phoneNumber, string otp)
    {
        return await this._server.VerifyPhoneOtpAsync(phoneNumber, otp).ConfigureAwait(false);
    }
}
