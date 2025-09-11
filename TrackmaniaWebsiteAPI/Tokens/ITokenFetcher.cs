using System.Text.Json;

namespace TrackmaniaWebsiteAPI.Tokens;

public interface ITokenFetcher
{
    Task<string> RetrieveAccessTokenAsync(TokenTypes tokenType);
}
