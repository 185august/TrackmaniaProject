using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackmaniaWebsiteAPI.Data;
using TrackmaniaWebsiteAPI.Models;

namespace TrackmaniaWebsiteAPI.PlayerAccount;

public class ComparisonPlayersService(
    TrackmaniaDbContext database,
    PlayerAccountService playerAccountService
)
{
    public async Task<IActionResult> UpdateUserComparisonPlayers(string playersId)
    {
        var listOfPlayers = playersId.ToLower().Split(',').ToList();
        var existingPlayer = await database
            .PlayerProfiles.Where(p => listOfPlayers.Contains(p.UbisoftUsername.ToLower()))
            .ToListAsync();
        var existingUsernames = existingPlayer
            .Select(profiles => profiles.UbisoftUsername)
            .ToHashSet();
        var missingUsernames = listOfPlayers.Except(existingUsernames).ToArray();

        var getMissingUserNames = await playerAccountService.GetUbisoftAccountIdAsync(
            missingUsernames
        );
        var playerObj = new List<PlayerProfiles>();

        await database.PlayerProfiles.AddRangeAsync(getMissingUserNames);
    }
}
