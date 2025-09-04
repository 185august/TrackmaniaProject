using TrackmaniaWebsiteAPI.Models;

namespace TrackmaniaWebsiteAPI.Services;

public interface IAuthService
{
    Task<User?> RegisterAsync(UserRegisterDto request);
    Task<User?> LoginAsync(UserLoginDto request);
    Task<string> LoginJwtAsync(UserLoginDto request);
}
