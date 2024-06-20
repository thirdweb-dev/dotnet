using System;
using System.Threading.Tasks;
using Nethereum.Web3.Accounts;

namespace Thirdweb.EWS
{
    internal partial class EmbeddedWallet
    {
        public async Task VerifyThirdwebClientIdAsync(string domain)
        {
            var error = await server.VerifyThirdwebClientIdAsync(domain).ConfigureAwait(false);
            if (error != "")
            {
                throw new InvalidOperationException($"Invalid thirdweb client id for domain {domain} | {error}");
            }
        }

        private async Task<VerifyResult> PostAuthSetup(Server.VerifyResult result, string userRecoveryCode, string twManagedRecoveryCodeOverride, string authProvider)
        {
            // Define necessary variables from the result.
            Account account;
            string walletUserId = result.WalletUserId;
            string authToken = result.AuthToken;
            string emailAddress = result.Email;
            string deviceShare = localStorage.Data?.DeviceShare;

            // Fetch user details from the server.
            Server.UserWallet userDetails = await server.FetchUserDetailsAsync(emailAddress, authToken).ConfigureAwait(false);
            bool isUserManaged = userDetails.RecoveryShareManagement == "USER_MANAGED";
            bool isNewUser = userDetails.IsNewUser;
            User user;

            // Initialize variables related to recovery codes and email status.
            string mainRecoveryCode = null;
            string[] backupRecoveryCodes = null;
            bool? wasEmailed = null;

            if (!isUserManaged)
            {
                mainRecoveryCode = twManagedRecoveryCodeOverride ?? result.RecoveryCode;
                if (mainRecoveryCode == null)
                    throw new InvalidOperationException("Server failed to return recovery code.");
                (account, deviceShare) = result.IsNewUser
                    ? await CreateAccountAsync(result.AuthToken, mainRecoveryCode).ConfigureAwait(false)
                    : await RecoverAccountAsync(result.AuthToken, mainRecoveryCode).ConfigureAwait(false);
                user = await MakeUserAsync(emailAddress, account, authToken, walletUserId, deviceShare, authProvider).ConfigureAwait(false);
                return new VerifyResult(user, mainRecoveryCode, backupRecoveryCodes, wasEmailed);
            }

            if (isNewUser)
            {
                // Create recovery code for user-managed accounts.
                mainRecoveryCode = MakeRecoveryCode();

                // Commented out section for future use: Generating multiple backup recovery codes.
                /*
                backupRecoveryCodes = new string[7];
                for (int i = 0; i < backupRecoveryCodes.Length; i++)
                    backupRecoveryCodes[i] = MakeRecoveryCode();
                */

                // Create a new account and handle the recovery codes.
                (account, deviceShare) = await CreateAccountAsync(authToken, mainRecoveryCode, backupRecoveryCodes).ConfigureAwait(false);

                // Attempt to send the recovery code via email and record the outcome.
                try
                {
                    if (emailAddress == null)
                        throw new ArgumentNullException(nameof(emailAddress));
                    await server.SendRecoveryCodeEmailAsync(authToken, mainRecoveryCode, emailAddress).ConfigureAwait(false);
                    wasEmailed = true;
                }
                catch
                {
                    wasEmailed = false;
                }
            }
            else
            {
                // Handling for existing users.
                if (userRecoveryCode == null)
                {
                    if (deviceShare == null)
                        throw new ArgumentNullException(nameof(userRecoveryCode));

                    // Fetch the auth share and create an account from shares.
                    string authShare = await server.FetchAuthShareAsync(authToken).ConfigureAwait(false);
                    account = MakeAccountFromShares(authShare, deviceShare);
                }
                else
                {
                    // Recover the account using the provided recovery code.
                    (account, deviceShare) = await RecoverAccountAsync(authToken, userRecoveryCode).ConfigureAwait(false);
                }
            }

            // Validate the device share returned from server operations.
            if (deviceShare == null)
            {
                throw new InvalidOperationException("Server failed to return account");
            }

            // Construct the user object and prepare the result.
            user = await MakeUserAsync(emailAddress, account, authToken, walletUserId, deviceShare, authProvider).ConfigureAwait(false);
            return new VerifyResult(user, mainRecoveryCode, backupRecoveryCodes, wasEmailed);
        }

        public async Task SignOutAsync()
        {
            user = null;
            await localStorage.RemoveAuthTokenAsync().ConfigureAwait(false);
        }

