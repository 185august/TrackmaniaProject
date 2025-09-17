using TrackmaniaWebsiteAPI.DatabaseQuery;

namespace TrackmaniaWebsiteAPI.CampaignMaps;

public interface IMapInfoService
{
    Task<List<object>> FindMapByYearAndSeason(int year, string season);
    Task GetAllMaps();
}
