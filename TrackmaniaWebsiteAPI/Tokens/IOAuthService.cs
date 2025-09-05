namespace TrackmaniaWebsiteAPI.Tokens;

public interface IOAuthService
{
    Task<string> GetUbisoftAccountId(string accountName);
}
