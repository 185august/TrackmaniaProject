using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TrackmaniaWebsiteAPI.DatabaseQuery;

namespace TrackmaniaWebsiteAPI.UserAuth;

public class AuthService(TrackmaniaDbContext context) : IAuthService
{
    public async Task<string?> RegisterAsync(UserRegisterDto request)
    {
        if (
            string.IsNullOrWhiteSpace(request.Username)
            || string.IsNullOrWhiteSpace(request.Password)
            || request.PlayerProfile is null
        )
        {
            return null;
        }

        var doesUserNameExist = await DoesUserExist(request.Username);
        if (doesUserNameExist != null || request.PlayerProfile is null)
        {
            return null;
        }

        var user = new User()
        {
            Username = request.Username,
            PlayerProfileId = request.PlayerProfile.Id,
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return "Registration successful";
    }

    public async Task<UserDetailsDto?> LoginAsync(UserLoginDto request)
    {
        var user = await context
            .Users.Include(u => u.PlayerProfile)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

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

        if (user.PlayerProfile is null)
        {
            return null;
        }

        return new UserDetailsDto
        {
            Username = user.Username,
            UbisoftUserId = user.PlayerProfile.UbisoftUserId,
            UbisoftUsername = user.PlayerProfile.UbisoftUsername,
        };
    }

    private async Task<User?> DoesUserExist(string username)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }
}
