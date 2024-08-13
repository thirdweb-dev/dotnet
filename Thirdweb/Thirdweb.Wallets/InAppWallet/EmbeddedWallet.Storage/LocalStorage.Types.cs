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
            internal string PhoneNumber => phoneNumber;
            internal string WalletUserId => walletUserId;
            internal string AuthProvider => authProvider;

            [DataMember]
            private string authToken;

            [DataMember]
            private string deviceShare;

            [DataMember]
            private string emailAddress;

            [DataMember]
            private string phoneNumber;

            [DataMember]
            private string walletUserId;

            [DataMember]
            private string authProvider;

            internal DataStorage(string authToken, string deviceShare, string emailAddress, string phoneNumber, string walletUserId, string authProvider)
            {
                this.authToken = authToken;
                this.deviceShare = deviceShare;
                this.emailAddress = emailAddress;
                this.phoneNumber = phoneNumber;
                this.walletUserId = walletUserId;
                this.authProvider = authProvider;
            }

            internal void ClearAuthToken() => authToken = null;
        }

        [DataContract]
        private class Storage
        {
            [DataMember]
            internal DataStorage Data { get; set; }
        }
    }
}
