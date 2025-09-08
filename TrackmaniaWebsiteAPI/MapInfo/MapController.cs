using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TrackmaniaWebsiteAPI.Data;
using TrackmaniaWebsiteAPI.MapTimes;
using TrackmaniaWebsiteAPI.Models;
using TrackmaniaWebsiteAPI.RequestQueue;
using TrackmaniaWebsiteAPI.Tokens;

namespace TrackmaniaWebsiteAPI.MapInfo
{
    [Route("[controller]")]
    [ApiController]
    public class MapController(
        IApiTokensService apiTokensService,
        IMapInfoService mapInfoService,
        ITimeCalculationService calculationService,
        IApiRequestQueue queue,
        MapRecordsService mapRecordsService
    ) : ControllerBase
    {
        //Requires core token
        [HttpGet("GetMapInfo")]
        public async Task<ActionResult> GetMapInfo(string mapUids)
        {
            string coreAccessToken = await apiTokensService.RetrieveTokenAsync(
                TokenTypes.CoreAccess
            );

            var requestUri =
                $"https://prod.trackmania.core.nadeo.online/maps/?mapUidList={mapUids}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "nadeo_v1",
                $"t={coreAccessToken}"
            );

            using var client = new HttpClient();
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return Problem(
                    $"API request failed with status code {response.StatusCode}. Reason: {response.ReasonPhrase}"
                );
            }
            var responseBody = await response.Content.ReadAsStreamAsync();

            try
            {
                // var obj = await JsonSerializer.DeserializeAsync<List<CampaignMapsInfo>>(
                //     responseBody,
                //     new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                // );
                // string jsonString = JsonSerializer.Serialize(
                //     obj,
                //     new JsonSerializerOptions { WriteIndented = true }
                // );
                // string filePath = "campaignMapsInfo.json";
                // await System.IO.File.WriteAllTextAsync(filePath, jsonString);
                var obj = JsonSerializer.Deserialize<JsonElement>(responseBody);
                return Ok(obj);
            }
            catch (JsonException e)
            {
                return Problem(
                    $"Failed to parse API response as JSON. Response content: {responseBody}. Error: {e.Message} "
                );
            }
        }

        [HttpGet("GetAllMapTimes")]
        public async Task<ActionResult<List<MapPersonalBestInfo>>> GetAllMapTimes(
            string mapId,
            string accountIdList,
            string mapUid
        )
        {
            try
            {
                var wr = await mapRecordsService.GetMapWr(mapUid);
                if (wr is null)
                {
                    return BadRequest("Could not get current wr");
                }
                var otherRecords = await mapRecordsService.GetMapPersonalBestInfo(
                    mapId,
                    accountIdList
                );
                var list = new List<MapPersonalBestInfo> { wr };
                list.AddRange(otherRecords);
                return Ok(list);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest("Could not get info");
            }
        }

        //Core access token
        [HttpGet("GetMapTimeByIds")]
        public async Task<ActionResult> GetMapTimeByIds(
            string mapId,
            string accountIdList,
            string mapUid
        )
        {
            var coreAccessToken = await apiTokensService.RetrieveTokenAsync(TokenTypes.CoreAccess);
            string requestUri =
                $"https://prod.trackmania.core.nadeo.online/v2/mapRecordsService/?accountIdList={accountIdList}&mapId={mapId}";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            request.Headers.Authorization = new AuthenticationHeaderValue(
                "nadeo_v1",
                $"t={coreAccessToken}"
            );

            //var client = new HttpClient();
            //var response = await client.SendAsync(request);
            var response = await queue.QueueRequest(client => client.SendAsync(request));
            Console.WriteLine($"Response status: {response.StatusCode}");
            Console.WriteLine(
                $"Response Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"))}"
            );

            if (!response.IsSuccessStatusCode)
            {
                return Problem(
                    $"API request failed with status code {response.StatusCode}. Reason: {response.ReasonPhrase}"
                );
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Response body length: {responseBody?.Length ?? 0}");
            Console.WriteLine($"Response body {responseBody}");
            if (string.IsNullOrEmpty(responseBody))
            {
                return Problem("API returned empty response");
            }

            try
            {
                var obj = JsonSerializer.Deserialize<List<MapPersonalBestInfo>>(
                    responseBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                if (obj is null)
                {
                    return Problem("Couldn't retrieve information about the time on the map");
                }

                foreach (var data in obj)
                {
                    if (data.RecordScore != null)
                        data.RecordScore.FormatedTime = calculationService.FormatTime(
                            data.RecordScore.Time
                        );
                }

                // if (obj.Count != 2)
                //     return Ok(obj);

                // var recordScoreNested = obj[0].RecordScore;
                // if (recordScoreNested != null)
                // {
                //     var recordScore = obj[1].RecordScore;
                //     if (recordScore != null)
                //     {
                //         double timeDifference = calculationService.CalculateTimeDifferenceWr(
                //             recordScoreNested.Time,
                //             recordScore.Time
                //         );
                //         string differenceText = $"The time difference was {timeDifference:F3} ";
                //         return Ok(new { obj, differenceText });
                //     }
                // }

                return Ok(obj);
            }
            catch (JsonException e)
            {
                return Problem(
                    $"Failed to parse API response as JSON. Response content: {responseBody}. Error: {e.Message} "
                );
            }
        }

        [HttpGet("GetMapWr")]
        public async Task<ActionResult> GetMapWr(string mapUid)
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
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            try
            {
                var obj = JsonSerializer.Deserialize<MapWrInfo>(
                    responseBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                return Ok(obj);
            }
            catch (JsonException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpGet("GetMapsByYearAndSeason")]
        public async Task<ActionResult<List<CampaignMapsInfo>>> GetMapsByYearAndSeason(
            int year,
            string season
        )
        {
            try
            {
                var maps = await mapInfoService.FindMapByYearAndSeason(year, season);
                return Ok(maps);
            }
            catch (Exception e)
            {
                return Problem($"Failed to retrieve data {e.Message}");
            }
        }

        [HttpGet("GetMapsBySeason")]
        public async Task<ActionResult<List<CampaignMapsInfo>>> GetMapsByYear(string season)
        {
            try
            {
                var maps = await mapInfoService.FindMapBySeason(season);
                return Ok(maps);
            }
            catch (Exception e)
            {
                return Problem($"Failed to retrieve data {e.Message}");
            }
        }
    }
}
