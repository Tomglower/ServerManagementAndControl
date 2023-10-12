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
    public class ServerController : ControllerBase
    {
        private readonly ServerManager _serverManager;
        public ServerController(ServerManager serverManager)
        {
            _serverManager = serverManager;
        }


        [HttpPost]
        [Authorize]
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
        [Authorize]
        [Route("GetServers")]
        public async Task<IActionResult> GetAllServers()
        {
            var machines = await _serverManager.GetServers();
            return Ok(machines); 
        }

        [HttpPost]
        [Authorize]
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

        
    }

}
