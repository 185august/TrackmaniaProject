using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TrackmaniaWebsiteAPI.ApiHelper;
using TrackmaniaWebsiteAPI.DatabaseQuery;
using TrackmaniaWebsiteAPI.Tokens;

namespace TrackmaniaWebsiteAPI.PlayerAccount;

public class PlayerAccountService(TrackmaniaDbContext context, IApiHelperMethods apiHelperMethods)
{
    public async Task<List<PlayerProfiles>> GetUbisoftAccountIdAsync(string[] accountNames)
    {
        string requestNames = string.Join("&", accountNames.Select(n => $"displayName[]={n}"));
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

        return accountNames
            .Select(user => new PlayerProfiles
            {
                UbisoftUsername = user,
                UbisoftUserId = response.GetProperty(user).ToString(),
            })
            .ToList();
    }

    public async Task<PlayerProfiles?> GetUbisoftAccountNameAsync(string accountId)
    {
        var requestUri = $"https://api.trackmania.com/api/display-names?accountId[]={accountId}";
        var request = await apiHelperMethods.CreateRequestWithAuthorization(
            TokenTypes.OAuth,
            requestUri,
            AuthorizationHeaderValue.Bearer
        );
        var response = await apiHelperMethods.SendRequestAsync<JsonElement>(request);

        var wr = new PlayerProfiles
        {
            UbisoftUserId = accountId,
            UbisoftUsername = response.GetProperty(accountId).ToString().ToUpper(),
        };
        context.PlayerProfiles.Add(wr);
        await context.SaveChangesAsync();
        return wr;
    }

    public async Task<List<PlayerProfiles>> GetAndUpdatePlayerAccountsAsync(string playerNames)
    {
        var listOfPlayers = playerNames.ToUpper().Split(',').ToList();
        var playersInDb = await context
            .PlayerProfiles.AsNoTracking()
            .Where(p => listOfPlayers.Contains(p.UbisoftUsername))
            .ToListAsync();
        var existingUsernames = playersInDb
            .Select(profiles => profiles.UbisoftUsername.Trim())
            .ToHashSet();

        string[] missingUsernames = listOfPlayers.Except(existingUsernames).ToArray();
        if (missingUsernames.Length == 0)
            return playersInDb;

        var getMissingUserNames = await AddNewPlayerProfilesToDbAsync(missingUsernames);
        if (getMissingUserNames.Count == 0)
            return [];

        playersInDb.AddRange(getMissingUserNames);

        return playersInDb;
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
