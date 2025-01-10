using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Thirdweb.EWS;

internal class AWS
{
    private const string AWS_REGION = "us-west-2";

    private static readonly string _recoverySharePasswordLambdaFunctionNameV2 = $"arn:aws:lambda:{AWS_REGION}:324457261097:function:lambda-thirdweb-auth-enc-key-prod-ThirdwebAuthEncKeyFunction";
    private static readonly string _migrationKeyId = $"arn:aws:kms:{AWS_REGION}:324457261097:key/ccfb9ecd-f45d-4f37-864a-25fe72dcb49e";

    internal static async Task<MemoryStream> InvokeRecoverySharePasswordLambdaAsync(string identityId, string token, string invokePayload, IThirdwebHttpClient httpClient)
    {
        var credentials = await GetTemporaryCredentialsAsync(identityId, token, httpClient).ConfigureAwait(false);
        return await InvokeLambdaWithTemporaryCredentialsAsync(credentials, invokePayload, httpClient, _recoverySharePasswordLambdaFunctionNameV2).ConfigureAwait(false);
    }

    internal static async Task<JToken> GenerateDataKey(string identityId, string token, IThirdwebHttpClient httpClient)
    {
        var credentials = await GetTemporaryCredentialsAsync(identityId, token, httpClient).ConfigureAwait(false);
        return await GenerateDataKey(credentials, httpClient).ConfigureAwait(false);
    }

    private static async Task<AwsCredentials> GetTemporaryCredentialsAsync(string identityId, string token, IThirdwebHttpClient httpClient)
    {
        var client = Utils.ReconstructHttpClient(httpClient);
        var endpoint = $"https://cognito-identity.{AWS_REGION}.amazonaws.com/";

        var payloadForGetCredentials = new { IdentityId = identityId, Logins = new Dictionary<string, string> { { "cognito-identity.amazonaws.com", token } } };

        var content = new StringContent(JsonConvert.SerializeObject(payloadForGetCredentials), Encoding.UTF8, "application/x-amz-json-1.1");

        client.AddHeader("X-Amz-Target", "AWSCognitoIdentityService.GetCredentialsForIdentity");

        var response = await client.PostAsync(endpoint, content).ConfigureAwait(false);
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get credentials: {responseContent}");
        }

        var credentialsResponse = JsonConvert.DeserializeObject<GetCredentialsForIdentityResponse>(responseContent);

        return new AwsCredentials
        {
            AccessKeyId = credentialsResponse.Credentials.AccessKeyId,
            SecretAccessKey = credentialsResponse.Credentials.SecretKey,
            SessionToken = credentialsResponse.Credentials.SessionToken
        };
    }

    private static async Task<JToken> GenerateDataKey(AwsCredentials credentials, IThirdwebHttpClient httpClient)
    {
        var endpoint = $"https://kms.{AWS_REGION}.amazonaws.com/";

        var payloadForGenerateDataKey = new { KeyId = _migrationKeyId, KeySpec = "AES_256" };
        var requestBodyString = JsonConvert.SerializeObject(payloadForGenerateDataKey);

        var contentType = "application/x-amz-json-1.1";

        var extraHeaders = new Dictionary<string, string> { { "X-Amz-Target", "TrentService.GenerateDataKey" } };

        var responseContent = await PostAwsRequestWithDateOverride(credentials, httpClient, AWS_REGION, "kms", endpoint, "/", "", requestBodyString, contentType, extraHeaders).ConfigureAwait(false);

        var responseObject = JToken.Parse(responseContent);
        var plaintextKeyBlob = responseObject["Plaintext"];
        var cipherTextBlob = responseObject["CiphertextBlob"];

        if (plaintextKeyBlob == null || cipherTextBlob == null)
        {
            throw new Exception("No migration key found. Please try again.");
        }

        return responseObject;
    }

    private static async Task<MemoryStream> InvokeLambdaWithTemporaryCredentialsAsync(AwsCredentials credentials, string invokePayload, IThirdwebHttpClient httpClient, string lambdaFunction)
    {
        var endpoint = $"https://lambda.{AWS_REGION}.amazonaws.com/2015-03-31/functions/{lambdaFunction}/invocations";
        var contentType = "application/json";

        var canonicalUri = $"/2015-03-31/functions/{Uri.EscapeDataString(lambdaFunction)}/invocations";
        var canonicalQueryString = "";

        var extraHeaders = new Dictionary<string, string>();

        var responseContent = await PostAwsRequestWithDateOverride(
                credentials,
                httpClient,
                AWS_REGION,
                "lambda",
                endpoint,
                canonicalUri,
                canonicalQueryString,
                invokePayload,
                contentType,
                extraHeaders
            )
            .ConfigureAwait(false);

        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(responseContent));
        return memoryStream;
    }

    private static async Task<string> PostAwsRequestWithDateOverride(
        AwsCredentials credentials,
        IThirdwebHttpClient httpClient,
        string region,
        string service,
        string endpoint,
        string canonicalUri,
        string canonicalQueryString,
        string requestBodyString,
        string contentType,
        Dictionary<string, string> extraHeaders,
        DateTime? dateOverride = null
    )
    {
        var client = Utils.ReconstructHttpClient(httpClient);

        if (extraHeaders != null)
        {
            foreach (var kvp in extraHeaders)
            {
                client.AddHeader(kvp.Key, kvp.Value);
            }
        }

        var dateTimeNow = dateOverride ?? DateTime.UtcNow;
        var amzDateFormat = "yyyyMMddTHHmmssZ";
        var amzDate = dateTimeNow.ToString(amzDateFormat);
        var dateStamp = dateTimeNow.ToString("yyyyMMdd");

        var canonicalHeaders = $"host:{new Uri(endpoint).Host}\n" + $"x-amz-date:{amzDate}\n";
        var signedHeaders = "host;x-amz-date";

#if NETSTANDARD
        using var sha256 = SHA256.Create();
        var payloadHash = ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(requestBodyString)));
