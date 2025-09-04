namespace TrackmaniaWebsiteAPI.MapTimes;

public class MapWrInfo
{
    public List<TopsItem> Tops { get; set; } = [];
}

public class TopsItem
{
    public List<TopItem> Top { get; set; } = [];
}

public class TopItem
{
    public string AccountId { get; set; } = string.Empty;
    public int Score { get; set; }
}
