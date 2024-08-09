namespace Thirdweb.EWS
{
    internal partial class EmbeddedWallet
    {
        public async Task<(bool isNewUser, bool isNewDevice)> SendPhoneOtpAsync(string phoneNumber)
        {
            var userWallet = await server.FetchUserDetailsAsync(phoneNumber, null).ConfigureAwait(false);
            _ = await server.SendPhoneOtpAsync(phoneNumber).ConfigureAwait(false);
            var isNewDevice = userWallet.IsNewUser || localStorage.Data?.WalletUserId != userWallet.WalletUserId;
            return (userWallet.IsNewUser, isNewDevice);
        }

        public async Task<VerifyResult> VerifyPhoneOtpAsync(string phoneNumber, string otp)
        {
            try
            {
                var result = await server.VerifyPhoneOtpAsync(phoneNumber, otp).ConfigureAwait(false);
                return await PostAuthSetup(result, null, "Phone").ConfigureAwait(false);
            }
            catch (VerificationException ex)
            {
                return new VerifyResult(ex.CanRetry);
            }
        }
    }
}
