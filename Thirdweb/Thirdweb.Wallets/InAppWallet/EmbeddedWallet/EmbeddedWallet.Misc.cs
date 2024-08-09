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

        internal string GetCurrentAuthToken()
        {
            return localStorage.Data?.AuthToken;
        }

        internal async Task<VerifyResult> PostAuthSetup(Server.VerifyResult result, string twManagedRecoveryCodeOverride, string authProvider)
        {
            var walletUserId = result.WalletUserId;
            var authToken = result.AuthToken;
            var emailAddress = result.Email;
            var phoneNumber = result.PhoneNumber;

            var mainRecoveryCode = (twManagedRecoveryCodeOverride ?? result.RecoveryCode) ?? throw new InvalidOperationException("Server failed to return recovery code.");

            (var account, var deviceShare) = result.IsNewUser
                ? await CreateAccountAsync(result.AuthToken, mainRecoveryCode).ConfigureAwait(false)
                : await RecoverAccountAsync(result.AuthToken, mainRecoveryCode).ConfigureAwait(false);
            var user = await MakeUserAsync(emailAddress, phoneNumber, account, authToken, walletUserId, deviceShare, authProvider).ConfigureAwait(false);
            return new VerifyResult(user, mainRecoveryCode);
        }

        public async Task SignOutAsync()
        {
            user = null;
            await localStorage.RemoveAuthTokenAsync().ConfigureAwait(false);
        }

        public async Task<User> GetUserAsync(string email, string phone, string authProvider)
        {
            email = email?.ToLower();

            if (user != null)
            {
                return user;
            }
            else if (localStorage.Data?.AuthToken == null)
            {
                throw new InvalidOperationException("User is not signed in");
            }

            var userWallet = await server.FetchUserDetailsAsync(null, localStorage.Data.AuthToken).ConfigureAwait(false);
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

                    var authShare = await server.FetchAuthShareAsync(localStorage.Data.AuthToken).ConfigureAwait(false);
                    var emailAddress = userWallet.StoredToken?.AuthDetails.Email;
                    var phoneNumber = userWallet.StoredToken?.AuthDetails.PhoneNumber;

                    if ((email != null && email != emailAddress) || (phone != null && phone != phoneNumber))
                    {
                        await SignOutAsync().ConfigureAwait(false);
                        throw new InvalidOperationException("User email or phone number do not match");
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

                    user = new User(MakeAccountFromShares(new[] { authShare, localStorage.Data.DeviceShare }), emailAddress, phoneNumber);
                    return user;
                default:
                    break;
            }
            throw new InvalidOperationException($"Unexpected user status '{userWallet.Status}'");
        }

        private async Task<User> MakeUserAsync(string emailAddress, string phoneNumber, Account account, string authToken, string walletUserId, string deviceShare, string authProvider)
        {
            var data = new LocalStorage.DataStorage(authToken, deviceShare, emailAddress, phoneNumber, walletUserId, authProvider);
            await localStorage.SaveDataAsync(data).ConfigureAwait(false);
            user = new User(account, emailAddress, phoneNumber);
            return user;
        }

        private async Task<(Account account, string deviceShare)> CreateAccountAsync(string authToken, string recoveryCode)
        {
            var secret = Secrets.Random(KEY_SIZE);
            (var deviceShare, var recoveryShare, var authShare) = CreateShares(secret);
            var encryptedRecoveryShare = await EncryptShareAsync(recoveryShare, recoveryCode).ConfigureAwait(false);
            Account account = new(secret);
            await server.StoreAddressAndSharesAsync(account.Address, authShare, encryptedRecoveryShare, authToken).ConfigureAwait(false);
            return (account, deviceShare);
        }

        private async Task<(Account account, string deviceShare)> RecoverAccountAsync(string authToken, string recoveryCode)
        {
            (var authShare, var encryptedRecoveryShare) = await server.FetchAuthAndRecoverySharesAsync(authToken).ConfigureAwait(false);
            var recoveryShare = await DecryptShareAsync(encryptedRecoveryShare, recoveryCode).ConfigureAwait(false);
            var account = MakeAccountFromShares(authShare, recoveryShare);
            Secrets secrets = new();
            var deviceShare = secrets.NewShare(DEVICE_SHARE_ID, new[] { authShare, recoveryShare });
            return (account, deviceShare);
        }

        public class VerifyResult
        {
            public User User { get; }
            public bool CanRetry { get; }
            public string MainRecoveryCode { get; }
            public bool? WasEmailed { get; }

            public VerifyResult(User user, string mainRecoveryCode)
            {
                User = user;
                MainRecoveryCode = mainRecoveryCode;
            }

            public VerifyResult(bool canRetry)
            {
                CanRetry = canRetry;
            }
        }
    }
}
