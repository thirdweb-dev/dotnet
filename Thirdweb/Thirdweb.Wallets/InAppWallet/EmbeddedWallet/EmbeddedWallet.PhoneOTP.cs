namespace Thirdweb.EWS;

internal partial class EmbeddedWallet
{
    public async Task<(bool isNewUser, bool isNewDevice)> SendPhoneOtpAsync(string phoneNumber)
    {
        var userWallet = await this._server.FetchUserDetailsAsync(phoneNumber, null).ConfigureAwait(false);
        _ = await this._server.SendPhoneOtpAsync(phoneNumber).ConfigureAwait(false);
        var isNewDevice = userWallet.IsNewUser || this._localStorage.Data?.WalletUserId != userWallet.WalletUserId;
        return (userWallet.IsNewUser, isNewDevice);
    }

    public async Task<Server.VerifyResult> VerifyPhoneOtpAsync(string phoneNumber, string otp)
    {
        return await this._server.VerifyPhoneOtpAsync(phoneNumber, otp).ConfigureAwait(false);
    }
}
