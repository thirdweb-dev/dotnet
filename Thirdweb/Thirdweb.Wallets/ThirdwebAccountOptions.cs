using System.Numerics;

namespace Thirdweb
{
    public class ThirdwebAccountOptions
    {
        internal ThirdwebClient Client { get; private set; }
        internal WalletType Type { get; private set; }
        internal string Email { get; private set; }
        internal string PrivateKey { get; private set; }

        public ThirdwebAccountOptions(ThirdwebClient client, WalletType type, string email = null, string privateKey = null)
        {
            Client = client;
            Type = type;
            Email = email;
            PrivateKey = privateKey;
        }
    }
}
