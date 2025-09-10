using TrackmaniaWebsiteAPI.DatabaseQuery;

namespace TrackmaniaWebsiteAPI.PlayerAccount;

public interface IComparisonPlayerService
{
    Task<List<PlayerProfiles>> UpdateUserComparisonPlayers(string playerNames);
}