#else
        var payloadHash = ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(requestBodyString)));
#endif

        var canonicalRequest = $"POST\n{canonicalUri}\n{canonicalQueryString}\n{canonicalHeaders}\n{signedHeaders}\n{payloadHash}";

        var algorithm = "AWS4-HMAC-SHA256";
        var credentialScope = $"{dateStamp}/{region}/{service}/aws4_request";
#if NETSTANDARD
        var stringToSign = $"{algorithm}\n{amzDate}\n{credentialScope}\n{ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(canonicalRequest)))}";
#else
        var stringToSign = $"{algorithm}\n{amzDate}\n{credentialScope}\n{ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalRequest)))}";
#endif

        var signingKey = GetSignatureKey(credentials.SecretAccessKey, dateStamp, region, service);
        var signature = ToHexString(HMACSHA256(signingKey, stringToSign));

        var authorizationHeader = $"{algorithm} Credential={credentials.AccessKeyId}/{credentialScope}, SignedHeaders={signedHeaders}, Signature={signature}";

        client.AddHeader("x-amz-date", amzDate);
        client.AddHeader("Authorization", authorizationHeader);

        if (!string.IsNullOrEmpty(credentials.SessionToken))
        {
            client.AddHeader("x-amz-security-token", credentials.SessionToken);
        }

        var content = new StringContent(requestBodyString, Encoding.UTF8, contentType);

        var response = await client.PostAsync(endpoint, content).ConfigureAwait(false);
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            if (dateOverride == null && responseContent.Contains("Signature expired"))
            {
                var idx = responseContent.LastIndexOf('(');
                if (idx > -1)
                {
                    var parsedTimeString = responseContent.Substring(idx + 1, amzDate.Length);
                    var serverTime = DateTime.ParseExact(parsedTimeString, amzDateFormat, System.Globalization.CultureInfo.InvariantCulture).ToUniversalTime();

                    Console.WriteLine($"Server time: {serverTime}");

                    return await PostAwsRequestWithDateOverride(
                            credentials,
                            httpClient,
                            region,
                            service,
                            endpoint,
                            canonicalUri,
                            canonicalQueryString,
                            requestBodyString,
                            contentType,
                            extraHeaders,
                            serverTime
                        )
                        .ConfigureAwait(false);
                }
            }

            throw new Exception($"AWS request failed: {responseContent}");
        }

        return responseContent;
    }

    private static byte[] HMACSHA256(byte[] key, string data)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    private static byte[] GetSignatureKey(string key, string dateStamp, string regionName, string serviceName)
    {
        var kDate = HMACSHA256(Encoding.UTF8.GetBytes("AWS4" + key), dateStamp);
        var kRegion = HMACSHA256(kDate, regionName);
        var kService = HMACSHA256(kRegion, serviceName);
        var kSigning = HMACSHA256(kService, "aws4_request");
        return kSigning;
    }

    private static string ToHexString(byte[] bytes)
    {
        var hex = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
        {
            _ = hex.AppendFormat("{0:x2}", b);
        }
        return hex.ToString();
    }

    internal class GetIdResponse
    {
        public string IdentityId { get; set; }
    }

    internal class GetCredentialsForIdentityResponse
    {
        public Credentials Credentials { get; set; }
    }

    internal class Credentials
    {
        public string AccessKeyId { get; set; }
        public string SecretKey { get; set; }
        public string SessionToken { get; set; }
    }

    internal class AwsCredentials
    {
        public string AccessKeyId { get; set; }
        public string SecretAccessKey { get; set; }
        public string SessionToken { get; set; }
    }

    internal class CredentialsResponse
    {
        public Credentials Credentials { get; set; }
    }

    internal class StartAuthResponse
    {
        public string Session { get; set; }
    }

    internal class FinishAuthResponse
    {
        public AuthenticationResult AuthenticationResult { get; set; }
    }

    internal class AuthenticationResult
    {
        public string AccessToken { get; set; }
        public string IdToken { get; set; }
        public string RefreshToken { get; set; }
    }

    internal class ErrorResponse
    {
        [JsonProperty("__type")]
        public string Type { get; set; }
        public string Message { get; set; }
    }

    internal class TokenCollection
    {
        internal TokenCollection(string accessToken, string idToken, string refreshToken)
        {
            this.AccessToken = accessToken;
            this.IdToken = idToken;
            this.RefreshToken = refreshToken;
        }

        public string AccessToken { get; }
        public string IdToken { get; }
        public string RefreshToken { get; }
    }
}
