using Nethereum.Web3.Accounts;

namespace Thirdweb.EWS
{
    internal class User
    {
        internal User(Account account, string emailAddress)
        {
            Account = account;
            EmailAddress = emailAddress;
        }

        public Account Account { get; internal set; }
        public string EmailAddress { get; internal set; }
    }
}
