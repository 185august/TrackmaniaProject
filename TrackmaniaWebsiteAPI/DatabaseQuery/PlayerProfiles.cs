using TrackmaniaWebsiteAPI.UserAuth;

namespace TrackmaniaWebsiteAPI.DatabaseQuery;

public class PlayerProfiles
{
    public int? Id { get; set; }
    public string UbisoftUsername { get; set; } = string.Empty;
    public string? UbisoftUserId { get; set; }
}
