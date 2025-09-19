using System.ComponentModel.DataAnnotations;

namespace TrackmaniaWebsiteAPI.DatabaseQuery;

public class User
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(100)]
    public string PasswordHash { get; set; } = string.Empty;
    public int? PlayerProfileId { get; set; }
    public PlayerProfiles? PlayerProfile { get; set; }
}
