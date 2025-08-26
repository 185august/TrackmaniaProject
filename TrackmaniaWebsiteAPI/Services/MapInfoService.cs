using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TrackmaniaWebsiteAPI.Data;
using TrackmaniaWebsiteAPI.Models;

namespace TrackmaniaWebsiteAPI.Services;

public class MapInfoService : IMapInfoService
{
    private readonly TrackmaniaDbContext _context;
    private const string FilePath = "campaignMapsInfo.json";

    public MapInfoService(TrackmaniaDbContext context)
    {
        _context = context;
    }

    public void AddMapsToList(string json)
    {
        using var mapInfo = JsonDocument.Parse(json);
        var result = mapInfo
            .RootElement.GetProperty("campaignList")
            .EnumerateArray()
            .SelectMany(campaign =>
                campaign
                    .GetProperty("playlist")
                    .EnumerateArray()
                    .Select(map => new
                    {
                        name = campaign.GetProperty("name").GetString(),
                        position = map.GetProperty("position").GetInt32(),
                        mapUid = map.GetProperty("mapUid").GetString(),
                    })
            )
            .ToList();

        string jsonString = JsonSerializer.Serialize(
            result,
            new JsonSerializerOptions { WriteIndented = true }
        );
        File.AppendAllText(FilePath, jsonString);
    }

    public async Task AddCampaignsMapsToDb()
    {
        try
        {
            string campaignMaps = await File.ReadAllTextAsync(FilePath);

            var data = JsonSerializer.Deserialize<List<CampaignMapsInfo>>(campaignMaps);
            if (data is not null)
            {
                _context.CampaignMaps.AddRange(data);
                await _context.SaveChangesAsync();
            }
        }
        catch (JsonException e)
        {
            Console.WriteLine($"Critical json exception {e.Message}");
            throw;
        }
    }

    public async Task<string> RetriveAllMapUids()
    {
        var jsonMapUids = await File.ReadAllTextAsync(FilePath);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        var data = JsonSerializer.Deserialize<List<MapUids>>(jsonMapUids, options);
        string mapUids = string.Empty;
        int n = 0;
        foreach (var map in data)
        {
            if (n == 0)
            {
                mapUids += $"{map.MapUid}";
                n++;
            }
            mapUids += $",{map.MapUid}";
        }

        return mapUids;
    }

    public class MapUids()
    {
        public string Name { get; set; }
        public int Position { get; set; }

        [Key]
        public string MapUid { get; set; }
    }
}
