using System;
using System.Threading.Tasks;

namespace Thirdweb.EWS
{
    internal partial class EmbeddedWallet
    {
        public async Task<(bool isNewUser, bool isNewDevice, bool needsPassword)> SendOtpEmailAsync(string emailAddress)
        {
            Server.UserWallet userWallet = await server.FetchUserDetailsAsync(emailAddress, null);
            bool isKmsWallet = userWallet.RecoveryShareManagement != "USER_MANAGED";
            string sessionId = "";
            if (isKmsWallet)
            {
                sessionId = await server.SendKmsOtpEmailAsync(emailAddress);
            }
            else
            {
                await server.SendUserOtpEmailAsync(emailAddress);
            }
            await localStorage.SaveSessionAsync(sessionId, isKmsWallet);
            bool isNewDevice = userWallet.IsNewUser || localStorage.Data?.WalletUserId != userWallet.WalletUserId;
            return (userWallet.IsNewUser, isNewDevice, !isKmsWallet);
        }

        public async Task<VerifyResult> VerifyOtpAsync(string emailAddress, string otp, string recoveryCode)
        {
            if (localStorage.Session == null)
            {
                throw new InvalidOperationException($"Must first invoke {nameof(SendOtpEmailAsync)}", new NullReferenceException());
            }
            try
            {
                if (localStorage.Session.IsKmsWallet)
                {
                    if (!await server.CheckIsEmailKmsOtpValidAsync(emailAddress, otp))
                    {
                        throw new VerificationException("Invalid OTP", true);
                    }
                    Server.VerifyResult result = await server.VerifyKmsOtpAsync(emailAddress, otp, localStorage.Session.Id);
                    await localStorage.RemoveSessionAsync();
                    return await PostAuthSetup(result, recoveryCode, null, "EmailOTP");
                }
                else
                {
                    if (!await server.CheckIsEmailUserOtpValidAsync(emailAddress, otp))
                    {
                        throw new VerificationException("Invalid OTP", true);
                    }
                    Server.VerifyResult result = await server.VerifyUserOtpAsync(emailAddress, otp);
                    await localStorage.RemoveSessionAsync();
                    return await PostAuthSetup(result, recoveryCode, null, "EmailOTP");
                }
            }
            catch (VerificationException ex)
            {
                Console.WriteLine("VerifyOtpAsync Error: " + ex.Message);
                return new VerifyResult(ex.CanRetry);
            }
        }
    }
}
