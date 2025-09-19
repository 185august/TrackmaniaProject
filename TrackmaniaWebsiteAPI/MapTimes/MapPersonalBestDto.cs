using Newtonsoft.Json;

namespace TrackmaniaWebsiteAPI.MapTimes;

public class MapPersonalBestDto
{
    public string AccountId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Medal { get; set; }

    [JsonIgnore]
    public string MedalText { get; set; } = string.Empty;

    public RecordScoreNested RecordScore { get; set; } = new();

    public MapPersonalBestDto() { }

    public MapPersonalBestDto(
        string accountId,
        string name,
        int medal,
        string medalText,
        RecordScoreNested recordScore
    )
    {
        AccountId = accountId;
        Name = name;
        Medal = medal;
        MedalText = medalText;
        RecordScore = recordScore;
    }
}

public class RecordScoreNested
{
    public int Time { get; set; }
    public string FormatedTime { get; set; } = string.Empty;

    [JsonIgnore]
    public int TimeVsWr { get; set; }

    public RecordScoreNested() { }

    public RecordScoreNested(int time, string formatedTime, int timeVsWr = 0)
    {
        Time = time;
        FormatedTime = formatedTime;
        TimeVsWr = timeVsWr;
    }
}
