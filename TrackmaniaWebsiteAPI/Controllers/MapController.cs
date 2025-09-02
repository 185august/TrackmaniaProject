using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackmaniaWebsiteAPI.Data;
using TrackmaniaWebsiteAPI.Models;
using TrackmaniaWebsiteAPI.Services;

namespace TrackmaniaWebsiteAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MapController(
        IApiTokensService apiTokensService,
        IMapInfoService mapInfoService,
        ITimeCalculationService calculationService,
        TrackmaniaDbContext context
    ) : ControllerBase
    {
        /*
        //Need Live Token
        [HttpGet("GetCampaigns")]
        public async Task<ActionResult> GetCampaigns(int length, int offset)
        {
            var liveAccessToken = await apiTokensService.RetrieveTokenAsync(TokenTypes.LiveAccess);
            if (String.IsNullOrEmpty(liveAccessToken.ToString()))
            {
                return Problem("Not a valid token");
            }
            string requestUri =
                $"https://live-services.trackmania.nadeo.live/api/campaign/official?length={length}&offset={offset}";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            request.Headers.Authorization = new AuthenticationHeaderValue(
                "nadeo_v1",
                $"t={liveAccessToken}"
            );

            using var client = new HttpClient();
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return Problem(
                    $"API request failed with status code: {response.StatusCode}. Reason: {response.ReasonPhrase}"
                );
            }
            string responseBody = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return Problem("API returned empty response");
            }

            try
            {
                var obj = JsonSerializer.Deserialize<JsonElement>(responseBody);
                return Ok(obj);
            }
            catch (JsonException e)
            {
                return Problem(
                    $"Failed to parse API response as JSON. Response content: {responseBody}. Error: {e.Message}"
                );
            }
        }
        */
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

        [HttpGet("GetMapRecord")]
        public async Task<ActionResult> GetMapRecord(string mapId, string accountIdList)
        {
            var coreAccessToken = await apiTokensService.RetrieveTokenAsync(TokenTypes.CoreAccess);
            string requestUri =
                $"https://prod.trackmania.core.nadeo.online/v2/mapRecords/?accountIdList={accountIdList}&mapId={mapId}";

            Console.WriteLine($"Request URI: {requestUri}");
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            request.Headers.Authorization = new AuthenticationHeaderValue(
                "nadeo_v1",
                $"t={coreAccessToken}"
            );

            var client = new HttpClient();
            var response = await client.SendAsync(request);

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
                var obj = JsonSerializer.Deserialize<List<MapRecordInfo>>(
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
                        data.RecordScore.FormatedTime =
                                calculationService.FormatTime(
                                        data.RecordScore.Time);
                }

                // if (obj.Count != 2)
                //     return Ok(obj);

                // var recordScoreNested = obj[0].RecordScore;
                // if (recordScoreNested != null)
                // {
                //     var recordScore = obj[1].RecordScore;
                //     if (recordScore != null)
                //     {
                //         double timeDifference = calculationService.CalculateTimeDifference(
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

        [HttpGet("GetMapsByYear")]
        public async Task<ActionResult<List<CampaignMapsInfo>>> GetMapsByYear(int year)
        {
            try
            {
                var maps = await mapInfoService.FindMapByYear(year);
                return Ok(maps);
            }
            catch (Exception e)
            {
                return Problem($"Failed to retrieve data {e.Message}");
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
