using System.Runtime.Serialization;

namespace Thirdweb.EWS
{
    internal partial class LocalStorage : LocalStorageBase
    {
        [DataContract]
        internal class DataStorage
        {
            internal string AuthToken => authToken;
            internal string DeviceShare => deviceShare;
            internal string EmailAddress => emailAddress;
            internal string WalletUserId => walletUserId;
            internal string AuthProvider => authProvider;

            [DataMember]
            private string authToken;

            [DataMember]
            private string deviceShare;

            [DataMember]
            private string emailAddress;

            [DataMember]
            private string walletUserId;

            [DataMember]
            private string authProvider;

            internal DataStorage(string authToken, string deviceShare, string emailAddress, string walletUserId, string authProvider)
            {
                this.authToken = authToken;
                this.deviceShare = deviceShare;
                this.emailAddress = emailAddress;
                this.walletUserId = walletUserId;
                this.authProvider = authProvider;
            }

            internal void ClearAuthToken() => authToken = null;
        }

        [DataContract]
        internal class SessionStorage
        {
            internal string Id => id;
            internal bool IsKmsWallet => isKmsWallet;

            [DataMember]
            private string id;

            [DataMember]
            private bool isKmsWallet;

            internal SessionStorage(string id, bool isKmsWallet)
            {
                this.id = id;
                this.isKmsWallet = isKmsWallet;
            }
        }

        [DataContract]
        private class Storage
        {
            [DataMember]
            internal DataStorage Data { get; set; }

            [DataMember]
            internal SessionStorage Session { get; set; }
        }
    }
}
