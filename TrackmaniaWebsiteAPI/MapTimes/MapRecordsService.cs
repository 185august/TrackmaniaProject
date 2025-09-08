using System.Net.Http.Headers;
using System.Text.Json;
using TrackmaniaWebsiteAPI.Models;
using TrackmaniaWebsiteAPI.RequestQueue;
using TrackmaniaWebsiteAPI.Tokens;

namespace TrackmaniaWebsiteAPI.MapTimes;

public class MapRecordsService(
    IApiTokensService apiTokensService,
    IApiRequestQueue queue,
    ITimeCalculationService calculationService
)
{
    public async Task<MapPersonalBestInfo?> GetMapWr(string mapUid)
    {
        var liveAccessToken = await apiTokensService.RetrieveTokenAsync(TokenTypes.LiveAccess);

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
        try
        {
            int time = obj.Tops[0].Top[0].Score;
            string accountId = obj.Tops[0].Top[0].AccountId;
            MapPersonalBestInfo mapTime = new()
            {
                AccountId = accountId,
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
        catch (NullReferenceException e)
        {
            Console.WriteLine($"Error: {e.Message}");
            throw;
        }
    }

    public async Task<List<MapPersonalBestInfo>> GetMapPersonalBestInfo(
        string mapId,
        string accountIdList
    )
    {
        string coreAccessToken = await apiTokensService.RetrieveTokenAsync(TokenTypes.CoreAccess);
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

        try
        {
            var obj = JsonSerializer.Deserialize<List<MapPersonalBestInfo>>(
                responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            foreach (var data in obj)
            {
                if (data.RecordScore != null)
                    data.RecordScore.FormatedTime = calculationService.FormatTime(
                        data.RecordScore.Time
                    );
            }

            return obj;
        }
        catch (JsonException e)
        {
            Console.WriteLine($"Json Exception: {e.Message}");
            return null;
        }
    }
}
