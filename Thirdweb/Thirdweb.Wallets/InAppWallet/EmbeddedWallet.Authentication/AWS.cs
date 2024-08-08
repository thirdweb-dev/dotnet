using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Thirdweb.EWS
{
    internal class AWS
    {
        private const string AWS_REGION = "us-west-2";

        private static readonly string recoverySharePasswordLambdaFunctionNameV2 = $"arn:aws:lambda:{AWS_REGION}:324457261097:function:lambda-thirdweb-auth-enc-key-prod-ThirdwebAuthEncKeyFunction";

        internal static async Task<MemoryStream> InvokeRecoverySharePasswordLambdaAsync(string identityId, string token, string invokePayload, Type thirdwebHttpClientType)
        {
            var credentials = await GetTemporaryCredentialsAsync(identityId, token, thirdwebHttpClientType).ConfigureAwait(false);
            return await InvokeLambdaWithTemporaryCredentialsAsync(credentials, invokePayload, thirdwebHttpClientType, recoverySharePasswordLambdaFunctionNameV2).ConfigureAwait(false);
        }

        private static async Task<AwsCredentials> GetTemporaryCredentialsAsync(string identityId, string token, Type thirdwebHttpClientType)
        {
            var client = thirdwebHttpClientType.GetConstructor(Type.EmptyTypes).Invoke(null) as IThirdwebHttpClient;
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

        private static async Task<MemoryStream> InvokeLambdaWithTemporaryCredentialsAsync(AwsCredentials credentials, string invokePayload, Type thirdwebHttpClientType, string lambdaFunction)
        {
            var endpoint = $"https://lambda.{AWS_REGION}.amazonaws.com/2015-03-31/functions/{lambdaFunction}/invocations";
            var requestBody = new StringContent(invokePayload, Encoding.UTF8, "application/json");

            var client = thirdwebHttpClientType.GetConstructor(Type.EmptyTypes).Invoke(null) as IThirdwebHttpClient;

            var dateTimeNow = DateTime.UtcNow;
            var dateStamp = dateTimeNow.ToString("yyyyMMdd");
            var amzDate = dateTimeNow.ToString("yyyyMMddTHHmmssZ");

            var canonicalUri = "/2015-03-31/functions/" + Uri.EscapeDataString(lambdaFunction) + "/invocations";
            var canonicalQueryString = "";
            var canonicalHeaders = $"host:lambda.{AWS_REGION}.amazonaws.com\nx-amz-date:{amzDate}\n";
            var signedHeaders = "host;x-amz-date";

            using var sha256 = SHA256.Create();
            var payloadHash = ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(invokePayload)));
            var canonicalRequest = $"POST\n{canonicalUri}\n{canonicalQueryString}\n{canonicalHeaders}\n{signedHeaders}\n{payloadHash}";

            var algorithm = "AWS4-HMAC-SHA256";
            var credentialScope = $"{dateStamp}/{AWS_REGION}/lambda/aws4_request";
            var stringToSign = $"{algorithm}\n{amzDate}\n{credentialScope}\n{ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(canonicalRequest)))}";

            var signingKey = GetSignatureKey(credentials.SecretAccessKey, dateStamp, AWS_REGION, "lambda");
            var signature = ToHexString(HMACSHA256(signingKey, stringToSign));

            var authorizationHeader = $"{algorithm} Credential={credentials.AccessKeyId}/{credentialScope}, SignedHeaders={signedHeaders}, Signature={signature}";

            client.AddHeader("x-amz-date", amzDate);
            client.AddHeader("Authorization", authorizationHeader);
            client.AddHeader("x-amz-security-token", credentials.SessionToken);

            var response = await client.PostAsync(endpoint, requestBody).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Lambda invocation failed: {responseContent}");
            }

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(responseContent));
            return memoryStream;
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
                hex.AppendFormat("{0:x2}", b);
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
                AccessToken = accessToken;
                IdToken = idToken;
                RefreshToken = refreshToken;
            }

            public string AccessToken { get; }
            public string IdToken { get; }
            public string RefreshToken { get; }
        }
    }
}
