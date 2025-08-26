using System.ComponentModel.DataAnnotations;

namespace TrackmaniaWebsiteAPI.Models;

public class CampaignMapsInfo
{
    [Key]
    public string MapId { get; set; }
    public string MapUid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Position { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public int AuthorScore { get; set; }
    public int BronzeScore { get; set; }
    public int GoldScore { get; set; }
    public int SilverScore { get; set; }
}
