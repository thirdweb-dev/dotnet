using Newtonsoft.Json;

namespace Thirdweb;

/// <summary>
/// Provides methods for downloading and uploading data to Thirdweb storage.
/// </summary>
public static class ThirdwebStorage
{
    /// <summary>
    /// Downloads data from the specified URI.
    /// </summary>
    /// <typeparam name="T">The type of data to download.</typeparam>
    /// <param name="client">The Thirdweb client.</param>
    /// <param name="uri">The URI to download from.</param>
    /// <param name="requestTimeout">The optional request timeout in milliseconds.</param>
    /// <returns>The downloaded data.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the URI is null or empty.</exception>
    /// <exception cref="Exception">Thrown if the download fails.</exception>
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

    /// <summary>
    /// Uploads raw byte data to Thirdweb storage.
    /// </summary>
    /// <param name="client">The Thirdweb client.</param>
    /// <param name="rawBytes">The raw byte data to upload.</param>
    /// <returns>The result of the upload.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the raw byte data is null or empty.</exception>
    /// <exception cref="Exception">Thrown if the upload fails.</exception>
    public static async Task<IPFSUploadResult> UploadRaw(ThirdwebClient client, byte[] rawBytes)
    {
        if (rawBytes == null || rawBytes.Length == 0)
        {
            throw new ArgumentNullException(nameof(rawBytes));
        }

        using var form = new MultipartFormDataContent { { new ByteArrayContent(rawBytes), "file", "file" } };

        var httpClient = client.HttpClient;

        var response = await httpClient.PostAsync(Constants.PIN_URI, form).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to upload raw bytes: {response.StatusCode} | {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
        }

        var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var res = JsonConvert.DeserializeObject<IPFSUploadResult>(result);
        res.PreviewUrl = $"https://{client.ClientId}.ipfscdn.io/ipfs/{res.IpfsHash}";
        return res;
    }

    /// <summary>
    /// Uploads a file to Thirdweb storage from the specified path.
    /// </summary>
    /// <param name="client">The Thirdweb client.</param>
    /// <param name="path">The path to the file.</param>
    /// <returns>The result of the upload.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the path is null or empty.</exception>
    public static async Task<IPFSUploadResult> Upload(ThirdwebClient client, string path)
    {
        return string.IsNullOrEmpty(path)
            ? throw new ArgumentNullException(nameof(path))
            : await UploadRaw(client, await File.ReadAllBytesAsync(path).ConfigureAwait(false)).ConfigureAwait(false);
    }
}
