namespace TrackmaniaWebsiteAPI.UserAuth;

public class UserDetailsDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UbisoftUsername { get; set; } = string.Empty;
    public string UbisoftUserId { get; set; } = string.Empty;
}
