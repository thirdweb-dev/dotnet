using System;
using System.Net.Http;
using System.Threading.Tasks;
using Nethereum.JsonRpc.Client.RpcMessages;
using Newtonsoft.Json;

namespace Thirdweb.AccountAbstraction
{
    public static class BundlerClient
    {
        // Bundler requests

        public static async Task<EthGetUserOperationReceiptResponse> EthGetUserOperationReceipt(ThirdwebClient client, string bundlerUrl, object requestId, string userOpHash)
        {
            var response = await BundlerRequest(client, bundlerUrl, requestId, "eth_getUserOperationReceipt", userOpHash);
            return JsonConvert.DeserializeObject<EthGetUserOperationReceiptResponse>(response.Result.ToString());
        }

        public static async Task<string> EthSendUserOperation(ThirdwebClient client, string bundlerUrl, object requestId, UserOperationHexified userOp, string entryPoint)
        {
            var response = await BundlerRequest(client, bundlerUrl, requestId, "eth_sendUserOperation", userOp, entryPoint);
            return response.Result.ToString();
        }

        public static async Task<EthEstimateUserOperationGasResponse> EthEstimateUserOperationGas(
            ThirdwebClient client,
            string bundlerUrl,
            object requestId,
            UserOperationHexified userOp,
            string entryPoint
        )
        {
            var response = await BundlerRequest(client, bundlerUrl, requestId, "eth_estimateUserOperationGas", userOp, entryPoint);
            return JsonConvert.DeserializeObject<EthEstimateUserOperationGasResponse>(response.Result.ToString());
        }

        public static async Task<ThirdwebGetUserOperationGasPriceResponse> ThirdwebGetUserOperationGasPrice(ThirdwebClient client, string bundlerUrl, object requestId)
        {
            var response = await BundlerRequest(client, bundlerUrl, requestId, "thirdweb_getUserOperationGasPrice");
            return JsonConvert.DeserializeObject<ThirdwebGetUserOperationGasPriceResponse>(response.Result.ToString());
        }

        // Paymaster requests

        public static async Task<PMSponsorOperationResponse> PMSponsorUserOperation(ThirdwebClient client, string paymasterUrl, object requestId, UserOperationHexified userOp, string entryPoint)
        {
            var response = await BundlerRequest(client, paymasterUrl, requestId, "pm_sponsorUserOperation", userOp, new EntryPointWrapper() { entryPoint = entryPoint });
            try
            {
                return JsonConvert.DeserializeObject<PMSponsorOperationResponse>(response.Result.ToString());
            }
            catch
            {
                return new PMSponsorOperationResponse() { paymasterAndData = response.Result.ToString() };
            }
        }

        public static async Task<PMSponsorTransactionResponse> PMSponsorTransaction(ThirdwebClient client, string paymasterUrl, object requestId, ThirdwebTransactionInput txInput)
        {
            var response = await BundlerRequest(client, paymasterUrl, requestId, "pm_sponsorTransaction", txInput);
            try
            {
                return JsonConvert.DeserializeObject<PMSponsorTransactionResponse>(response.Result.ToString());
            }
            catch
            {
                return new PMSponsorTransactionResponse() { paymaster = null, paymasterInput = null };
            }
        }

        // Request

        private static async Task<RpcResponseMessage> BundlerRequest(ThirdwebClient client, string url, object requestId, string method, params object[] args)
        {
            using var httpClient = new HttpClient();
#if DEBUG
            Console.WriteLine($"Bundler Request: {method}({JsonConvert.SerializeObject(args)}");
#endif
            var requestMessage = new RpcRequestMessage(requestId, method, args);
            var requestMessageJson = JsonConvert.SerializeObject(requestMessage);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url) { Content = new StringContent(requestMessageJson, System.Text.Encoding.UTF8, "application/json") };
            if (new Uri(url).Host.EndsWith(".thirdweb.com"))
            {
                httpRequestMessage.Headers.Add("x-sdk-name", "Thirdweb.NET");
                httpRequestMessage.Headers.Add("x-sdk-os", System.Runtime.InteropServices.RuntimeInformation.OSDescription);
                httpRequestMessage.Headers.Add("x-sdk-platform", "dotnet");
                httpRequestMessage.Headers.Add("x-sdk-version", Constants.VERSION);
                if (!string.IsNullOrEmpty(client.ClientId))
                {
                    httpRequestMessage.Headers.Add("x-client-id", client.ClientId);
                }

                if (!string.IsNullOrEmpty(client.SecretKey))
                {
                    httpRequestMessage.Headers.Add("x-secret-key", client.SecretKey);
                }

                if (!string.IsNullOrEmpty(client.BundleId))
                {
                    httpRequestMessage.Headers.Add("x-bundle-id", client.BundleId);
                }
            }

            var httpResponse = await httpClient.SendAsync(httpRequestMessage);

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Bundler Request Failed. Error: {httpResponse.StatusCode} - {httpResponse.ReasonPhrase} - {await httpResponse.Content.ReadAsStringAsync()}");
            }

            var httpResponseJson = await httpResponse.Content.ReadAsStringAsync();

#if DEBUG
            Console.WriteLine($"Bundler Response: {httpResponseJson}");
#endif

            var response = JsonConvert.DeserializeObject<RpcResponseMessage>(httpResponseJson);
            return response.Error != null ? throw new Exception($"Bundler Request Failed. Error: {response.Error.Code} - {response.Error.Message} - {response.Error.Data}") : response;
        }
    }
}
