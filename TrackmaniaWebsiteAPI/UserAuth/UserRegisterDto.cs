using TrackmaniaWebsiteAPI.PlayerAccount;

namespace TrackmaniaWebsiteAPI.UserAuth;

public class UserRegisterDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public PlayerProfileDto? PlayerProfile { get; set; }
}
