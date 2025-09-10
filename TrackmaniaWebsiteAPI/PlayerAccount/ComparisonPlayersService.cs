using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackmaniaWebsiteAPI.DatabaseQuery;

namespace TrackmaniaWebsiteAPI.PlayerAccount;

public class ComparisonPlayersService(
    TrackmaniaDbContext database,
    PlayerAccountService playerAccountService
) : IComparisonPlayerService
{
    public async Task<List<PlayerProfiles>> UpdateUserComparisonPlayers(string playerNames)
    {
        var listOfPlayers = playerNames.ToLower().Split(',').ToList();
        var existingPlayers = await database
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

        var getMissingUserNames = await playerAccountService.GetUbisoftAccountIdAsync(
            missingUsernames
        );

        await database.PlayerProfiles.AddRangeAsync(getMissingUserNames);
        await database.SaveChangesAsync();

        existingPlayers.AddRange(getMissingUserNames);

        return existingPlayers;
    }
}
