using TrackmaniaWebsiteAPI.DatabaseQuery;

namespace TrackmaniaWebsiteAPI.PlayerAccount;

public interface IPlayerAccountService
{
    Task<List<PlayerProfileDto>> GetAndUpdatePlayerAccountsAsync(string playerNames);
    Task<List<PlayerProfiles>> GetUbisoftAccountIdAsync(string[] ubisoftAccountNames);
    Task<PlayerProfiles?> GetUbisoftAccountNameAsync(string accountId);
}
