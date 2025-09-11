using System.Net.Http.Headers;
using System.Text.Json;
using TrackmaniaWebsiteAPI.DatabaseQuery;
using TrackmaniaWebsiteAPI.PlayerAccount;
using TrackmaniaWebsiteAPI.RequestQueue;
using TrackmaniaWebsiteAPI.Tokens;

namespace TrackmaniaWebsiteAPI.MapTimes;

public class MapTimesService(
    ITokenFetcher apiTokensService,
    IApiRequestQueue queue,
    ITimeCalculationService calculationService,
    PlayerAccountService playerAccountService
)
{
    public async Task<MapPersonalBestData?> GetMapWr(string mapUid)
    {
        var liveAccessToken = await apiTokensService.RetrieveAccessTokenAsync(TokenTypes.Live);

        var requestUri =
            $"https://live-services.trackmania.nadeo.live/api/token/leaderboard/group/Personal_Best/map/{mapUid}/top?length=1&onlyWorld=true&offset=0";

        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        request.Headers.Authorization = new AuthenticationHeaderValue(
            "nadeo_v1",
            $"t={liveAccessToken}"
        );

        var response = await queue.QueueRequest(client => client.SendAsync(request));

        var responseBody = await response.Content.ReadAsStringAsync();

        var obj = JsonSerializer.Deserialize<MapWrData>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        if (obj is null)
        {
            return null;
        }
        int time = obj.Tops[0].Top[0].Score;
        string accountId = obj.Tops[0].Top[0].AccountId;

        var recordHolder = await playerAccountService.GetAndUpdatePlayerAccountsAsync(accountId);
        var recordHolderName =
            recordHolder.Count == 0 ? "World Record" : recordHolder[0].UbisoftUsername + "(WR)";
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
        string accountIdList = "";
        for (int i = 0; i < players.Length; i++)
        {
            if (i != 0)
            {
                accountIdList += $",{players[i].UbisoftUserId.Trim()}";
            }
            else
            {
                accountIdList += players[i].UbisoftUserId.Trim();
            }
        }
        string coreAccessToken = await apiTokensService.RetrieveAccessTokenAsync(TokenTypes.Core);
        string requestUri =
            $"https://prod.trackmania.core.nadeo.online/v2/mapRecords/?accountIdList={accountIdList}&mapId={mapId}";

        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        request.Headers.Authorization = new AuthenticationHeaderValue(
            "nadeo_v1",
            $"t={coreAccessToken}"
        );

        var response = await queue.QueueRequest(client => client.SendAsync(request));

        response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync();

        var obj = JsonSerializer.Deserialize<List<MapPersonalBestData>>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        if (obj.Count == 0)
            return obj;

        foreach (var data in obj)
        {
            foreach (var player in players)
            {
                if (data.AccountId == player.UbisoftUserId)
                {
                    data.Name = player.UbisoftUsername;
                }
            }
            data.RecordScore.FormatedTime = calculationService.FormatTime(data.RecordScore.Time);
            switch (data.Medal)
            {
                case 4:
                    data.MedalText = "Author";
                    break;
                case 3:
                    data.MedalText = "Gold";
                    break;
                case 2:
                    data.MedalText = "Silver";
                    break;
                case 1:
                    data.MedalText = "Bronze";
                    break;
                default:
                    data.MedalText = "No medal";
                    break;
            }
        }
        return obj;
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
}
