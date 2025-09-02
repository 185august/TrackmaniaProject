namespace TrackmaniaWebsiteAPI.Services;

public interface IOAuthService
{
    Task<string> GetUbisoftAccountId(string accountName);
}