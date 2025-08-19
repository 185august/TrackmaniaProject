using TrackmaniaWebsiteAPI.Models;

namespace TrackmaniaWebsiteAPI.Services;

public interface IAuthService
{
    Task<User?> RegisterAsync(UserDto request);
    Task<string?> LoginAync(UserDto request);
}