        public async Task<User> GetUserAsync(string email, string authProvider)
        {
            if (user != null)
            {
                return user;
            }
            else if (localStorage.Data?.AuthToken == null)
            {
                throw new InvalidOperationException("User is not signed in");
            }

            Server.UserWallet userWallet = await server.FetchUserDetailsAsync(null, localStorage.Data.AuthToken).ConfigureAwait(false);
            switch (userWallet.Status)
            {
                case "Logged Out":
                    await SignOutAsync().ConfigureAwait(false);
                    throw new InvalidOperationException("User is logged out");
                case "Logged In, Wallet Uninitialized":
                    await SignOutAsync().ConfigureAwait(false);
                    throw new InvalidOperationException("User is logged in but wallet is uninitialized");
                case "Logged In, Wallet Initialized":
                    if (string.IsNullOrEmpty(localStorage.Data?.DeviceShare))
                    {
                        await SignOutAsync().ConfigureAwait(false);
                        throw new InvalidOperationException("User is logged in but wallet is uninitialized");
                    }

                    string authShare = await server.FetchAuthShareAsync(localStorage.Data.AuthToken).ConfigureAwait(false);
                    string emailAddress = userWallet.StoredToken?.AuthDetails.Email;

                    if (email != null && email != emailAddress)
                    {
                        await SignOutAsync().ConfigureAwait(false);
                        throw new InvalidOperationException("User email does not match");
                    }
                    else if (email == null && localStorage.Data.AuthProvider != authProvider)
                    {
                        await SignOutAsync().ConfigureAwait(false);
                        throw new InvalidOperationException($"User auth provider does not match. Expected {localStorage.Data.AuthProvider}, got {authProvider}");
                    }
                    else if (authShare == null)
                    {
                        throw new InvalidOperationException("Server failed to return auth share");
                    }
                    user = new User(MakeAccountFromShares(new[] { authShare, localStorage.Data.DeviceShare }), emailAddress);
                    return user;
            }
            throw new InvalidOperationException($"Unexpected user status '{userWallet.Status}'");
        }

        private async Task<User> MakeUserAsync(string emailAddress, Account account, string authToken, string walletUserId, string deviceShare, string authProvider)
        {
            var data = new LocalStorage.DataStorage(authToken, deviceShare, emailAddress ?? "", walletUserId, authProvider);
            await localStorage.SaveDataAsync(data).ConfigureAwait(false);
            user = new User(account, emailAddress ?? "");
            return user;
        }

        private async Task<(Account account, string deviceShare)> CreateAccountAsync(string authToken, string recoveryCode, string[] backupRecoveryCodes = null)
        {
            string secret = Secrets.Random(KEY_SIZE);
            (string deviceShare, string recoveryShare, string authShare) = CreateShares(secret);
            string encryptedRecoveryShare = await EncryptShareAsync(recoveryShare, recoveryCode).ConfigureAwait(false);
            Account account = new(secret);

            string[] backupRecoveryShares = null;
            if (backupRecoveryCodes != null)
            {
                backupRecoveryShares = new string[backupRecoveryCodes.Length];
                for (int i = 0; i < backupRecoveryCodes.Length; i++)
                {
                    backupRecoveryShares[i] = await EncryptShareAsync(recoveryShare, backupRecoveryCodes[i]).ConfigureAwait(false);
                }
            }
            await server.StoreAddressAndSharesAsync(account.Address, authShare, encryptedRecoveryShare, authToken, backupRecoveryShares).ConfigureAwait(false);
            return (account, deviceShare);
        }

        private async Task<(Account account, string deviceShare)> RecoverAccountAsync(string authToken, string recoveryCode)
        {
            (string authShare, string encryptedRecoveryShare) = await server.FetchAuthAndRecoverySharesAsync(authToken).ConfigureAwait(false);
            // make below async
            string recoveryShare = await Task.Run(() => DecryptShare(encryptedRecoveryShare, recoveryCode)).ConfigureAwait(false);
            Account account = MakeAccountFromShares(authShare, recoveryShare);
            Secrets secrets = new();
            string deviceShare = secrets.NewShare(DEVICE_SHARE_ID, new[] { authShare, recoveryShare });
            return (account, deviceShare);
        }

        public class VerifyResult
        {
            public User User { get; }
            public bool CanRetry { get; }
            public string MainRecoveryCode { get; }
            public string[] BackupRecoveryCodes { get; }
            public bool? WasEmailed { get; }

            public VerifyResult(User user, string mainRecoveryCode, string[] backupRecoveryCodes, bool? wasEmailed)
            {
                User = user;
                MainRecoveryCode = mainRecoveryCode;
                BackupRecoveryCodes = backupRecoveryCodes;
                WasEmailed = wasEmailed;
            }

            public VerifyResult(bool canRetry)
            {
                CanRetry = canRetry;
            }
        }
    }
}
