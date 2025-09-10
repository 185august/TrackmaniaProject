using TrackmaniaWebsiteAPI.DatabaseQuery;

namespace TrackmaniaWebsiteAPI.MapInfo;

public interface IMapInfoService
{
    Task AddCampaignsMapsToDb();
    Task<string> RetriveAllMapUids();
    Task<List<CampaignMapsInfo>> FindMapByYear(int year);
    Task<List<CampaignMapsInfo>> FindMapByYearAndSeason(int year, string season);
    Task<List<CampaignMapsInfo>> FindMapBySeason(string season);
}
