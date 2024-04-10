using Nethereum.Signer;
using Thirdweb.EWS;

namespace Thirdweb
{
    public class InAppWallet : PrivateKeyWallet
    {
        internal EmbeddedWallet _embeddedWallet;
        internal string _email;
        internal string _phoneNumber;

        internal InAppWallet(ThirdwebClient client, string email, string phoneNumber, EmbeddedWallet embeddedWallet, EthECKey ecKey)
            : base(client, ecKey)
        {
            _email = email;
            _phoneNumber = phoneNumber;
            _embeddedWallet = embeddedWallet;
        }

        public static async Task<InAppWallet> Create(ThirdwebClient client, string email = null, string phoneNumber = null)
        {
            if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(phoneNumber))
            {
                throw new ArgumentException("Email or Phone Number must be provided to login.");
            }

            var embeddedWallet = new EmbeddedWallet(client);
            EthECKey ecKey;
            try
            {
                var user = await embeddedWallet.GetUserAsync(email, email == null ? "PhoneOTP" : "EmailOTP");
                ecKey = new EthECKey(user.Account.PrivateKey);
            }
            catch
            {
                Console.WriteLine("User not found. Please call InAppWallet.SendOTP() to initialize the login process.");
                ecKey = null;
            }
            return new InAppWallet(client, email, phoneNumber, embeddedWallet, ecKey);
        }

        public override async Task Disconnect()
        {
            await base.Disconnect();
            await _embeddedWallet.SignOutAsync();
        }

        #region OTP Flow

        public async Task SendOTP()
        {
            if (string.IsNullOrEmpty(_email) && string.IsNullOrEmpty(_phoneNumber))
            {
                throw new Exception("Email or Phone Number is required for OTP login");
            }

            try
            {
                if (_email != null)
                {
                    (var isNewUser, var isNewDevice, var needsRecoveryCode) = await _embeddedWallet.SendOtpEmailAsync(_email);
                }
                else if (_phoneNumber != null)
                {
                    (var isNewUser, var isNewDevice, var needsRecoveryCode) = await _embeddedWallet.SendOtpPhoneAsync(_phoneNumber);
                }
                else
                {
                    throw new Exception("Email or Phone Number must be provided to login.");
                }

                Console.WriteLine("OTP sent to user. Please call InAppWallet.SubmitOTP to login.");
            }
            catch (Exception e)
            {
                throw new Exception("Failed to send OTP email", e);
            }
        }

        public async Task<(string, bool)> SubmitOTP(string otp)
        {
            if (string.IsNullOrEmpty(otp))
            {
                throw new ArgumentNullException(nameof(otp), "OTP cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(_email) && string.IsNullOrEmpty(_phoneNumber))
            {
                throw new Exception("Email or Phone Number is required for OTP login");
            }

            var res = _email == null ? await _embeddedWallet.VerifyPhoneOtpAsync(_phoneNumber, otp, null) : await _embeddedWallet.VerifyOtpAsync(_email, otp, null);
            if (res.User == null)
            {
                var canRetry = res.CanRetry;
                if (canRetry)
                {
                    Console.WriteLine("Invalid OTP. Please try again.");
                }
                else
                {
                    Console.WriteLine("Invalid OTP. Please request a new OTP.");
                }
                return (null, canRetry);
            }
            else
            {
                _ecKey = new EthECKey(res.User.Account.PrivateKey);
                return (await GetAddress(), false);
            }
        }

        public Task<string> GetEmail()
        {
            return Task.FromResult(_email);
        }

        public Task<string> GetPhoneNumber()
        {
            return Task.FromResult(_phoneNumber);
        }

        #endregion
    }
}
