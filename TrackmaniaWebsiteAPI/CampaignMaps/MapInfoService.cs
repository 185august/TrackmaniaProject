using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TrackmaniaWebsiteAPI.DatabaseQuery;

namespace TrackmaniaWebsiteAPI.CampaignMaps;

public class MapInfoService(TrackmaniaDbContext context) : IMapInfoService
{
    public async Task<List<object>> FindMapByYearAndSeason(int year, string season)
    {
        return await context
            .CampaignMaps.AsNoTracking()
            .Where(m => m.Year == year && m.Season == season)
            .OrderBy(m => m.Position)
            .Select(m => new
            {
                m.MapId,
                m.MapUid,
                m.Name,
            })
            .ToListAsync<object>();
    }

    public async Task GetAllMaps()
    {
        try
        {
            var maps = await context.CampaignMaps.ToListAsync();
            string mapsJsonContent = JsonSerializer.Serialize(
                maps,
                new JsonSerializerOptions { WriteIndented = true }
            );
            await File.WriteAllTextAsync("CampaignMaps.json", mapsJsonContent);
        }
        catch (JsonException e)
        {
            Console.WriteLine(e);
        }
    }
}
