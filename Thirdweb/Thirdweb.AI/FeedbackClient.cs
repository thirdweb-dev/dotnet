using System.Text;
using Newtonsoft.Json;

namespace Thirdweb.AI;

internal class FeedbackClient
{
    private readonly IThirdwebHttpClient _httpClient;

    public FeedbackClient(IThirdwebHttpClient httpClient)
    {
        this._httpClient = httpClient;
    }

    /// <summary>
    /// Submits feedback for a specific session and request.
    /// </summary>
    /// <param name="feedback">The feedback parameters to submit.</param>
    /// <returns>The submitted feedback details.</returns>
    public async Task<Feedback> SubmitFeedbackAsync(FeedbackParams feedback)
    {
        var content = new StringContent(JsonConvert.SerializeObject(feedback), Encoding.UTF8, "application/json");
        var response = await this._httpClient.PostAsync($"{Constants.NEBULA_API_URL}/feedback", content);
        _ = response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ResponseModel<Feedback>>(responseContent).Result;
    }
}
