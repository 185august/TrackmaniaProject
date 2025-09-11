using System.ComponentModel.DataAnnotations;

namespace TrackmaniaWebsiteAPI.DatabaseQuery;

public class CampaignMapsInfo
{
    [Key]
    [MaxLength(50)]
    public string MapId { get; set; } = string.Empty;

    [MaxLength(50)]
    public string MapUid { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(10)]
    public string Season { get; set; } = string.Empty;
    public int? Year { get; set; }
    public int? Position { get; set; }

    [MaxLength(70)]
    public string ThumbnailUrl { get; set; } = string.Empty;
    public int AuthorScore { get; set; }
    public int BronzeScore { get; set; }
    public int GoldScore { get; set; }
    public int SilverScore { get; set; }
}
