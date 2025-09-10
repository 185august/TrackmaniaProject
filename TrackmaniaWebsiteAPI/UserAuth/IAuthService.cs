namespace TrackmaniaWebsiteAPI.UserAuth;

public interface IAuthService
{
    Task<UserDetailsDto?> RegisterAsync(UserRegisterDto request);
    Task<UserDetailsDto?> LoginAsync(UserLoginDto request);
    Task<string> LoginJwtAsync(UserLoginDto request);
}
