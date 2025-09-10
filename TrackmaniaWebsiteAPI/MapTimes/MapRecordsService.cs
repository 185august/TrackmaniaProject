using System.Net.Http.Headers;
using System.Text.Json;
using TrackmaniaWebsiteAPI.DatabaseQuery;
using TrackmaniaWebsiteAPI.PlayerAccount;
using TrackmaniaWebsiteAPI.RequestQueue;
using TrackmaniaWebsiteAPI.Tokens;

namespace TrackmaniaWebsiteAPI.MapTimes;

public class MapRecordsService(
    ITokenFetcher apiTokensService,
    IApiRequestQueue queue,
    ITimeCalculationService calculationService,
    IComparisonPlayerService comparisonPlayerService
)
{
    public async Task<MapPersonalBestInfo?> GetMapWr(string mapUid)
    {
        var liveAccessToken = await apiTokensService.RetrieveAccessTokenAsync(TokenTypesNew.Live);

        var requestUri =
            $"https://live-services.trackmania.nadeo.live/api/token/leaderboard/group/Personal_Best/map/{mapUid}/top?length=1&onlyWorld=true&offset=0";

        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        request.Headers.Authorization = new AuthenticationHeaderValue(
            "nadeo_v1",
            $"t={liveAccessToken}"
        );

        var response = await queue.QueueRequest(client => client.SendAsync(request));

        var responseBody = await response.Content.ReadAsStringAsync();

        var obj = JsonSerializer.Deserialize<MapWrInfo>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        if (obj is null)
        {
            return null;
        }
        int time = obj.Tops[0].Top[0].Score;
        string accountId = obj.Tops[0].Top[0].AccountId;

        var recordHolder = await comparisonPlayerService.UpdateUserComparisonPlayers(accountId);
        var recordHolderName =
            recordHolder.Count == 0 ? "World Record" : recordHolder[0].UbisoftUsername + "(WR)";
        MapPersonalBestInfo mapTime = new()
        {
            AccountId = accountId,
            Name = recordHolderName,
            Medal = 4,
            RecordScore =
            {
                FormatedTime = calculationService.FormatTime(time),
                RespawnCount = 0,
                Time = time,
            },
        };
        return mapTime;
    }

    public async Task<List<MapPersonalBestInfo>> GetMapPersonalBestInfo(
        string mapId,
        PlayerProfiles[] players
    )
    {
        string accountIdList = "";
        for (int i = 0; i < players.Length; i++)
        {
            if (i != 0)
            {
                accountIdList += $",{players[0].UbisoftUserId.Trim()}";
            }
            else
            {
                accountIdList += players[0].UbisoftUserId.Trim();
            }
        }
        string coreAccessToken = await apiTokensService.RetrieveAccessTokenAsync(
            TokenTypesNew.Core
        );
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

        Console.WriteLine($"Response body length: {responseBody?.Length ?? 0}");
        Console.WriteLine($"Response body {responseBody}");

        var obj = JsonSerializer.Deserialize<List<MapPersonalBestInfo>>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        if (obj == null)
            return obj;
        foreach (var data in obj)
        {
            data.RecordScore.FormatedTime = calculationService.FormatTime(data.RecordScore.Time);
        }
        return obj;
    }
}
