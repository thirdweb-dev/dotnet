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

            using var cts = new CancellationTokenSource(requestTimeout ?? client.FetchTimeoutOptions.GetTimeout(TimeoutType.Storage));

            var httpClient = client.HttpClient;

            var response = await httpClient.GetAsync(uri, cts.Token).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to download {uri}: {response.StatusCode} | {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
            }

            if (typeof(T) == typeof(byte[]))
            {
                return (T)(object)await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }
            else if (typeof(T) == typeof(string))
            {
                return (T)(object)await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            else
            {
                var content = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<T>(System.Text.Encoding.UTF8.GetString(content));
            }
        }

        public static async Task<IPFSUploadResult> Upload(ThirdwebClient client, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            using var form = new MultipartFormDataContent { { new ByteArrayContent(File.ReadAllBytes(path)), "file", Path.GetFileName(path) } };

            var httpClient = client.HttpClient;

            var response = await httpClient.PostAsync(Constants.PIN_URI, form).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to upload {path}: {response.StatusCode} | {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
            }

            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var res = JsonConvert.DeserializeObject<IPFSUploadResult>(result);
            res.PreviewUrl = $"https://{client.ClientId}.ipfscdn.io/ipfs/{res.IpfsHash}";
            return res;
        }
    }
}
