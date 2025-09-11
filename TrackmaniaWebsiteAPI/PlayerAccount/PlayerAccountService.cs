using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TrackmaniaWebsiteAPI.DatabaseQuery;
using TrackmaniaWebsiteAPI.RequestQueue;
using TrackmaniaWebsiteAPI.Tokens;

namespace TrackmaniaWebsiteAPI.PlayerAccount;

public class PlayerAccountService(
    ITokenFetcher apiTokensService,
    IApiRequestQueue requestQueue,
    TrackmaniaDbContext context
)
{
    public async Task<List<PlayerProfiles>> GetUbisoftAccountIdAsync(string[] accountNames)
    {
        var accessToken = await apiTokensService.RetrieveAccessTokenAsync(TokenTypes.OAuth);

        string requestNames = string.Join(
            "&",
            accountNames.Select(n => $"displayName[]={n.ToLower()}")
        );
        string requestUri =
            $"https://api.trackmania.com/api/display-names/account-ids?{requestNames}";

        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await requestQueue.QueueRequest(client => client.SendAsync(request));

        response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(responseBody);

        if (json.ValueKind == JsonValueKind.Array && json.GetArrayLength() == 0)
        {
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

    public async Task<List<PlayerProfiles>> GetAndUpdatePlayerAccountsAsync(string playerNames)
    {
        var listOfPlayers = playerNames.ToLower().Split(',').ToList();
        var existingPlayers = await context
            .PlayerProfiles.AsNoTracking()
            .Where(p => listOfPlayers.Contains(p.UbisoftUsername.ToLower()))
            .ToListAsync();
        var existingUsernames = existingPlayers
            .Select(profiles => profiles.UbisoftUsername.Trim())
            .ToHashSet();

        string[] missingUsernames = listOfPlayers.Except(existingUsernames).ToArray();
        if (missingUsernames.Length == 0)
        {
            return existingPlayers;
        }

        var getMissingUserNames = await AddNewPlayerProfilesToDbAsync(missingUsernames);

        existingPlayers.AddRange(getMissingUserNames);

        return existingPlayers;
    }

    private async Task<List<PlayerProfiles>> AddNewPlayerProfilesToDbAsync(
        string[] missingUsernames
    )
    {
        var getMissingUserNames = await GetUbisoftAccountIdAsync(missingUsernames);

        await context.PlayerProfiles.AddRangeAsync(getMissingUserNames);
        await context.SaveChangesAsync();
        return getMissingUserNames;
    }
}
