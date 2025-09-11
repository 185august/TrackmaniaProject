using Newtonsoft.Json;

namespace TrackmaniaWebsiteAPI.MapTimes;

public class MapPersonalBestData
{
    public string AccountId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Medal { get; set; }

    [JsonIgnore]
    public string MedalText { get; set; } = string.Empty;

    public RecordScoreNested RecordScore { get; set; } = new();
}

public class RecordScoreNested
{
    public int RespawnCount { get; set; }
    public int Time { get; set; }
    public string FormatedTime { get; set; } = string.Empty;

    [JsonIgnore]
    public int TimeVsWr { get; set; }
}
