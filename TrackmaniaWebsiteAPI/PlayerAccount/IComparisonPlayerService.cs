using TrackmaniaWebsiteAPI.DatabaseQuery;

namespace TrackmaniaWebsiteAPI.PlayerAccount;

public interface IComparisonPlayerService
{
    Task<List<PlayerProfiles>> UpdateUserComparisonPlayers(string playerNames);

    Task<List<PlayerProfiles>> GetCurrentUserValues(
        int userId,
        List<PlayerProfiles> playerProfiles
    );
}
