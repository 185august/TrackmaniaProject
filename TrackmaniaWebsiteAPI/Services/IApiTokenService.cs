using TrackmaniaWebsiteAPI.Models;

namespace TrackmaniaWebsiteAPI.Services;

public interface IApiTokensService
{
    string? GetUbisoftTicket();

    string? GetToken(TokenTypes tokenType);
    void UpdateUbisoftTicket(string ubisoftTicket);
    void UpdateToken(TokenTypes tokenType, string newToken);
}
