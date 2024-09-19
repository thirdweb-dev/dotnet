using System.Runtime.Serialization;

namespace Thirdweb.EWS;

internal partial class Server
{
    internal class VerifyResult
    {
        internal VerifyResult(string authProvider, bool isNewUser, string authToken, string walletUserId, string recoveryCode, string email, string phoneNumber, string authIdentifier)
        {
            this.AuthProvider = authProvider;
            this.IsNewUser = isNewUser;
            this.AuthToken = authToken;
            this.WalletUserId = walletUserId;
            this.RecoveryCode = recoveryCode;
            this.Email = email;
            this.PhoneNumber = phoneNumber;
            this.AuthIdentifier = authIdentifier;
        }

        internal string AuthProvider { get; }
        internal bool IsNewUser { get; }
        internal string AuthToken { get; }
        internal string WalletUserId { get; }
        internal string RecoveryCode { get; }
        internal string Email { get; }
        internal string PhoneNumber { get; }
        internal string AuthIdentifier { get; }
    }

    [DataContract]
    internal class AccountConnectResponse
    {
        [DataMember(Name = "linkedAccounts", IsRequired = true)]
        public List<LinkedAccount> LinkedAccounts { get; set; }
    }

    [DataContract]
    public class LinkedAccount
    {
        [DataMember(Name = "type", IsRequired = true)]
        public string Type { get; set; }

        [DataMember(Name = "details", IsRequired = true)]
        public LinkedAccountDetails Details { get; set; }

        [DataContract]
        public class LinkedAccountDetails
        {
            [DataMember(Name = "email", EmitDefaultValue = false)]
            public string Email { get; set; }

            [DataMember(Name = "address", EmitDefaultValue = false)]
            public string Address { get; set; }

            [DataMember(Name = "phone", EmitDefaultValue = false)]
            public string Phone { get; set; }

            [DataMember(Name = "id", EmitDefaultValue = false)]
            public string Id { get; set; }
        }
    }

    [DataContract]
    private class SendEmailOtpReturnType
    {
        [DataMember(Name = "email")]
        internal string Email { get; set; }
    }

    [DataContract]
    private class SendPhoneOtpReturnType
    {
        [DataMember(Name = "phone")]
        internal string Phone { get; set; }
    }

#pragma warning disable CS0169, CS8618, IDE0051 // Deserialization will construct the following classes.
    [DataContract]
    private class AuthVerifiedTokenReturnType
    {
        [DataMember(Name = "verifiedToken")]
        internal VerifiedTokenType VerifiedToken { get; set; }

        [DataMember(Name = "verifiedTokenJwtString")]
        internal string VerifiedTokenJwtString { get; set; }

        [DataContract]
        internal class VerifiedTokenType
        {
            [DataMember(Name = "authDetails")]
            internal AuthDetailsType AuthDetails { get; set; }

            [DataMember(Name = "authProvider")]
            internal string AuthProvider { get; set; }

            [DataMember(Name = "developerClientId")]
            private string _developerClientId;

            [DataMember(Name = "isNewUser")]
            internal bool IsNewUser { get; set; }

            [DataMember(Name = "rawToken")]
            private string _rawToken;

            [DataMember(Name = "userId")]
            private string _userId;
        }
    }

    [DataContract]
    private class HttpErrorWithMessage
    {
        [DataMember(Name = "error")]
        internal string Error { get; set; } = "";

        [DataMember(Name = "message")]
        internal string Message { get; set; } = "";
    }

    [DataContract]
    private class SharesGetResponse
    {
        [DataMember(Name = "authShare")]
        internal string AuthShare { get; set; }

        [DataMember(Name = "maybeEncryptedRecoveryShares")]
        internal string[] MaybeEncryptedRecoveryShares { get; set; }
    }

    [DataContract]
    internal class UserWallet
    {
        [DataMember(Name = "status")]
        internal string Status { get; set; }

        [DataMember(Name = "isNewUser")]
        internal bool IsNewUser { get; set; }

        [DataMember(Name = "walletUserId")]
        internal string WalletUserId { get; set; }

        [DataMember(Name = "recoveryShareManagement")]
        internal string RecoveryShareManagement { get; set; }

        [DataMember(Name = "storedToken")]
        internal StoredTokenType StoredToken { get; set; }
    }

    [DataContract]
    internal class IdTokenResponse
    {
        [DataMember(Name = "token")]
        internal string Token { get; set; }

        [DataMember(Name = "identityId")]
        internal string IdentityId { get; set; }

        [DataMember(Name = "lambdaToken")]
        internal string LambdaToken { get; set; }
    }

    [DataContract]
    private class RecoverySharePasswordResponse
    {
        [DataMember(Name = "body")]
        internal string Body { get; set; }

        [DataMember(Name = "recoveryShareEncKey")]
        internal string RecoverySharePassword { get; set; }
    }

    [DataContract]
    internal class AuthResultType
    {
        [DataMember(Name = "storedToken")]
        internal StoredTokenType StoredToken { get; set; }
    }

    [DataContract]
    internal class StoredTokenType
    {
        [DataMember(Name = "jwtToken")]
        internal string JwtToken { get; set; }

        [DataMember(Name = "authProvider")]
        internal string AuthProvider { get; set; }

        [DataMember(Name = "authDetails")]
        internal AuthDetailsType AuthDetails { get; set; }

        [DataMember(Name = "developerClientId")]
        internal string DeveloperClientId { get; set; }

        [DataMember(Name = "cookieString")]
        internal string CookieString { get; set; }

        [DataMember(Name = "shouldStoreCookieString")]
        internal bool ShouldStoreCookieString { get; set; }

        [DataMember(Name = "isNewUser")]
        internal bool IsNewUser { get; set; }
    }

    [DataContract]
    internal class AuthDetailsType
    {
        [DataMember(Name = "phoneNumber")]
        internal string PhoneNumber { get; set; }

        [DataMember(Name = "email")]
        internal string Email { get; set; }

        [DataMember(Name = "userWalletId")]
        internal string UserWalletId { get; set; }

        [DataMember(Name = "authIdentifier")]
        internal string AuthIdentifier { get; set; }

        [DataMember(Name = "recoveryCode")]
        internal string RecoveryCode { get; set; }

        [DataMember(Name = "recoveryShareManagement")]
        internal string RecoveryShareManagement { get; set; }
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS0169 // The field 'Server.*' is never used
#pragma warning restore IDE0051 // The field 'Server.*' is unused
}
