using System.Text;
using Newtonsoft.Json;

namespace Thirdweb.AI;

internal class ExecutionClient
{
    private readonly IThirdwebHttpClient _httpClient;

    public ExecutionClient(IThirdwebHttpClient httpClient)
    {
        this._httpClient = httpClient;
    }

    public async Task<ChatResponse> ExecuteAsync(ChatParamsSingleMessage command)
    {
        var content = new StringContent(JsonConvert.SerializeObject(command), Encoding.UTF8, "application/json");
        var response = await this._httpClient.PostAsync($"{Constants.NEBULA_API_URL}/execute", content);
        _ = response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ChatResponse>(responseContent);
    }

    public async Task<ChatResponse> ExecuteBatchAsync(ChatParamsMultiMessages commands)
    {
        var content = new StringContent(JsonConvert.SerializeObject(commands), Encoding.UTF8, "application/json");
        var response = await this._httpClient.PostAsync($"{Constants.NEBULA_API_URL}/execute", content);
        _ = response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ChatResponse>(responseContent);
    }
}
