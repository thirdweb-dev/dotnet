using System.Text;
using Newtonsoft.Json;

namespace Thirdweb.AI;

internal class SessionManager
{
    private readonly IThirdwebHttpClient _httpClient;

    public SessionManager(IThirdwebHttpClient httpClient)
    {
        this._httpClient = httpClient;
    }

    public async Task<List<SessionList>> ListSessionsAsync()
    {
        var response = await this._httpClient.GetAsync($"{Constants.NEBULA_API_URL}/session/list");
        _ = response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ResponseModel<List<SessionList>>>(content).Result;
    }

    public async Task<Session> GetSessionAsync(string sessionId)
    {
        var response = await this._httpClient.GetAsync($"{Constants.NEBULA_API_URL}/session/{sessionId}");
        _ = response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ResponseModel<Session>>(content).Result;
    }

    public async Task<Session> CreateSessionAsync(CreateSessionParams parameters)
    {
        var content = new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");
        var response = await this._httpClient.PostAsync($"{Constants.NEBULA_API_URL}/session", content);
        _ = response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ResponseModel<Session>>(responseContent).Result;
    }

    public async Task<Session> UpdateSessionAsync(string sessionId, UpdateSessionParams parameters)
    {
        var content = new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");
        var response = await this._httpClient.PutAsync($"{Constants.NEBULA_API_URL}/session/{sessionId}", content);
        _ = response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ResponseModel<Session>>(responseContent).Result;
    }

    public async Task<SessionDeleteResponse> DeleteSessionAsync(string sessionId)
    {
        var response = await this._httpClient.DeleteAsync($"{Constants.NEBULA_API_URL}/session/{sessionId}");
        _ = response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ResponseModel<SessionDeleteResponse>>(content).Result;
    }

    public async Task<Session> ClearSessionAsync(string sessionId)
    {
        var response = await this._httpClient.PostAsync($"{Constants.NEBULA_API_URL}/session/{sessionId}/clear", null);
        _ = response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ResponseModel<Session>>(content).Result;
    }
}
