using Nethereum.Web3.Accounts;

namespace Thirdweb.EWS;

internal class User
{
    internal User(Account account, string emailAddress, string phoneNumber)
    {
        this.Account = account;
        this.EmailAddress = emailAddress;
        this.PhoneNumber = phoneNumber;
    }

    public Account Account { get; internal set; }
    public string EmailAddress { get; internal set; }
    public string PhoneNumber { get; internal set; }
}
