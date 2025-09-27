using System.Runtime.Serialization;

namespace Thirdweb.EWS;

internal partial class LocalStorage : LocalStorageBase
{
    [DataContract]
    internal class DataStorage
    {
        [field: DataMember(Name = "authToken")]
        internal string AuthToken { get; }

        [field: DataMember(Name = "deviceShare")]
        internal string DeviceShare { get; }

        [field: DataMember(Name = "emailAddress")]
        internal string EmailAddress { get; }

        [field: DataMember(Name = "phoneNumber")]
        internal string PhoneNumber { get; }

        [field: DataMember(Name = "walletUserId")]
        internal string WalletUserId { get; }

        [field: DataMember(Name = "authProvider")]
        internal string AuthProvider { get; }

        [field: DataMember(Name = "authIdentifier")]
        internal string AuthIdentifier { get; }

        internal DataStorage(string authToken, string deviceShare, string emailAddress, string phoneNumber, string walletUserId, string authProvider, string authIdentifier)
        {
            this.AuthToken = authToken;
            this.DeviceShare = deviceShare;
            this.EmailAddress = emailAddress;
            this.PhoneNumber = phoneNumber;
            this.WalletUserId = walletUserId;
            this.AuthProvider = authProvider;
            this.AuthIdentifier = authIdentifier;
        }
    }

    [DataContract]
    private class Storage
    {
        [DataMember]
        internal DataStorage Data { get; set; }
    }
}
