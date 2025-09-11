using TrackmaniaWebsiteAPI.DatabaseQuery;

namespace TrackmaniaWebsiteAPI.CampaignMaps;

public interface IMapInfoService
{
    Task<List<CampaignMapsInfo>> FindMapByYearAndSeason(int year, string season);
}
