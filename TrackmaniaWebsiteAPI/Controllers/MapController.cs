using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TrackmaniaWebsiteAPI.Models;
using TrackmaniaWebsiteAPI.Services;

namespace TrackmaniaWebsiteAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MapController(IApiTokensService apiTokensService) : ControllerBase
    {
        private readonly string? _coreAccessToken = apiTokensService.GetToken(
            TokenTypes.CoreAccess
        );

        //Need Live Token
        [HttpGet("GetCampaigns")]
        public async Task<ActionResult> GetCampaigns(int length, int offset)
        {
            var liveAccessToken = apiTokensService.GetToken(TokenTypes.LiveAccess);
            if (String.IsNullOrEmpty(liveAccessToken))
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
                // var mapList = new MapService();
                // mapList.AddMapToList(responseBody);
                return Ok(obj);
            }
            catch (JsonException e)
            {
                return Problem(
                    $"Failed to parse API response as JSON. Response content: {responseBody}. Error: {e.Message}"
                );
            }
        }

        //Requires core token
        [HttpGet("GetMapInfo")]
        public async Task<ActionResult> GetMapInfo()
        {
            var requestUri =
                "https://live-services.trackmania.nadeo.live/api/token/map/get-multiple?mapUidList={mapUidList}";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "nadeo_v1",
                $"t={_coreAccessToken}"
            );

            using var client = new HttpClient();
            var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<JsonElement>(responseBody);
            return Ok(obj);
        }
    }
}
