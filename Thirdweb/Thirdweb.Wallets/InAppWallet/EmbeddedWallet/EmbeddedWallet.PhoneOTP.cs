using System;
using System.Threading.Tasks;

namespace Thirdweb.EWS
{
    internal partial class EmbeddedWallet
    {
        public async Task<(bool isNewUser, bool isNewDevice, bool needsPassword)> SendOtpPhoneAsync(string phoneNumber)
        {
            string sessionId = await server.SendKmsPhoneOtpAsync(phoneNumber).ConfigureAwait(false);
            bool isKmsWallet = true;
            await localStorage.SaveSessionAsync(sessionId, isKmsWallet).ConfigureAwait(false);
            bool isNewUser = true;
            bool isNewDevice = true;
            return (isNewUser, isNewDevice, !isKmsWallet);
        }

        public async Task<VerifyResult> VerifyPhoneOtpAsync(string phoneNumber, string otp, string recoveryCode)
        {
            if (localStorage.Session == null)
            {
                throw new InvalidOperationException($"Must first invoke {nameof(SendOtpPhoneAsync)}", new NullReferenceException());
            }
            try
            {
                // if (!await server.CheckIsPhoneKmsOtpValidAsync(phoneNumber, otp))
                // {
                //     throw new VerificationException("Invalid OTP", true);
                // }
                Server.VerifyResult result = await server.VerifyKmsPhoneOtpAsync(phoneNumber, otp, localStorage.Session.Id).ConfigureAwait(false);
                await localStorage.RemoveSessionAsync().ConfigureAwait(false);
                return await PostAuthSetup(result, recoveryCode, null, "PhoneOTP").ConfigureAwait(false);
            }
            catch (VerificationException ex)
            {
                return new VerifyResult(ex.CanRetry);
            }
        }
    }
}
