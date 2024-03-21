using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ControlPanel.Data;
using Microsoft.EntityFrameworkCore;
using ControlPanel.Data.Models;
using ControlPanel.Core.Result;
using ControlPanel.Core.Models;

namespace ControlPanel.Core;

public class AuthManager
{

    private readonly AppDbContext _authContext;

    public AuthManager(AppDbContext appDbContext)
    {
        _authContext = appDbContext;

    }


    public async Task<AuthResult> Authenticate(User user)
    {
        var result = new AuthResult();

        if (user is null)
        {
            result.Res = false;
        }

        var CheckedUser = await _authContext.Users.FirstOrDefaultAsync(Users => Users.UserName == user.UserName);

        if (CheckedUser is null)
        {
            result.Res = false;
        }

        if (PasswordHasher.VerifyPassword(user.Password, CheckedUser.Password))
        {
            user.Token = CreateJwtToken(CheckedUser);
            CheckedUser.Token = user.Token;
            _authContext.Entry(CheckedUser).State = EntityState.Modified;
            await _authContext.SaveChangesAsync();
            result.Res = true;
            result.Id = CheckedUser.id;
        }

        return result;
    }



    private string CreateJwtToken(User user)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("veryverysecret.....");
        var identify = new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.Role,user.Role),
                new Claim(ClaimTypes.Name,$"{user.UserName}")
            });

        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = identify,
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = credentials
        };
        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        return jwtTokenHandler.WriteToken(token);
    }

    public async Task<bool> Registration(User user)
    {
        if (user is null) 
        { 
            return false; 
        }

        if (string.IsNullOrEmpty(user.Role)) 
        { 
            user.Role = "User";
        }

        user.Password = PasswordHasher.HashPassword(user.Password);

        await _authContext.Users.AddAsync(user);
        await _authContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CheckUserNameExist(string username)
    {
        return await _authContext.Users.AnyAsync(Users => Users.UserName == username);
    }


    public async Task<bool> CheckEmailExist(string email)
    {
        return await _authContext.Users.AnyAsync(Users => Users.Email == email);
    }

    public bool ValidateToken(string authToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = GetValidationParameters();

        try
        {
            SecurityToken validatedToken;
            tokenHandler.ValidateToken(authToken, validationParameters, out validatedToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private TokenValidationParameters GetValidationParameters()
    {
        return new TokenValidationParameters()
        {
            ValidateLifetime = true,
            ValidateAudience = false,
            ValidateIssuer = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("veryverysecret....."))
        };
    }
    private string CreateSimpleJwtToken()
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("veryverysecret....."); 

        var identity = new ClaimsIdentity();

        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = identity,
            Expires = DateTime.UtcNow.AddDays(7), 
            SigningCredentials = credentials
        };
        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        return jwtTokenHandler.WriteToken(token);
    }

}
