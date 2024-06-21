using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Thirdweb.EWS
{
    internal class AWS
    {
        private static readonly string awsRegion = "us-west-2";
        private const string cognitoAppClientId = "2e02ha2ce6du13ldk8pai4h3d0";
        private static readonly string cognitoIdentityPoolId = $"{awsRegion}:2ad7ab1e-f48b-48a6-adfa-ac1090689c26";
        private static readonly string cognitoUserPoolId = $"{awsRegion}_UFwLcZIpq";
        private static readonly string recoverySharePasswordLambdaFunctionName = "arn:aws:lambda:us-west-2:324457261097:function:recovery-share-password-GenerateRecoverySharePassw-bbE5ZbVAToil";

        internal static async Task SignUpCognitoUserAsync(string emailAddress, string userName)
        {
            emailAddress ??= "cognito@thirdweb.com";

            using var client = new HttpClient();
            var endpoint = $"https://cognito-idp.{awsRegion}.amazonaws.com/";
            var payload = new
            {
                ClientId = cognitoAppClientId,
                Username = userName,
                Password = Secrets.Random(12),
                UserAttributes = new[] { new { Name = "email", Value = emailAddress } }
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/x-amz-json-1.1");

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };

            request.Headers.Add("X-Amz-Target", "AWSCognitoIdentityProviderService.SignUp");

            var response = await client.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new Exception($"Sign-up failed: {responseBody}");
            }
        }

        internal static async Task<string> StartCognitoUserAuth(string userName)
        {
            using var client = new HttpClient();
            var endpoint = $"https://cognito-idp.{awsRegion}.amazonaws.com/";
            var payload = new
            {
                AuthFlow = "CUSTOM_AUTH",
                ClientId = cognitoAppClientId,
                AuthParameters = new Dictionary<string, string> { { "USERNAME", userName } },
                ClientMetadata = new Dictionary<string, string>()
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/x-amz-json-1.1");

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };

            request.Headers.Add("X-Amz-Target", "AWSCognitoIdentityProviderService.InitiateAuth");

            var response = await client.SendAsync(request).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent);
                if (errorResponse.Type == "UserNotFoundException")
                {
                    return null;
                }
                throw new Exception($"Authentication initiation failed: {responseContent}");
            }

            var jsonResponse = JsonConvert.DeserializeObject<StartAuthResponse>(responseContent);
            return jsonResponse.Session;
        }

        internal static async Task<TokenCollection> FinishCognitoUserAuth(string userName, string otp, string sessionId)
        {
            using var client = new HttpClient();
            var endpoint = $"https://cognito-idp.{awsRegion}.amazonaws.com/";
            var payload = new
            {
                ChallengeName = "CUSTOM_CHALLENGE",
                ClientId = cognitoAppClientId,
                ChallengeResponses = new Dictionary<string, string> { { "USERNAME", userName }, { "ANSWER", otp } },
                Session = sessionId
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/x-amz-json-1.1");

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };

            request.Headers.Add("X-Amz-Target", "AWSCognitoIdentityProviderService.RespondToAuthChallenge");

            var response = await client.SendAsync(request).ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent);
                if (errorResponse.Type == "NotAuthorizedException")
                {
                    throw new VerificationException("The session expired", false);
                }
                if (errorResponse.Type == "UserNotFoundException")
                {
                    throw new InvalidOperationException("The user was not found");
                }
                throw new Exception($"Challenge response failed: {responseContent}");
            }

            var jsonResponse = JsonConvert.DeserializeObject<FinishAuthResponse>(responseContent);
            var result = jsonResponse.AuthenticationResult ?? throw new VerificationException("The OTP is incorrect", true);
            return new TokenCollection(result.AccessToken.ToString(), result.IdToken.ToString(), result.RefreshToken.ToString());
        }

        internal static async Task<MemoryStream> InvokeRecoverySharePasswordLambdaAsync(string idToken, string invokePayload)
        {
            var credentials = await GetTemporaryCredentialsAsync(idToken);
            return await InvokeLambdaWithTemporaryCredentialsAsync(credentials, invokePayload);
        }

        private static async Task<AwsCredentials> GetTemporaryCredentialsAsync(string idToken)
        {
            using var client = new HttpClient();
            var endpoint = $"https://cognito-identity.{awsRegion}.amazonaws.com/";

            var payloadForGetId = new { IdentityPoolId = cognitoIdentityPoolId, Logins = new Dictionary<string, string> { { $"cognito-idp.{awsRegion}.amazonaws.com/{cognitoUserPoolId}", idToken } } };

            var content = new StringContent(JsonConvert.SerializeObject(payloadForGetId), Encoding.UTF8, "application/x-amz-json-1.1");

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };

            request.Headers.Add("X-Amz-Target", "AWSCognitoIdentityService.GetId");

            var response = await client.SendAsync(request).ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get identity ID: {responseContent}");
            }

            var identityIdResponse = JsonConvert.DeserializeObject<GetIdResponse>(responseContent);

            var payloadForGetCredentials = new
            {
                IdentityId = identityIdResponse.IdentityId,
                Logins = new Dictionary<string, string> { { $"cognito-idp.{awsRegion}.amazonaws.com/{cognitoUserPoolId}", idToken } }
            };

            content = new StringContent(JsonConvert.SerializeObject(payloadForGetCredentials), Encoding.UTF8, "application/x-amz-json-1.1");

            request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };

            request.Headers.Add("X-Amz-Target", "AWSCognitoIdentityService.GetCredentialsForIdentity");

            response = await client.SendAsync(request).ConfigureAwait(false);
            responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

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

        private static async Task<MemoryStream> InvokeLambdaWithTemporaryCredentialsAsync(AwsCredentials credentials, string invokePayload)
        {
            var endpoint = $"https://lambda.{awsRegion}.amazonaws.com/2015-03-31/functions/{recoverySharePasswordLambdaFunctionName}/invocations";
            var requestBody = new StringContent(invokePayload, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = requestBody };

            var dateTimeNow = DateTime.UtcNow;
            var dateStamp = dateTimeNow.ToString("yyyyMMdd");
            var amzDate = dateTimeNow.ToString("yyyyMMddTHHmmssZ");

            var canonicalUri = "/2015-03-31/functions/" + Uri.EscapeDataString(recoverySharePasswordLambdaFunctionName) + "/invocations";
            var canonicalQueryString = "";
            var canonicalHeaders = $"host:lambda.{awsRegion}.amazonaws.com\nx-amz-date:{amzDate}\n";
            var signedHeaders = "host;x-amz-date";

            using var sha256 = SHA256.Create();
            var payloadHash = ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(invokePayload)));
            var canonicalRequest = $"POST\n{canonicalUri}\n{canonicalQueryString}\n{canonicalHeaders}\n{signedHeaders}\n{payloadHash}";

            var algorithm = "AWS4-HMAC-SHA256";
            var credentialScope = $"{dateStamp}/{awsRegion}/lambda/aws4_request";
            var stringToSign = $"{algorithm}\n{amzDate}\n{credentialScope}\n{ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(canonicalRequest)))}";

            var signingKey = GetSignatureKey(credentials.SecretAccessKey, dateStamp, awsRegion, "lambda");
            var signature = ToHexString(HMACSHA256(signingKey, stringToSign));

            var authorizationHeader = $"{algorithm} Credential={credentials.AccessKeyId}/{credentialScope}, SignedHeaders={signedHeaders}, Signature={signature}";

            request.Headers.Add("x-amz-date", amzDate);
            request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
            request.Headers.Add("x-amz-security-token", credentials.SessionToken);

            var response = await client.SendAsync(request).ConfigureAwait(false);
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
