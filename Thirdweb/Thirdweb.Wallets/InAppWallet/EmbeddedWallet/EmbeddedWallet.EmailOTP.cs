using System;
using System.Threading.Tasks;

namespace Thirdweb.EWS
{
    internal partial class EmbeddedWallet
    {
        public async Task<(bool isNewUser, bool isNewDevice)> SendOtpEmailAsync(string emailAddress)
        {
            var userWallet = await server.FetchUserDetailsAsync(emailAddress, null).ConfigureAwait(false);
            var sessionId = "";
            sessionId = await server.SendEmailOtpAsync(emailAddress).ConfigureAwait(false);
            await localStorage.SaveSessionAsync(sessionId).ConfigureAwait(false);
            var isNewDevice = userWallet.IsNewUser || localStorage.Data?.WalletUserId != userWallet.WalletUserId;
            return (userWallet.IsNewUser, isNewDevice);
        }

        public async Task<VerifyResult> VerifyOtpAsync(string emailAddress, string otp)
        {
            if (localStorage.Session == null)
            {
                throw new InvalidOperationException($"Must first invoke {nameof(SendOtpEmailAsync)}", new NullReferenceException());
            }
            try
            {
                if (!await server.CheckIsEmailOtpValidAsync(emailAddress, otp).ConfigureAwait(false))
                {
                    throw new VerificationException("Invalid OTP", true);
                }
                var result = await server.VerifyEmailOtpAsync(emailAddress, otp, localStorage.Session.Id).ConfigureAwait(false);
                await localStorage.RemoveSessionAsync().ConfigureAwait(false);
                return await PostAuthSetup(result, null, "EmailOTP").ConfigureAwait(false);
            }
            catch (VerificationException ex)
            {
                return new VerifyResult(ex.CanRetry);
            }
        }
    }
}
