using System.ComponentModel.DataAnnotations;

namespace TrackmaniaWebsiteAPI.Models;

public class CampaignMapInfo
{
    [Key]
    public int MapUid { get; set; }
    public int MapId { get; set; }
    public int Position { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
}
