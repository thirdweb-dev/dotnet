using System.Runtime.Serialization;

namespace Thirdweb.EWS;

internal partial class LocalStorage : LocalStorageBase
{
    [DataContract]
    internal class DataStorage
    {
        internal string AuthToken => this._authToken;
        internal string DeviceShare => this._deviceShare;
        internal string EmailAddress => this._emailAddress;
        internal string PhoneNumber => this._phoneNumber;
        internal string WalletUserId => this._walletUserId;
        internal string AuthProvider => this._authProvider;
        internal string AuthIdentifier => this._authIdentifier;

        [DataMember(Name = "authToken")]
        private string _authToken;

        [DataMember(Name = "deviceShare")]
        private string _deviceShare;

        [DataMember(Name = "emailAddress")]
        private string _emailAddress;

        [DataMember(Name = "phoneNumber")]
        private string _phoneNumber;

        [DataMember(Name = "walletUserId")]
        private string _walletUserId;

        [DataMember(Name = "authProvider")]
        private string _authProvider;

        [DataMember(Name = "authIdentifier")]
        private string _authIdentifier;

        internal DataStorage(string authToken, string deviceShare, string emailAddress, string phoneNumber, string walletUserId, string authProvider, string authIdentifier)
        {
            this._authToken = authToken;
            this._deviceShare = deviceShare;
            this._emailAddress = emailAddress;
            this._phoneNumber = phoneNumber;
            this._walletUserId = walletUserId;
            this._authProvider = authProvider;
            this._authIdentifier = authIdentifier;
        }
    }

    [DataContract]
    private class Storage
    {
        [DataMember]
        internal DataStorage Data { get; set; }
    }
}
