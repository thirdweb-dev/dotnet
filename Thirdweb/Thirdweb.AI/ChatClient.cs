using System.Text;
using Newtonsoft.Json;

namespace Thirdweb.AI;

internal class ChatClient
{
    private readonly IThirdwebHttpClient _httpClient;

    public ChatClient(IThirdwebHttpClient httpClient)
    {
        this._httpClient = httpClient;
    }

    public async Task<ChatResponse> SendMessageAsync(ChatParamsSingleMessage message)
    {
        var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");
        var response = await this._httpClient.PostAsync($"{Constants.NEBULA_API_URL}/chat", content);
        _ = response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ChatResponse>(responseContent);
    }

    public async Task<ChatResponse> SendMessagesAsync(ChatParamsMultiMessages messages)
    {
        var content = new StringContent(JsonConvert.SerializeObject(messages), Encoding.UTF8, "application/json");
        var response = await this._httpClient.PostAsync($"{Constants.NEBULA_API_URL}/chat", content);
        _ = response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ChatResponse>(responseContent);
    }
}
