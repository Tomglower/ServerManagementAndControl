using ControlPanel.Core;
using ControlPanel.Core.Models;
using ControlPanel.Data.Models;
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
    [Route("api/[controller]")]
    [ApiController]
    public class UIController : ControllerBase
    {
        private readonly AuthManager _authController; 
        private readonly ServerManager _serverManager;
        private readonly PrometheusExporter _prometheusExporter;
        public UIController(AuthManager userController, ServerManager serverManager, PrometheusExporter prometheusExporter)
        {
            _authController = userController;
            _serverManager = serverManager;
            _prometheusExporter = prometheusExporter;
        }

        [HttpPost]
        [Route("auth")]
        public async Task<IActionResult> Authentication([FromBody] User user)
        {
            var result = await _authController.Authenticate(user);
            if (result)
            {
                Console.WriteLine("Controller" + user.Token.ToString());

                return Ok(new
                {
                    Token = user.Token,
                    Message = "Login Success!"
                });
            }
            else
            {
                return BadRequest(new { Message = "Unluck;("});
            }
        }

       
        [HttpPost]
        [Route("registration")]
        public async Task<IActionResult> Registration([FromBody] User user)
        {
            if(await _authController.CheckEmailExist(user.Email))
            {
                return BadRequest(new { Message = "Email already exist" });
            }
            if (await _authController.CheckUserNameExist(user.UserName))
            {
                return BadRequest(new { Message = "Username already exist" });
            }
            var result = await _authController.Registration(user);
            if (result) 
            {
                return Ok(new { Message = "User registered!" });
            }
            else
            {
                return BadRequest(new { Message = "Unluck;(" });
            }
        }

        [HttpPost]
        [Route("AddServer")]
        public async Task<IActionResult> AddServer([FromBody] ServerData data)
        {
            if (await _serverManager.CheckMachinelExist(data.link))
            {
                return BadRequest(new { Message = "Server already exist" });
            }
            var result = await _serverManager.AddServer(data);
            if(result)
            {
                return Ok(new { Message = "Server Added!" });
            }
            else
            {
                return BadRequest(new { Message = "Is not a Server!" });
            }
        }
        [HttpGet]
        [Route("GetServers")]
        public async Task<IActionResult> GetAllServers()
        {
            var machines = await _serverManager.GetServers();
            return Ok(machines); 
        }

        [HttpPost]
        [Route("UpdateServerData")]
        public async Task<IActionResult> UpdateServerData([FromBody] ServerData serverData)
        {
            var result = await _serverManager.UpdateMachineData(serverData);

            if (result.Success)
            {
                return Ok(new {data = result.Data });
            }
            else
            {
                return BadRequest(new { message = result.Message });
            }
        }
        [HttpPost]
        [Route("GetMetricsPrometheus")]
        public async Task<IActionResult> GetMetricsPrometheus([FromBody] MachineQueryRequest request)
        {
            var prometheusResponse = await _prometheusExporter.GetMetricsPrometheusAsync(request);

            return Ok(prometheusResponse);
        }
        
    }

}
