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
            .PlayerProfiles.Where(p => listOfPlayers.Contains(p.UbisoftUsername.ToLower()))
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

    public async Task<List<PlayerProfiles>> GetCurrentUserValues(
        int userId,
        List<PlayerProfiles> playerProfiles
    )
    {
        var user = await database
            .Users.Include(u => u.PlayerProfile)
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync();
        if (user != null)
        {
            playerProfiles.Add(
                new PlayerProfiles
                {
                    UbisoftUserId = user.UbisoftUserId,
                    UbisoftUsername = user.UbisoftUsername,
                }
            );
        }

        return playerProfiles;
    }
}
