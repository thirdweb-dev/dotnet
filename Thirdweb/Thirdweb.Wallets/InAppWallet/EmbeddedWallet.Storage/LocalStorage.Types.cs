using System.Runtime.Serialization;

namespace Thirdweb.EWS;

internal partial class LocalStorage : LocalStorageBase
{
    [DataContract]
    internal class DataStorage
    {
        internal string AuthToken => this.authToken;
        internal string DeviceShare => this.deviceShare;
        internal string EmailAddress => this.emailAddress;
        internal string PhoneNumber => this.phoneNumber;
        internal string WalletUserId => this.walletUserId;
        internal string AuthProvider => this.authProvider;

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

        internal void ClearAuthToken()
        {
            this.authToken = null;
        }
    }

    [DataContract]
    private class Storage
    {
        [DataMember]
        internal DataStorage Data { get; set; }
    }
}
