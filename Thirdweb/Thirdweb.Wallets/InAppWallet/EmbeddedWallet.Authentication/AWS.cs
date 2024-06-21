using System.Text;
using Amazon.CognitoIdentity;
using Amazon.Lambda;
using Amazon.Lambda.Model;
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
            var region = Amazon.RegionEndpoint.USWest2;
            var request = new InvokeRequest() { FunctionName = recoverySharePasswordLambdaFunctionName, Payload = invokePayload, };
            var credentials = new CognitoAWSCredentials(cognitoIdentityPoolId, region);
            var providerName = $"cognito-idp.{awsRegion}.amazonaws.com/{cognitoUserPoolId}";
            credentials.AddLogin(providerName, idToken);
            var client = new AmazonLambdaClient(credentials, region);
            var lambdaResponse = await client.InvokeAsync(request).ConfigureAwait(false);
            return lambdaResponse.Payload;
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
    }
}
