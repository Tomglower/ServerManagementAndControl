using ControlPanel.Core;
using ControlPanel.Core.Models;
using ControlPanel.Core.Request;
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


        //[HttpPost]
        //[Authorize]
        //[Route("AddServer")]
        //public async Task<IActionResult> AddServer([FromBody] ServerData data)
        //{
        //    if (await _serverManager.CheckMachinelExist(data.link))
        //    {
        //        return BadRequest(new { Message = "Server already exist" });
        //    }
        //    var result = await _serverManager.AddServer(data);
        //    if (result)
        //    {
        //        return Ok(new { Message = "Server Added!" });
        //    }
        //    else
        //    {
        //        return BadRequest(new { Message = "Is not a Server!" });
        //    }
        //}
        [HttpPost]
        [Authorize]
        [Route("AddServer")]
        public async Task<IActionResult> AddServer([FromBody] AddServerDataRequest request)
        {
            if (await _serverManager.CheckMachinelExist(request.Link))
            {
                return BadRequest(new { Message = "Server already exists" });
            }

            var result = await _serverManager.AddServer(new ServerData
            {
                link = request.Link
            }, request.UserId);; // Передайте userId как второй аргумент

            if (result)
            {
                return Ok(new { Message = "Server Added!" });
            }
            else
            {
                return BadRequest(new { Message = "Is not a Server!" });
            }
        }

        [HttpPost]
        [Authorize]
        [Route("GetServer")]
        public async Task<IActionResult> GetServer([FromBody] GetServerRequest request)
        {
            int userId = request.UserId;

            var server = await _serverManager.GetServers(userId);

            if (server == null)
            {
                return NotFound(new { Message = "Server not found" });
            }

            return Ok(server);
        }

        [HttpPost]
        [Authorize]
        [Route("UpdateServerData")]
        public async Task<IActionResult> UpdateServerData([FromBody] ServerData serverData)
        {
            var result = await _serverManager.UpdateMachineData(serverData);

            if (result.Success)
            {
                return Ok(new { data = result.Data });
            }
            else
            {
                return BadRequest(new { message = result.Message });
            }
        }

        [HttpPost]
        [Authorize]
        [Route("CheckMachine")]
        public async Task<IActionResult> CheckMachine([FromBody] CheckMachineRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Link))
            {
                return BadRequest(new { Message = "Invalid Link" });
            }

            var checkResult = await _serverManager.CheckMachine(request.Link);

            if (checkResult.Exists)
            {
                return Ok(new { Message = checkResult.Message  });
            }
            else
            {
                return Ok(new { Message = checkResult.Message });
            }
        }
        [HttpPost]
        [Authorize]
        [Route("DeleteMachine")]
        public async Task<IActionResult> DeleteMachine(DeleteMachineRequest req)
        {
            try
            {
                var deleteMachine = await _serverManager.DeleteMachine(req.id);
                if (deleteMachine.Exists)
                {
                    return Ok(new { Message = deleteMachine.Message });
                }
                else
                {
                    return BadRequest(new { Message = deleteMachine.Message });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Ошибка: " + ex.Message });
            }
        }




    }

}
