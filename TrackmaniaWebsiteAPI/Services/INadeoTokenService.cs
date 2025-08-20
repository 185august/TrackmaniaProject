using System.Text.Json;

namespace TrackmaniaWebsiteAPI.Services;

public interface INadeoTokenService
{
    Task<JsonElement> RequestUbisoftTicketAsync(string ticket, string jsonBody);
}
