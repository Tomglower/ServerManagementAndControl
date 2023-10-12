using ControlPanel.Core;
using ControlPanel.Core.Models;
using ControlPanel.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ControlPanel.UI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PrometheusController : ControllerBase
    {

        private readonly PrometheusExporter _prometheusExporter;
        public PrometheusController(PrometheusExporter prometheusExporter)
        {

            _prometheusExporter = prometheusExporter;
        }

       
        [HttpPost]
        [Authorize]
        [Route("GetMetricsPrometheus")]
        public async Task<IActionResult> GetMetricsPrometheus([FromBody] MachineQueryRequest request)
        {
            var prometheusResponse = await _prometheusExporter.GetMetricsPrometheusAsync(request);

            return Ok(prometheusResponse);
        }
        
    }

}
