namespace TrackmaniaWebsiteAPI.UserAuth;

public interface IAuthService
{
    Task<string?> RegisterAsync(UserRegisterDto request);
    Task<UserDetailsDto?> LoginAsync(UserLoginDto request);
}
