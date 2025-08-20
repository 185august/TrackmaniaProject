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

public class AuthService(TrackmaniaDbContext context, IConfiguration configuration) : IAuthService
{
    public async Task<User?> RegisterAsync(UserDto request)
    {
        if (await DoesUserExist(request) != null)
        {
            return null;
        }

        var user = new User();
        var hashedPassword = new PasswordHasher<User>().HashPassword(user, request.Password);
        user.Username = request.Username;
        user.PasswordHash = hashedPassword;

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<string?> LoginAync(UserDto request)
    {
        var user = await DoesUserExist(request);
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
        {
            return null;
        }

        return "Success";
    }

    private async Task<User?> DoesUserExist(UserDto request)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
    }
}
