using ControlPanel.Core;
using ControlPanel.Core.Models;
using ControlPanel.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ControlPanel.UI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthManager _authManager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthManager authManager, ILogger<AuthController> logger)
        {
            _authManager = authManager;
            _logger = logger;
        }

        [HttpPost]
        [Route("auth")]
        public async Task<IActionResult> Authentication([FromBody] User user)
        {
            _logger.LogInformation("Starting authentication for user: {UserName}", user.UserName);
            
            try
            {
                var result = await _authManager.Authenticate(user);
                if (result.Res)
                {
                    _logger.LogInformation("Authentication successful for user: {UserName}", user.UserName);

                    return Ok(new
                    {
                        id = result.Id,
                        Token = user.Token,
                        Message = "Login Success!"
                    });
                }
                else
                {
                    _logger.LogWarning("Authentication failed for user: {UserName}", user.UserName);

                    return BadRequest(new { Message = "Unlucky;(" });
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred during authentication for user: {UserName}", user.UserName);

                return StatusCode(500, new { Message = "Database error: " + ex.Message });
            }
        }

        [HttpPost]
        [Route("registration")]
        public async Task<IActionResult> Registration([FromBody] User user)
        {
            _logger.LogInformation("Starting registration for user: {UserName}", user.UserName);

            try
            {
                if (await _authManager.CheckEmailExist(user.Email))
                {
                    _logger.LogWarning("Email already exists: {Email}", user.Email);

                    return BadRequest(new { Message = "Email already exists" });
                }

                if (await _authManager.CheckUserNameExist(user.UserName))
                {
                    _logger.LogWarning("Username already exists: {UserName}", user.UserName);

                    return BadRequest(new { Message = "Username already exists" });
                }

                var result = await _authManager.Registration(user);
                if (result)
                {
                    _logger.LogInformation("Registration successful for user: {UserName}", user.UserName);

                    return Ok(new { Message = "User registered!" });
                }
                else
                {
                    _logger.LogWarning("Registration failed for user: {UserName}", user.UserName);

                    return BadRequest(new { Message = "Unlucky;(" });
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred during registration for user: {UserName}", user.UserName);

                return StatusCode(500, new { Message = "Database error: " + ex.Message });
            }
        }

        [HttpPost]
        [Route("authorize")]
        public IActionResult Authorize([FromBody] string authToken)
        {
            _logger.LogInformation("Starting authorization check with token.");

            if (_authManager.ValidateToken(authToken))
            {
                _logger.LogInformation("Authorization successful for token.");

                return Ok(new { Message = "Token is valid" });
            }
            else
            {
                _logger.LogWarning("Authorization failed for token.");

                return Unauthorized(new { Message = "Token is not valid" });
            }
        }
    }
}
