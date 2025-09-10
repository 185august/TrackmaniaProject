using System.Net.Http.Headers;
using System.Text.Json;
using TrackmaniaWebsiteAPI.DatabaseQuery;
using TrackmaniaWebsiteAPI.RequestQueue;
using TrackmaniaWebsiteAPI.Tokens;

namespace TrackmaniaWebsiteAPI.PlayerAccount;

public class PlayerAccountService(
    ApiTokensServiceRefactor apiTokensService,
    IApiRequestQueue requestQueue
)
{
    public async Task<List<PlayerProfiles>> GetUbisoftAccountIdAsync(string[] accountNames)
    {
        var accessToken = await apiTokensService.RetrieveAccessTokenAsync(TokenTypesNew.OAuth);

        string requestNames = string.Join(
            "&",
            accountNames.Select(n => $"displayName[]={n.ToLower()}")
        );
        string requestUri =
            $"https://api.trackmania.com/api/display-names/account-ids?{requestNames}";

        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await requestQueue.QueueRequest(client => client.SendAsync(request));

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"API call failed with status code {response.StatusCode}"
            );
        }

        string responseBody = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(responseBody);

        if (json.ValueKind == JsonValueKind.Array && json.GetArrayLength() == 0)
        {
            Console.WriteLine($"No users found for: {string.Join(", ", accountNames)}");
            return new List<PlayerProfiles>();
        }

        var listOfPlayerAccounts = new List<PlayerProfiles>();
        foreach (var user in accountNames)
        {
            listOfPlayerAccounts.Add(
                new PlayerProfiles
                {
                    UbisoftUsername = user.ToLower(),
                    UbisoftUserId = json.GetProperty(user.ToLower()).ToString(),
                }
            );
        }

        return listOfPlayerAccounts;
    }
}
