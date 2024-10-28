namespace Thirdweb.EWS;

internal partial class EmbeddedWallet
{
    public async Task SendEmailOtpAsync(string emailAddress)
    {
        emailAddress = emailAddress.ToLower();
        _ = await this._server.SendEmailOtpAsync(emailAddress).ConfigureAwait(false);
    }

    public async Task<Server.VerifyResult> VerifyEmailOtpAsync(string emailAddress, string otp)
    {
        emailAddress = emailAddress.ToLower();
        return await this._server.VerifyEmailOtpAsync(emailAddress, otp).ConfigureAwait(false);
    }
}
