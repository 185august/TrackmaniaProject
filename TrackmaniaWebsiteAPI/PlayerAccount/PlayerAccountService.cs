using System.Net.Http.Headers;
using System.Text.Json;
using TrackmaniaWebsiteAPI.Tokens;

public class PlayerAccountService
{
    private readonly IApiTokensService _apiTokensService;

    public PlayerAccountService(IApiTokensService apiTokensService)
    {
        _apiTokensService = apiTokensService;
    }

    public async Task<JsonElement> GetUbisoftAccountIdAsync(string[] accountNames)
    {
        string accessToken = await _apiTokensService.RetrieveTokenAsync(TokenTypes.OAuth2Access);

        string requestNames = string.Join("&", accountNames.Select(n => $"displayName[]={n}"));
        string requestUri =
            $"https://api.trackmania.com/api/display-names/account-ids?{requestNames}";

        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var client = new HttpClient();
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"API call failed with status code {response.StatusCode}"
            );
        }

        string responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(responseBody);
    }
}
