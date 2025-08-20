using System.Text.Json;

namespace TrackmaniaWebsiteAPI.Services;

public class MapService
{
    public void AddMapToList(string json)
    {
        using var mapInfo = JsonDocument.Parse(json);
        var result = mapInfo
            .RootElement.GetProperty("mapList")
            .EnumerateArray()
            .Select(e => new
            {
                MapId = e.GetProperty("mapId").GetString(),
                ThumbnailUrl = e.GetProperty("thumbnailUrl").GetString(),
            })
            .ToList();
        foreach (var map in result)
        {
            Console.WriteLine($"{map.MapId} \n {map.ThumbnailUrl}");
        }
    }
}
