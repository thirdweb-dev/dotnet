using System.Linq;
using System.Runtime.Serialization;

namespace Thirdweb.EWS
{
    internal partial class Server
    {
        internal class VerifyResult
        {
            internal VerifyResult(bool isNewUser, string authToken, string walletUserId, string recoveryCode, string email)
            {
                IsNewUser = isNewUser;
                AuthToken = authToken;
                WalletUserId = walletUserId;
                RecoveryCode = recoveryCode;
                Email = email;
            }

            internal bool IsNewUser { get; }
            internal string AuthToken { get; }
            internal string WalletUserId { get; }
            internal string RecoveryCode { get; }
            internal string Email { get; }
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
                internal UserAuthDetails AuthDetails { get; set; }

                [DataMember]
                private string authProvider;

                [DataMember]
                private string developerClientId;

                [DataMember(Name = "isNewUser")]
                internal bool IsNewUser { get; set; }

                [DataMember]
                private string rawToken;

                [DataMember]
                private string userId;
            }
        }

        [DataContract]
        private class GetUserStatusApiReturnType
        {
            [DataMember]
#pragma warning disable CS0649 // Deserialization will populate this field.
            private string status;
#pragma warning restore CS0649 // Field 'Server.GetUserStatusApiReturnType.status' is never assigned to, and will always have its default value null
            internal UserStatus Status => (UserStatus)status.Length;

            [DataMember]
            private StoredTokenType storedToken;

            [DataMember(Name = "user")]
            internal UserType User { get; set; }

            [DataContract]
            internal class UserType
            {
                [DataMember(Name = "authDetails")]
                internal UserAuthDetails AuthDetails { get; set; }

                [DataMember]
                private string walletAddress;
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
        private class IsEmailUserOtpValidResponse
        {
            [DataMember(Name = "isValid")]
            internal bool IsValid { get; set; }
        }

        [DataContract]
        private class IsEmailKmsOtpValidResponse
        {
            [DataMember(Name = "isOtpValid")]
            internal bool IsOtpValid { get; set; }
        }

        [DataContract]
        private class HeadlessOauthLoginLinkResponse
        {
            [DataMember(Name = "googleLoginLink")]
            internal string GoogleLoginLink { get; set; }

            [DataMember(Name = "platformLoginLink")]
            internal string PlatformLoginLink { get; set; }

            [DataMember(Name = "oauthLoginLink")]
            internal string OauthLoginLink { get; set; }
        }

        [DataContract]
        internal class StoredTokenType
        {
            [DataMember]
            private string jwtToken;

            [DataMember]
            private string authProvider;

            [DataMember(Name = "authDetails")]
            internal UserAuthDetails AuthDetails { get; set; }

            [DataMember]
            private string developerClientId;

            [DataMember]
            private string cookieString;

            [DataMember]
            private bool isNewUser;
        }

        [DataContract]
        internal class UserAuthDetails
        {
            [DataMember(Name = "email")]
            internal string Email { get; set; }

            [DataMember(Name = "userWalletId")]
            internal string WalletUserId { get; set; }

            [DataMember(Name = "recoveryShareManagement")]
            internal string RecoveryShareManagement { get; set; }

            [DataMember(Name = "recoveryCode")]
            internal string RecoveryCode { get; set; }

            [DataMember(Name = "backupRecoveryCodes")]
            internal string[] BackupRecoveryCodes { get; set; }
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
        private class IdTokenResponse
        {
            [DataMember(Name = "accessToken")]
            internal string AccessToken { get; set; }

            [DataMember(Name = "idToken")]
            internal string IdToken { get; set; }
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
        internal class RecoveryShareManagementResponse
        {
            internal string Value => data.oauth.FirstOrDefault()?.recovery_share_management;
#pragma warning disable CS0649 // Deserialization will populate these fields.
            [DataMember]
            private RecoveryShareManagementResponse data;

            [DataMember]
            private RecoveryShareManagementResponse[] oauth;

            [DataMember]
            private string recovery_share_management;
#pragma warning restore CS0649 // Field 'Server.RecoveryShareManagementResponse.*' is never assigned to, and will always have its default value null
        }

        [DataContract]
        internal class AuthResultType_OAuth
        {
            [DataMember(Name = "storedToken")]
            internal StoredTokenType_OAuth StoredToken { get; set; }

            [DataMember(Name = "walletDetails")]
            internal WalletDetailsType_OAuth WalletDetails { get; set; }
        }

        [DataContract]
        internal class StoredTokenType_OAuth
        {
            [DataMember(Name = "jwtToken")]
            internal string JwtToken { get; set; }

            [DataMember(Name = "authProvider")]
            internal string AuthProvider { get; set; }

            [DataMember(Name = "authDetails")]
            internal AuthDetailsType_OAuth AuthDetails { get; set; }

            [DataMember(Name = "developerClientId")]
            internal string DeveloperClientId { get; set; }

            [DataMember(Name = "cookieString")]
            internal string CookieString { get; set; }

            [DataMember(Name = "shouldStoreCookieString")]
            internal bool ShouldStoreCookieString { get; set; }

            [DataMember(Name = "isNewUser")]
            internal bool IsNewUser { get; set; }

            [DataContract]
            internal class AuthDetailsType_OAuth
            {
                [DataMember(Name = "email")]
                internal string Email { get; set; }

                [DataMember(Name = "userWalletId")]
                internal string UserWalletId { get; set; }

                [DataMember(Name = "recoveryCode")]
                internal string RecoveryCode { get; set; }
            }
        }

        [DataContract]
        internal class WalletDetailsType_OAuth
        {
            [DataMember(Name = "deviceShareStored")]
            internal string DeviceShareStored { get; set; }

            [DataMember(Name = "isIframeStorageEnabled")]
            internal bool IsIframeStorageEnabled { get; set; }

            [DataMember(Name = "walletAddress")]
            internal string WalletAddress { get; set; }
        }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS0169 // The field 'Server.*' is never used
#pragma warning restore IDE0051 // The field 'Server.*' is unused
    }
}
