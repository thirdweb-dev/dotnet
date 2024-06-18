using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;

namespace Thirdweb.EWS
{
    internal class AWS
    {
        private static readonly RegionEndpoint awsRegion = RegionEndpoint.USWest2;
        private const string cognitoAppClientId = "2e02ha2ce6du13ldk8pai4h3d0";
        private static readonly string cognitoIdentityPoolId = $"{awsRegion.SystemName}:2ad7ab1e-f48b-48a6-adfa-ac1090689c26";
        private static readonly string cognitoUserPoolId = $"{awsRegion.SystemName}_UFwLcZIpq";
        private static readonly string recoverySharePasswordLambdaFunctionName =
            $"arn:aws:lambda:{awsRegion.SystemName}:324457261097:function:recovery-share-password-GenerateRecoverySharePassw-bbE5ZbVAToil";

        internal static async Task SignUpCognitoUserAsync(string emailAddress, string userName)
        {
            emailAddress ??= "cognito@thirdweb.com";
            AmazonCognitoIdentityProviderClient provider = new(new AnonymousAWSCredentials(), awsRegion);
            CognitoUserPool userPool = new(cognitoUserPoolId, cognitoAppClientId, provider);
            Dictionary<string, string> userAttributes = new() { { "email", emailAddress }, };
            await userPool.SignUpAsync(userName, Secrets.Random(12), userAttributes, new Dictionary<string, string>()).ConfigureAwait(false);
        }

        internal static async Task<string> StartCognitoUserAuth(string userName)
        {
            // https://stackoverflow.com/questions/66258459/how-to-get-aws-cognito-access-token-with-username-and-password-in-net-core-3-1
            AmazonCognitoIdentityProviderClient provider = new(new AnonymousAWSCredentials(), awsRegion);
            CognitoUserPool userPool = new(cognitoUserPoolId, cognitoAppClientId, provider);
            CognitoUser user = new(userName, cognitoAppClientId, userPool, provider);
            InitiateCustomAuthRequest customRequest =
                new()
                {
                    AuthParameters = new Dictionary<string, string>() { { "USERNAME", userName }, },
                    ClientMetadata = new Dictionary<string, string>(),
                };
            try
            {
                AuthFlowResponse authResponse = await user.StartWithCustomAuthAsync(customRequest).ConfigureAwait(false);
                return authResponse.SessionID;
            }
            catch (UserNotFoundException)
            {
                return null;
            }
        }

        internal static async Task<TokenCollection> FinishCognitoUserAuth(string userName, string otp, string sessionId)
        {
            AmazonCognitoIdentityProviderClient provider = new(new AnonymousAWSCredentials(), awsRegion);
            CognitoUserPool userPool = new(cognitoUserPoolId, cognitoAppClientId, provider);
            CognitoUser user = new(userName, cognitoAppClientId, userPool, provider);
            RespondToCustomChallengeRequest challengeRequest =
                new()
                {
                    ChallengeParameters = new Dictionary<string, string>() { { "USERNAME", userName }, { "ANSWER", otp }, },
                    ClientMetadata = new Dictionary<string, string>(),
                    SessionID = sessionId,
                };
            try
            {
                AuthFlowResponse authResponse = await user.RespondToCustomAuthAsync(challengeRequest).ConfigureAwait(false);
                AuthenticationResultType result = authResponse.AuthenticationResult ?? throw new VerificationException("The OTP is incorrect", true);
                return new TokenCollection(result.AccessToken, result.IdToken, result.RefreshToken);
            }
            catch (NotAuthorizedException)
            {
                throw new VerificationException("The session expired", false);
            }
            catch (UserNotFoundException)
            {
                throw new InvalidOperationException("The user was not found");
            }
        }

        internal static async Task<MemoryStream> InvokeRecoverySharePasswordLambdaAsync(string idToken, string invokePayload)
        {
            InvokeRequest request = new() { FunctionName = recoverySharePasswordLambdaFunctionName, Payload = invokePayload, };
            CognitoAWSCredentials credentials = new(cognitoIdentityPoolId, awsRegion);
            string providerName = $"cognito-idp.{awsRegion.SystemName}.amazonaws.com/{cognitoUserPoolId}";
            credentials.AddLogin(providerName, idToken);
            AmazonLambdaClient client = new(credentials, awsRegion);
            InvokeResponse lambdaResponse = await client.InvokeAsync(request).ConfigureAwait(false);
            return lambdaResponse.Payload;
        }
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
