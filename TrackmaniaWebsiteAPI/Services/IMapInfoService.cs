namespace TrackmaniaWebsiteAPI.Services;

public interface IMapInfoService
{
    Task AddCampaignsMapsToDb();
    Task<string> RetriveAllMapUids();
}
