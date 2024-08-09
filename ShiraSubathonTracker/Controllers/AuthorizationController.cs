using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShiraSubathonTracker.DAL;
using ShiraSubathonTracker.DAL.Entities.Users;

namespace ShiraSubathonTracker.Controllers;


[ApiController]
[Route("auth")]
public class AuthorizationController(TrackerDatabaseContext context): ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginModel loginModel)
    {
        // Validate user credentials with your database
        var user = await context.Users
            .SingleOrDefaultAsync(u => u.Username == loginModel.Username);

        if (user == null)
            return Unauthorized();
        if (!VerifyPassword(loginModel, user.Password))
            return Unauthorized();

        // Create JWT token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(user.SecretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, "ApiAccess")
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        await UpsetToken(tokenString, user.Username);

        // Return the token
        return Ok(new { Token = tokenString });
    }

    private async Task UpsetToken(string token, string username)
    {
        var jwtToken = await context.JwtTokens.SingleOrDefaultAsync(x => x.Username == username);

        if (jwtToken == null)
        {
            jwtToken ??= new JwtToken
            {
                Username = username,
                Token = token,
                IsBlocked = false
            };

            context.JwtTokens.Add(jwtToken);
        }
        else
        {
            jwtToken.Token = token;
        }

        await context.SaveChangesAsync();
    }
    
    // For local use only
    // [HttpPost("register")]
    // public async Task<IActionResult> Register([FromBody] UserLoginModel loginModel, CancellationToken cancellationToken)
    // {
    //     var secret = Guid.NewGuid();
    //     var passwordHasher = new PasswordHasher<UserLoginModel>();
    //
    //     var newUser = new User
    //     {
    //         Username = loginModel.Username,
    //         Password = passwordHasher.HashPassword(loginModel, loginModel.Password),
    //         SecretKey = secret.ToString()
    //     };
    //
    //     context.Users.Add(newUser);
    //     await context.SaveChangesAsync(cancellationToken);
    //     return Ok();
    // }

    private static bool VerifyPassword(UserLoginModel loginModel, string password)
    {
        var passwordHasher = new PasswordHasher<UserLoginModel>();
        var verificationResult = passwordHasher.VerifyHashedPassword(loginModel, password, loginModel.Password);
        return verificationResult == PasswordVerificationResult.Success;
    }
}

public class UserLoginModel
{
    public required string Username { get; set; } 
    public required string Password { get; set; } 
}