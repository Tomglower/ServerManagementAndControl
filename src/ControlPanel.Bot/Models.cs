using Newtonsoft.Json;

namespace TelegramWorker;

public class PrometheusApiResponse
{
    public string Status { get; set; }
    public PrometheusData Data { get; set; }
}

public class PrometheusData
{
    public string ResultType { get; set; }
    public List<PrometheusResult> Result { get; set; }
}

public class PrometheusResult
{
    public Dictionary<string, string> Metric { get; set; }
    public object[] Value { get; set; }
}
public class Server
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("link")]
    public string Link { get; set; }

    [JsonProperty("data")] public object Data { get; set; }

    [JsonProperty("userId")]
    public int UserId { get; set; }
    
}
public class ApiResponse
{
    public string Status { get; set; }
    public ApiResponseData Data { get; set; }
}

public class ApiResponseData
{
    public string ResultType { get; set; }
    public List<ApiResult> Result { get; set; }
}

public class ApiResult
{
    public ApiMetric Metric { get; set; }
    // Обратите внимание, что мы ожидаем, что 'Value' будет содержать список элементов типа 'double'.
    public List<double> Value { get; set; }
}

public class ApiMetric
{
    public string Name { get; set; }
    public string Instance { get; set; }
    public string Job { get; set; }
}