using System.Text.Json;

namespace TrackmaniaWebsiteAPI.Tokens;

public interface ITokenRefresher
{
    Task<TokenData> RefreshNadeoTokenAsync(string refreshToken);
}
