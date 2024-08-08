namespace Thirdweb.EWS
{
    internal partial class EmbeddedWallet
    {
        public async Task<(bool isNewUser, bool isNewDevice)> SendOtpPhoneAsync(string phoneNumber)
        {
            var sessionId = await server.SendPhoneOtpAsync(phoneNumber).ConfigureAwait(false);
            await localStorage.SaveSessionAsync(sessionId).ConfigureAwait(false);
            var isNewUser = true;
            var isNewDevice = true;
            return (isNewUser, isNewDevice);
        }

        public async Task<VerifyResult> VerifyPhoneOtpAsync(string phoneNumber, string otp)
        {
            if (localStorage.Session == null)
            {
                throw new InvalidOperationException($"Must first invoke {nameof(SendOtpPhoneAsync)}", new NullReferenceException());
            }
            try
            {
                var result = await server.VerifyPhoneOtpAsync(phoneNumber, otp, localStorage.Session.Id).ConfigureAwait(false);
                await localStorage.RemoveSessionAsync().ConfigureAwait(false);
                return await PostAuthSetup(result, null, "PhoneOTP").ConfigureAwait(false);
            }
            catch (VerificationException ex)
            {
                return new VerifyResult(ex.CanRetry);
            }
        }
    }
}
