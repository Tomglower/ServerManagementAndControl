using ControlPanel.Core;
using ControlPanel.Core.Models;
using ControlPanel.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ControlPanel.UI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PrometheusController : ControllerBase
    {
        private readonly PrometheusExporter _prometheusExporter;
        private readonly ILogger<PrometheusController> _logger;

        public PrometheusController(PrometheusExporter prometheusExporter, ILogger<PrometheusController> logger)
        {
            _prometheusExporter = prometheusExporter;
            _logger = logger;
        }

        [HttpPost]
        [Authorize]
        [Route("GetMetricsPrometheus")]
        public async Task<IActionResult> GetMetricsPrometheus([FromBody] MachineQueryRequest request)
        {
            _logger.LogInformation("Received request to fetch Prometheus metrics for machine: {MachineId}", request.Link);

            try
            {
                var prometheusResponse = await _prometheusExporter.GetMetricsPrometheusAsync(request);
                
                _logger.LogInformation("Successfully fetched Prometheus metrics for machine: {MachineId}",  request.Link);

                return Ok(prometheusResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching Prometheus metrics for machine: {MachineId}",  request.Link);
                
                return StatusCode(500, new { Message = "An error occurred while fetching metrics" });
            }
        }
    }
}