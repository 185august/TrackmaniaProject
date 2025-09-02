using TrackmaniaWebsiteAPI.Models;

namespace TrackmaniaWebsiteAPI.Services;

public interface IAuthService
{
    Task<User?> RegisterAsync(UserRegisterDto request);
    Task<User?> LoginAsync(UserLoginDto request);
    Task<User?> RegisterUbisoftName(string ubisoftUsername, User user);
    Task<string> LoginJwtAsync(UserLoginDto request);
}
