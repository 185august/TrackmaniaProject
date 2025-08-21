using System.Text.Json;

namespace TrackmaniaWebsiteAPI.Services;

public interface INadeoTokenService
{
    Task<JsonElement> RequestNadeoTokenAsync(string ticket, string nadeoAudience);
    Task<JsonElement> RefreshNadeoTokenAsync(string refreshToken);
}
