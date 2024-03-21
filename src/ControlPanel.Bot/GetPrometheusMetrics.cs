using System.Net.Http.Headers;

namespace TelegramWorker;

public class GetPrometheusMetrics
{
    private readonly HttpClient _httpClient;

    public GetPrometheusMetrics(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // public async Task<string> GetResourceAsync(string resourceUrl, string authToken)
    // {
    //     
    //     _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ControlPanel.Core.AuthManager.CreateSimpleJwtToken());
    //
    //     var response = await _httpClient.GetAsync("http://localhost:5143/Prometheus/");
    //
    //     if (response.IsSuccessStatusCode)
    //     {
    //         return await response.Content.ReadAsStringAsync();
    //     }
    //     else
    //     {
    //         throw new HttpRequestException($"Error: {response.StatusCode}");
    //     }
    // }
}