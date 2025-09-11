using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TrackmaniaWebsiteAPI.DatabaseQuery;

namespace TrackmaniaWebsiteAPI.UserAuth;

public class AuthService(TrackmaniaDbContext context) : IAuthService
{
    public async Task<string?> RegisterAsync(UserRegisterDto request)
    {
        var doesUserNameExist = await DoesUserExist(request.Username);
        if (doesUserNameExist != null)
        {
            return null;
        }
        var user = new User();
        string hashedPassword = new PasswordHasher<User>().HashPassword(user, request.Password);
        user.Username = request.Username;
        user.PasswordHash = hashedPassword;
        user.PlayerProfileId = request.PlayerProfile.Id;

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return "success";
    }

    public async Task<UserDetailsDto?> LoginAsync(UserLoginDto request)
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

        user.PlayerProfile = await context.PlayerProfiles.FindAsync(user.PlayerProfileId);
        if (user.PlayerProfile is null)
        {
            return null;
        }
        return new UserDetailsDto
        {
            Id = user.Id,
            Username = user.Username,
            UbisoftUserId = user.PlayerProfile.UbisoftUserId!,
            UbisoftUsername = user.PlayerProfile.UbisoftUsername,
        };
    }

    private async Task<User?> DoesUserExist(string username)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }
}
