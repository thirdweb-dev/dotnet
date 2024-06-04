using Newtonsoft.Json;

namespace Thirdweb
{
    public static class ThirdwebStorage
    {
        public static async Task<T> Download<T>(ThirdwebClient client, string uri, int? requestTimeout = null)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentNullException(nameof(uri));
            }

            uri = uri.ReplaceIPFS($"https://{client.ClientId}.ipfscdn.io/ipfs/");

            using var httpClient = ThirdwebHttpClientFactory.CreateThirdwebHttpClient();

            if (Utils.IsThirdwebRequest(uri))
            {
                var headers = Utils.GetThirdwebHeaders(client);
                httpClient.SetHeaders(headers);
            }

            requestTimeout ??= client.FetchTimeoutOptions.GetTimeout(TimeoutType.Storage);

            var response = await httpClient.GetAsync(uri, new CancellationTokenSource(requestTimeout.Value).Token);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to download {uri}: {response.StatusCode} | {response.ReasonPhrase} | {await response.Content.ReadAsStringAsync()}");
            }

            var content = await response.Content.ReadAsStringAsync();

            return typeof(T) == typeof(string) ? (T)(object)content : JsonConvert.DeserializeObject<T>(content);
        }

        public static async Task<IPFSUploadResult> Upload(ThirdwebClient client, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            using var form = new MultipartFormDataContent { { new ByteArrayContent(File.ReadAllBytes(path)), "file", Path.GetFileName(path) } };

            using var httpClient = ThirdwebHttpClientFactory.CreateThirdwebHttpClient();

            var headers = Utils.GetThirdwebHeaders(client);
            httpClient.SetHeaders(headers);

            var response = await httpClient.PostAsync(Constants.PIN_URI, form);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to upload {path}: {response.StatusCode} | {response.ReasonPhrase} | {await response.Content.ReadAsStringAsync()}");
            }

            var result = await response.Content.ReadAsStringAsync();

            var res = JsonConvert.DeserializeObject<IPFSUploadResult>(result);
            res.PreviewUrl = $"https://{client.ClientId}.ipfscdn.io/ipfs/{res.IpfsHash}";
            return res;
        }
    }
}
