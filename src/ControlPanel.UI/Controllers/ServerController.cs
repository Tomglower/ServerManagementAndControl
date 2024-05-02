using ControlPanel.Core;
using ControlPanel.Core.Models;
using ControlPanel.Core.Request;
using ControlPanel.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace ControlPanel.UI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ServerController : ControllerBase
    {
        private readonly ServerManager _serverManager;
        private readonly ILogger<ServerController> _logger;

        public ServerController(ServerManager serverManager, ILogger<ServerController> logger)
        {
            _serverManager = serverManager;
            _logger = logger;
        }

        [HttpPost]
        [Authorize]
        [Route("AddServer")]
        public async Task<IActionResult> AddServer([FromBody] AddServerDataRequest request)
        {
            _logger.LogInformation("AddServer endpoint called for link: {Link}", request.Link);

            if (await _serverManager.CheckMachinelExist(request.Link))
            {
                _logger.LogWarning("Server already exists for link: {Link}", request.Link);
                return BadRequest(new { Message = "Server already exists" });
            }

            var result = await _serverManager.AddServer(new ServerData { link = request.Link }, request.UserId);

            if (result)
            {
                _logger.LogInformation("Server successfully added for link: {Link}", request.Link);
                return Ok(new { Message = "Server Added!" });
            }
            else
            {
                _logger.LogWarning("Failed to add server for link: {Link}", request.Link);
                return BadRequest(new { Message = "Is not a Server!" });
            }
        }

        [HttpPost]
        [Authorize]
        [Route("GetServer")]
        public async Task<IActionResult> GetServer([FromBody] GetServerRequest request)
        {
            _logger.LogInformation("GetServer endpoint called for User ID: {UserId}", request.UserId);

            var server = await _serverManager.GetServers(request.UserId);

            if (server == null)
            {
                _logger.LogWarning("No servers found for User ID: {UserId}", request.UserId);
                return NotFound(new { Message = "Server not found" });
            }

            _logger.LogInformation("Server data retrieved for User ID: {UserId}", request.UserId);
            return Ok(server);
        }

        [HttpPost]
        [Authorize]
        [Route("UpdateServerData")]
        public async Task<IActionResult> UpdateServerData([FromBody] ServerData serverData)
        {
            _logger.LogInformation("UpdateServerData endpoint called for link: {Link}", serverData.link);

            var result = await _serverManager.UpdateMachineData(serverData);

            if (result.Success)
            {
                _logger.LogInformation("Server data successfully updated for link: {Link}", serverData.link);
                return Ok(new { data = result.Data });
            }
            else
            {
                _logger.LogWarning("Failed to update server data for link: {Link}", serverData.link);
                return BadRequest(new { message = result.Message });
            }
        }

        [HttpPost]
        [Authorize]
        [Route("CheckMachine")]
        public async Task<IActionResult> CheckMachine([FromBody] CheckMachineRequest request)
        {
            _logger.LogInformation("CheckMachine endpoint called for link: {Link}", request.Link);

            if (string.IsNullOrWhiteSpace(request.Link))
            {
                _logger.LogWarning("Invalid link provided: {Link}", request.Link);
                return BadRequest(new { Message = "Invalid Link" });
            }

            var checkResult = await _serverManager.CheckMachine(request.Link);

            if (checkResult.Exists)
            {
                _logger.LogInformation("Machine exists for link: {Link}", request.Link);
                return Ok(new { Message = checkResult.Message });
            }
            else
            {
                _logger.LogWarning("Machine does not exist for link: {Link}", request.Link);
                return Ok(new { Message = checkResult.Message });
            }
        }

        [HttpPost]
        [Authorize]
        [Route("DeleteMachine")]
        public async Task<IActionResult> DeleteMachine([FromBody] DeleteMachineRequest request)
        {
            _logger.LogInformation("DeleteMachine endpoint called for ID: {Id}", request.id);

            try
            {
                var deleteMachine = await _serverManager.DeleteMachine(request.id);
                if (deleteMachine.Exists)
                {
                    _logger.LogInformation("Machine successfully deleted for ID: {Id}", request.id);
                    return Ok(new { Message = deleteMachine.Message });
                }
                else
                {
                    _logger.LogWarning("Failed to delete machine for ID: {Id}", request.id);
                    return BadRequest(new { Message = deleteMachine.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the machine with ID: {Id}", request.id);
                return BadRequest(new { Message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        [Route("GetStatus")]
        public async Task<bool> GetStatus(CheckMachineRequest req)
        {
            _logger.LogInformation("GetStatus endpoint called for link: {Link}", req.Link);

            using (Ping ping = new Ping())
            {
                try
                {
                    PingReply reply = await ping.SendPingAsync(req.Link, 1000);
                    bool status = reply.Status == IPStatus.Success;

                    if (status)
                    {
                        _logger.LogInformation("Ping successful for link: {Link}", req.Link);
                    }
                    else
                    {
                        _logger.LogWarning("Ping failed for link: {Link}", req.Link);
                    }

                    return status;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while pinging link: {Link}", req.Link);
                    return false;
                }
            }
        }
    }
}
