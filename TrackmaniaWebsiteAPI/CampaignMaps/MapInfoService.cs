using Microsoft.EntityFrameworkCore;
using TrackmaniaWebsiteAPI.DatabaseQuery;

namespace TrackmaniaWebsiteAPI.CampaignMaps;

public class MapInfoService(TrackmaniaDbContext context) : IMapInfoService
{
    public async Task<List<CampaignMapsInfo>> FindMapByYearAndSeason(int year, string season)
    {
        return await context
            .CampaignMaps.AsNoTracking()
            .Where(m => m.Year == year && m.Season == season)
            .OrderBy(m => m.Position)
            .ToListAsync();
    }
}
