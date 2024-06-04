namespace Thirdweb
{
    public static class ThirdwebHttpClientFactory
    {
        public static IThirdwebHttpClient CreateThirdwebHttpClient(Dictionary<string, string> headers = null)
        {
            IThirdwebHttpClient client;
#if UNITY_5_3_OR_NEWER
            client = new UnityThirdwebHttpClient();
#else
            client = new ThirdwebHttpClient();
#endif
            if (headers != null)
            {
                client.SetHeaders(headers);
            }
            return client;
        }
    }
}
