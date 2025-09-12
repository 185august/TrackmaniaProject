using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TrackmaniaWebsiteAPI.ApiHelper;
using TrackmaniaWebsiteAPI.DatabaseQuery;
using TrackmaniaWebsiteAPI.PlayerAccount;
using TrackmaniaWebsiteAPI.Tokens;

namespace TrackmaniaWebsiteAPI.MapTimes;

public class MapTimesService(
    IApiHelperMethods apiHelperMethods,
    ITimeCalculationService calculationService,
    PlayerAccountService playerAccountService,
    TrackmaniaDbContext context
)
{
    private Dictionary<int, string> Medals = new()
    {
        { 0, "No Medal" },
        { 1, "Bronze" },
        { 2, "Silver" },
        { 3, "Gold" },
        { 4, "Author" },
    };

    public async Task<MapPersonalBestData?> GetMapWr(string mapUid)
    {
        string requestUri =
            $"https://live-services.trackmania.nadeo.live/api/token/leaderboard/group/Personal_Best/map/{mapUid}/top?length=1&onlyWorld=true&offset=0";
        var request = await apiHelperMethods.CreateRequestWithAuthorization(
            TokenTypes.Live,
            requestUri
        );

        var recordData = await apiHelperMethods.SendRequestAsync<MapWrData>(request);
        if (recordData is null || recordData.Tops.Count == 0)
            return null;

        int time = recordData.Tops[0].Top[0].Score;
        string accountId = recordData.Tops[0].Top[0].AccountId;
        string recordHolderName = await GetWrName(accountId);
        MapPersonalBestData mapTime = new()
        {
            AccountId = accountId,
            Name = recordHolderName,
            Medal = 4,
            MedalText = "Author",
            RecordScore =
            {
                FormatedTime = calculationService.FormatTime(time),
                RespawnCount = 0,
                Time = time,
            },
        };
        return mapTime;
    }

    public async Task<List<MapPersonalBestData>> GetMapPersonalBestData(
        string mapId,
        PlayerProfiles[] players
    )
    {
        string accountsIdString = string.Join(",", players.Select(p => p.UbisoftUserId));

        string requestUri =
            $"https://prod.trackmania.core.nadeo.online/v2/mapRecords/?accountIdList={accountsIdString}&mapId={mapId}";

        var request = await apiHelperMethods.CreateRequestWithAuthorization(
            TokenTypes.Core,
            requestUri
        );

        var playersData = await apiHelperMethods.SendRequestAsync<List<MapPersonalBestData>>(
            request
        );
        if (playersData is null)
        {
            return [];
        }
        return playersData.Count == 0 ? playersData : FormatPlayersData(players, playersData);
    }

    public List<MapPersonalBestData> GetTimeDifferenceToWrAndSort(
        List<MapPersonalBestData> otherRecords,
        MapPersonalBestData wr
    )
    {
        var personalBestInfos = new List<MapPersonalBestData> { wr };
        personalBestInfos.AddRange(otherRecords);
        foreach (var person in personalBestInfos)
        {
            person.RecordScore.TimeVsWr = calculationService.CalculateTimeDifferenceWr(
                wr.RecordScore.Time,
                person.RecordScore.Time
            );
        }
        personalBestInfos.Sort((a, b) => a.RecordScore.TimeVsWr - b.RecordScore.TimeVsWr);
        return personalBestInfos;
    }

    private string GetMedalType(int medalNumber)
    {
        return Medals[medalNumber];
    }

    private async Task<string> GetWrName(string accountId)
    {
        var nameDatabaseRequest = await context
            .PlayerProfiles.AsNoTracking()
            .Where(p => accountId.Contains(p.UbisoftUserId))
            .ToArrayAsync();
        if (nameDatabaseRequest.Length != 0)
            return nameDatabaseRequest[0].UbisoftUsername + "(WR)";

        var nameRequest = await playerAccountService.GetUbisoftAccountNameAsync(accountId);
        return nameRequest is not null ? nameRequest.UbisoftUsername + "(WR)" : "WORLD RECORD";
    }

    private List<MapPersonalBestData> FormatPlayersData(
        PlayerProfiles[] players,
        List<MapPersonalBestData> playersData
    )
    {
        var playerLookUp = players.ToDictionary(p => p.UbisoftUserId, p => p.UbisoftUsername);

        foreach (var data in playersData)
        {
            if (playerLookUp.TryGetValue(data.AccountId, out var username))
            {
                data.Name = username;
            }
            data.RecordScore.FormatedTime = calculationService.FormatTime(data.RecordScore.Time);
            data.MedalText = GetMedalType(data.Medal);
        }

        return playersData;
    }
}
