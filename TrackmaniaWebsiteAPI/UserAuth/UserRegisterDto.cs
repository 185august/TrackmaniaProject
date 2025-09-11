using TrackmaniaWebsiteAPI.DatabaseQuery;

namespace TrackmaniaWebsiteAPI.UserAuth;

public class UserRegisterDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public PlayerProfiles? PlayerProfile { get; set; }
}
