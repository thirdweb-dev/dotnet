using System;
using System.Threading.Tasks;

namespace Thirdweb.EWS
{
    internal partial class EmbeddedWallet
    {
        public async Task<(bool isNewUser, bool isNewDevice, bool needsPassword)> SendOtpPhoneAsync(string phoneNumber)
        {
            string sessionId = await server.SendKmsPhoneOtpAsync(phoneNumber);
            bool isKmsWallet = true;
            await localStorage.SaveSessionAsync(sessionId, isKmsWallet);
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
                Server.VerifyResult result = await server.VerifyKmsPhoneOtpAsync(phoneNumber, otp, localStorage.Session.Id);
                await localStorage.RemoveSessionAsync();
                return await PostAuthSetup(result, recoveryCode, null, "PhoneOTP");
            }
            catch (VerificationException ex)
            {
                return new VerifyResult(ex.CanRetry);
            }
        }
    }
}
