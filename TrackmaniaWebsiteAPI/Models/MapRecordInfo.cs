namespace TrackmaniaWebsiteAPI.Models;

public class MapRecordInfo
{
    public string AccountId { get; set; } = string.Empty;
    public int Medal { get; set; }
    public RecordScoreNested? RecordScore { get; set; }
    public DateTime TimeStamp { get; set; }
}

public class RecordScoreNested
{
    public int RespawnCount { get; set; }
    public double Time { get; set; }
}
