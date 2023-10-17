using ControlPanel.Core.Models;
using ControlPanel.Core.Request;
using ControlPanel.Data;
using Newtonsoft.Json;


namespace ControlPanel.Core
{
    public class PrometheusExporter
    {
        private readonly AppDbContext _authContext;
        private readonly IHttpClientFactory _httpClientFactory;
        public PrometheusExporter(AppDbContext appDbContext, IHttpClientFactory httpClientFactory)
        {
            _authContext = appDbContext;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<PrometheusResponse> GetMetricsPrometheusAsync(MachineQueryRequest request)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var prometheusUrl = $"http://{request.Link}:9090/api/v1/query";
                var url = $"{prometheusUrl}?query={request.Query}";
                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var prometheusResponse = JsonConvert.DeserializeObject<PrometheusResponse>(content);
                    return prometheusResponse;
                }
                else
                {
                    return new PrometheusResponse { Status = "Error" };
                }
            }
            catch (Exception ex)
            {
                return new PrometheusResponse { Status = $"Error: {ex.Message}" };
            }
        }


    }
}
