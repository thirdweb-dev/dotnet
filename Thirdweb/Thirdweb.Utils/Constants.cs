namespace Thirdweb
{
    public static class Constants
    {
        public const string ADDRESS_ZERO = "0x0000000000000000000000000000000000000000";
        public const string NATIVE_TOKEN_ADDRESS = "0xEeeeeEeeeEeEeeEeEeEeeEEEeeeeEeeeeeeeEEeE";
        public const double DECIMALS_18 = 1000000000000000000;

        internal const string VERSION = "0.1.0";
        internal const int DEFAULT_FETCH_TIMEOUT = 60000;
        internal const string DEFAULT_ENTRYPOINT_ADDRESS = "0x5FF137D4b0FDCD49DcA30c7CF57E578a026d2789"; // v0.6
        internal const string DUMMY_SIG = "0xfffffffffffffffffffffffffffffff0000000000000000000000000000000007aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa1c";
        internal const string DUMMY_PAYMASTER_AND_DATA_HEX =
            "0x0101010101010101010101010101010101010101000000000000000000000000000000000000000000000000000001010101010100000000000000000000000000000000000000000000000000000000000000000101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101";
        internal const string FALLBACK_IPFS_GATEWAY = "https://ipfs.io/ipfs/";
        internal const string PIN_URI = "https://storage.thirdweb.com/ipfs/upload";
    }
}
