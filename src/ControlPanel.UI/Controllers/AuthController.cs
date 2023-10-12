using ControlPanel.Core;
using ControlPanel.Core.Models;
using ControlPanel.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ControlPanel.UI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthManager _authController; 
        public AuthController(AuthManager userController)
        {
            _authController = userController;
        }

        [HttpPost]
        [Route("auth")]
        public async Task<IActionResult> Authentication([FromBody] User user)
        {
            try
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
                    return BadRequest(new { Message = "Unluck;(" });
                }
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { Message = "Database error: " + ex.Message });
            }
        }


        [HttpPost]
        [Route("registration")]
        public async Task<IActionResult> Registration([FromBody] User user)
        {
            try
            {
                if (await _authController.CheckEmailExist(user.Email))
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
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { Message = "Database error: " + ex.Message });
            }
        }


        public IActionResult Authorize()
        {

            return Ok(new { Message = "Token is valid" });

        }
        [HttpPost]
        [Route("authorize")]
        public IActionResult Authorize([FromBody] string authToken)
        {
            if ( _authController.ValidateToken(authToken))
            {
                return Ok(new { Message = "Token is valid" });
            }
            else
            {
                return Unauthorized(new { Message = "Token is not valid" });
            }
        }

       


    }

}
