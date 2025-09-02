using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TrackmaniaWebsiteAPI.Data;
using TrackmaniaWebsiteAPI.Models;
using TrackmaniaWebsiteAPI.Models;

namespace TrackmaniaWebsiteAPI.Services;

public class AuthService(TrackmaniaDbContext context, IConfiguration configuration, IOAuthService oAuthService, JwtHelperService jwtHelper) : IAuthService
{
    public async Task<User?> RegisterAsync(UserRegisterDto request)
    {
        if (await DoesUserExist(request.Username) != null)
        {
            return null;
        }

        try
        {
            var user = new User();
            string hashedPassword = new PasswordHasher<User>().HashPassword(user, request.Password);
            user.Username = request.Username;
            user.PasswordHash = hashedPassword;
            user.UbisoftUserId = request.UbisoftUserId;
            user.UbisoftUsername = request.UbisoftUsername;
            
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public async Task<User?> RegisterUbisoftName(string ubisoftUsername, User user)
    {
        try
        {
            user.UbisoftUserId =
                    await oAuthService.GetUbisoftAccountId(ubisoftUsername);
            user.UbisoftUsername = ubisoftUsername;
            await context.SaveChangesAsync();
            return user;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public async Task<User?> LoginAsync(UserLoginDto request)
    {
        var user = await DoesUserExist(request.Username);
        if (user is null)
        {
            return null;
        }

        
        if (
            new PasswordHasher<User>().VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password
            ) == PasswordVerificationResult.Failed
        )
            return null;
        
        return user;
    }
    
    public async Task<string> LoginJwtAsync(UserLoginDto request)
    {
        var user = await DoesUserExist(request.Username);
        if (user is null || new PasswordHasher<User>().VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    request.Password) == PasswordVerificationResult.Failed
)
        {
            throw new Exception("User login failed");
        }

        
        string token = jwtHelper.CreateToken(user);
        return token;
    }

    private async Task<User?> DoesUserExist(string username)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }
}
