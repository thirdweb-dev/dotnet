using Thirdweb;

internal class EmbeddedWallet : PrivateKeyWallet
{
    internal EmbeddedWallet(string privateKeyHex)
        : base(privateKeyHex) { }
}
