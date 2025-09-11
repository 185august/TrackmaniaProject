using System.ComponentModel.DataAnnotations;

namespace TrackmaniaWebsiteAPI.DatabaseQuery;

public class PlayerProfiles
{
    public int? Id { get; set; }

    [MaxLength(50)]
    public string UbisoftUsername { get; set; } = string.Empty;

    [MaxLength(50)]
    public string UbisoftUserId { get; set; } = string.Empty;
}
