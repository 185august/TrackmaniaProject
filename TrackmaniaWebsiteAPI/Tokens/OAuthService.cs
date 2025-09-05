using System.Net.Http.Headers;
using System.Text.Json;
using TrackmaniaWebsiteAPI.Models;

namespace TrackmaniaWebsiteAPI.Tokens;

public class OAuthService(IApiTokensService apiTokensService) : IOAuthService
{
    public async Task<string> GetUbisoftAccountId(string accountName)
    {
        var accessToken = await apiTokensService.RetrieveTokenAsync(TokenTypes.OAuth2Access);
        var requestUri =
            $"https://api.trackmania.com/api/display-names/account-ids?displayName[]={accountName}";

        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", $"{accessToken}");

        using var client = new HttpClient();
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return $"{response.ReasonPhrase}";
        }
        var responseBody = await response.Content.ReadAsStringAsync();
        var obj = JsonSerializer.Deserialize<JsonElement>(responseBody);
        var id = obj.GetProperty($"{accountName}");
        return id.ToString();
    }
}
