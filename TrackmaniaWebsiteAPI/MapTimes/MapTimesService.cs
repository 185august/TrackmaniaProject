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
    private JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private async Task<HttpRequestMessage> CreateRequestWithAuthorization(
        TokenTypes tokenType,
        string requestUriValue
    )
    {
        string accessToken = await apiTokensService.RetrieveAccessTokenAsync(tokenType);

        var request = new HttpRequestMessage(HttpMethod.Get, requestUriValue);

        request.Headers.Authorization = new AuthenticationHeaderValue(
            "nadeo_v1",
            $"t={accessToken}"
        );
        return request;
    }

    private async Task<T?> SendRequestAndGetJsonString<T>(HttpRequestMessage request)
    {
        var response = await queue.QueueRequest(client => client.SendAsync(request));

        response.EnsureSuccessStatusCode();

        string jsonString = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<T>(jsonString, jsonSerializerOptions);
    }

    public async Task<MapPersonalBestData?> GetMapWr(string mapUid)
    {
        string requestUri =
            $"https://live-services.trackmania.nadeo.live/api/token/leaderboard/group/Personal_Best/map/{mapUid}/top?length=1&onlyWorld=true&offset=0";
        var request = await CreateRequestWithAuthorization(TokenTypes.Live, requestUri);
        // var response = await queue.QueueRequest(client => client.SendAsync(request));
        //
        // string responseBody = await response.Content.ReadAsStringAsync();
        //
        // var obj = JsonSerializer.Deserialize<MapWrData>(responseBody, jsonSerializerOptions);
        var obj = await SendRequestAndGetJsonString<MapWrData>(request);
        if (obj is null)
            return null;

        if (obj.Tops.Count == 0)
            return null;

        int time = obj.Tops[0].Top[0].Score;
        string accountId = obj.Tops[0].Top[0].AccountId;

        var recordHolder = await playerAccountService.GetAndUpdatePlayerAccountsAsync(accountId);
        string recordHolderName =
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
        string requestUri =
            $"https://prod.trackmania.core.nadeo.online/v2/mapRecords/?accountIdList={accountIdList}&mapId={mapId}";

        var request = await CreateRequestWithAuthorization(TokenTypes.Core, requestUri);
        // var response = await queue.QueueRequest(client => client.SendAsync(request));
        //
        // response.EnsureSuccessStatusCode();
        //
        // string responseBody = await response.Content.ReadAsStringAsync();
        //
        // var obj = JsonSerializer.Deserialize<List<MapPersonalBestData>>(
        //     responseBody,
        //     jsonSerializerOptions
        // );
        var obj = await SendRequestAndGetJsonString<List<MapPersonalBestData>>(request);
        if (obj is null)
        {
            return [];
        }
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
            data.MedalText = data.Medal switch
            {
                4 => "Author",
                3 => "Gold",
                2 => "Silver",
                1 => "Bronze",
                _ => "No medal",
            };
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
