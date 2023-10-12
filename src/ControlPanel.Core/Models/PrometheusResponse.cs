using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlPanel.Core.Models
{
    public class PrometheusResponse
    {
        public string Status { get; set; }
        public Data Data { get; set; }
    }

    public class Data
    {
        public string ResultType { get; set; }
        public List<ResultItem> Result { get; set; }
    }

    public class ResultItem
    {
        public Metric Metric { get; set; }
        public List<double> Value { get; set; }
    }

    public class Metric
    {
        public string __name__ { get; set; }
        public string instance { get; set; }
        public string job { get; set; }
    }
}
