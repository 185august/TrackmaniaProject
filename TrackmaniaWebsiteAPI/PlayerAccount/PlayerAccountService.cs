using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TrackmaniaWebsiteAPI.ApiHelper;
using TrackmaniaWebsiteAPI.DatabaseQuery;
using TrackmaniaWebsiteAPI.Tokens;

namespace TrackmaniaWebsiteAPI.PlayerAccount;

public class PlayerAccountService(TrackmaniaDbContext context, IApiHelperMethods apiHelperMethods)
{
    private async Task<List<PlayerProfileDto>> GetPlayerProfilesFromDatabaseAsync(
        List<string> listOfPlayers
    )
    {
        return await context
            .PlayerProfiles.AsNoTracking()
            .Where(p => listOfPlayers.Contains(p.UbisoftUsername))
            .Select(u => new PlayerProfileDto()
            {
                UbisoftUserId = u.UbisoftUserId,
                UbisoftUsername = u.UbisoftUsername,
            })
            .ToListAsync();
    }

    public async Task<List<PlayerProfileDto>> GetAndUpdatePlayerAccountsAsync(string playerNames)
    {
        var listOfPlayers = playerNames.ToUpper().Split(',').ToList();
        var playersInDb = await GetPlayerProfilesFromDatabaseAsync(listOfPlayers);
        var existingUsernames = playersInDb
            .Select(profiles => profiles.UbisoftUsername.Trim())
            .ToHashSet();

        string[] missingUsernames = listOfPlayers.Except(existingUsernames).ToArray();
        if (missingUsernames.Length == 0)
            return playersInDb;

        var getMissingUserNames = await AddNewPlayerProfilesToDbAsync(missingUsernames);
        if (getMissingUserNames.Count == 0)
            return [];

        return playersInDb;
    }

    public async Task<List<PlayerProfiles>> GetUbisoftAccountIdAsync(string[] ubisoftAccountNames)
    {
        string requestNames = string.Join(
            "&",
            ubisoftAccountNames.Select(n => $"displayName[]={n}")
        );
        string requestUri =
            $"https://api.trackmania.com/api/display-names/account-ids?{requestNames}";

        var request = await apiHelperMethods.CreateRequestWithAuthorization(
            TokenTypes.OAuth,
            requestUri,
            AuthorizationHeaderValue.Bearer
        );

        var response = await apiHelperMethods.SendRequestAsync<JsonElement>(request);

        if (response.ValueKind == JsonValueKind.Array && response.GetArrayLength() == 0)
        {
            return [];
        }

        return ubisoftAccountNames
            .Select(user => new PlayerProfiles
            {
                UbisoftUsername = user,
                UbisoftUserId = response.GetProperty(user).ToString(),
            })
            .ToList();
    }

    public async Task<PlayerProfiles?> GetUbisoftAccountNameAsync(string accountId)
    {
        string requestUri = $"https://api.trackmania.com/api/display-names?accountId[]={accountId}";
        var request = await apiHelperMethods.CreateRequestWithAuthorization(
            TokenTypes.OAuth,
            requestUri,
            AuthorizationHeaderValue.Bearer
        );
        var response = await apiHelperMethods.SendRequestAsync<JsonElement>(request);

        var playerProfile = new PlayerProfiles
        {
            UbisoftUserId = accountId,
            UbisoftUsername = response.GetProperty(accountId).ToString().ToUpper(),
        };
        context.PlayerProfiles.Add(playerProfile);
        await context.SaveChangesAsync();
        return playerProfile;
    }

    private async Task<List<PlayerProfileDto>> AddNewPlayerProfilesToDbAsync(
        string[] missingUsernames
    )
    {
        var getMissingUserNames = await GetUbisoftAccountIdAsync(missingUsernames);

        await context.PlayerProfiles.AddRangeAsync(getMissingUserNames);
        await context.SaveChangesAsync();
        return getMissingUserNames
            .Select(profiles => new PlayerProfileDto
            {
                UbisoftUserId = profiles.UbisoftUserId,
                UbisoftUsername = profiles.UbisoftUsername,
            })
            .ToList();
    }
}
