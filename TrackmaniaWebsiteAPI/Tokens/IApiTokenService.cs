using System.Text.Json;
using TrackmaniaWebsiteAPI.Models;

namespace TrackmaniaWebsiteAPI.Tokens;

public interface IApiTokensService
{
    string? GetUbisoftTicket();
    Task<JsonElement> RequestNadeoTokenAsync(string nadeoAudience);
    Task<JsonElement> RefreshNadeoTokenAsync(string refreshToken);

    Task<string> RetrieveTokenAsync(TokenTypes tokenType);
    void UpdateUbisoftTicket(string ubisoftTicket);
    void UpdateToken(TokenTypes tokenType, string newToken);
}
