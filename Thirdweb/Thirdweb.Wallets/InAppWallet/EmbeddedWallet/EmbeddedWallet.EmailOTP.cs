namespace Thirdweb.EWS;

internal partial class EmbeddedWallet
{
    public async Task<(bool isNewUser, bool isNewDevice)> SendEmailOtpAsync(string emailAddress)
    {
        emailAddress = emailAddress.ToLower();
        var userWallet = await this._server.FetchUserDetailsAsync(emailAddress, null).ConfigureAwait(false);
        _ = await this._server.SendEmailOtpAsync(emailAddress).ConfigureAwait(false);
        var isNewDevice = userWallet.IsNewUser || this._localStorage.Data?.WalletUserId != userWallet.WalletUserId;
        return (userWallet.IsNewUser, isNewDevice);
    }

    public async Task<Server.VerifyResult> VerifyEmailOtpAsync(string emailAddress, string otp)
    {
        emailAddress = emailAddress.ToLower();
        return await this._server.VerifyEmailOtpAsync(emailAddress, otp).ConfigureAwait(false);
    }
}
