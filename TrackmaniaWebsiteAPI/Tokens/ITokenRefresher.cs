using System.Text.Json;

namespace TrackmaniaWebsiteAPI.Tokens;

public interface ITokenRefresher
{
    Task<JsonElement> RefreshNadeoTokenAsync(string refreshToken);
}
