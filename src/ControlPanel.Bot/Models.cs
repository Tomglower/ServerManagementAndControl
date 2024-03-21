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