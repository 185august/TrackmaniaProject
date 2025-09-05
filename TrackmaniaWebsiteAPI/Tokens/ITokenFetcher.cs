using System.Text.Json;

namespace TrackmaniaWebsiteAPI.Tokens;

public interface ITokenFetcher
{
    Task<string> RequestTicket();
    Task<JsonElement> RequestNadeoTokenAsync(string nadeoAudience);
}
