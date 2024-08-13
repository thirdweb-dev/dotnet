namespace Thirdweb.EWS
{
    internal partial class EmbeddedWallet
    {
        public async Task<(bool isNewUser, bool isNewDevice)> SendEmailOtpAsync(string emailAddress)
        {
            emailAddress = emailAddress.ToLower();
            var userWallet = await server.FetchUserDetailsAsync(emailAddress, null).ConfigureAwait(false);
            _ = await server.SendEmailOtpAsync(emailAddress).ConfigureAwait(false);
            var isNewDevice = userWallet.IsNewUser || localStorage.Data?.WalletUserId != userWallet.WalletUserId;
            return (userWallet.IsNewUser, isNewDevice);
        }

        public async Task<Server.VerifyResult> VerifyEmailOtpAsync(string emailAddress, string otp)
        {
            emailAddress = emailAddress.ToLower();
            return await server.VerifyEmailOtpAsync(emailAddress, otp).ConfigureAwait(false);
        }
    }
}
