using System.ComponentModel.DataAnnotations;
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
    private readonly Dictionary<MedalTypes, string> _medals = new()
    {
        { MedalTypes.NoMedal, "No Medal" },
        { MedalTypes.Bronze, "Bronze" },
        { MedalTypes.Silver, "Silver" },
        { MedalTypes.Gold, "Gold" },
        { MedalTypes.Author, "Author" },
    };

    public async Task<MapPersonalBestDto?> GetMapWr(string mapUid)
    {
        string requestUri =
            $"https://live-services.trackmania.nadeo.live/api/token/leaderboard/group/Personal_Best/map/{mapUid}/top?length=1&onlyWorld=true&offset=0";
        var request = await apiHelperMethods.CreateRequestWithAuthorization(
            TokenTypes.Live,
            requestUri
        );

        var recordData = await apiHelperMethods.SendRequestAsync<MapWrDto>(request);
        if (recordData is null || recordData.Tops.Count == 0)
            return null;

        int time = recordData.Tops[0].Top[0].Score;
        string accountId = recordData.Tops[0].Top[0].AccountId;
        string recordHolderName = await GetWrName(accountId);
        //Since it's only for campaign maps, the world record will always have author medal/4
        const int authorMedal = (int)MedalTypes.Author;
        const string authorMedalText = nameof(MedalTypes.Author);
        return new MapPersonalBestDto(
            accountId,
            recordHolderName,
            authorMedal,
            authorMedalText,
            new RecordScoreNested(time, calculationService.FormatTime(time))
        );
    }

    public async Task<List<MapPersonalBestDto>> GetMapPersonalBestData(
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

        var playersData = await apiHelperMethods.SendRequestAsync<List<MapPersonalBestDto>>(
            request
        );
        if (playersData is null)
        {
            return [];
        }
        return playersData.Count == 0 ? playersData : FormatPlayersData(players, playersData);
    }

    public List<MapPersonalBestDto> GetTimeDifferenceToWrAndSort(
        List<MapPersonalBestDto> otherRecords,
        MapPersonalBestDto wr
    )
    {
        var personalBestInfos = new List<MapPersonalBestDto> { wr };
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

    private string GetMedalType(MedalTypes medalType)
    {
        return _medals[medalType];
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

    private List<MapPersonalBestDto> FormatPlayersData(
        PlayerProfiles[] players,
        List<MapPersonalBestDto> playersData
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
            data.MedalText = GetMedalType((MedalTypes)data.Medal);
        }

        return playersData;
    }

    private enum MedalTypes
    {
        [Display(Name = "No medal")]
        NoMedal,
        Bronze,
        Silver,
        Gold,
        Author,
    }
}
