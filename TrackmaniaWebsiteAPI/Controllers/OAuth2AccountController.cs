using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrackmaniaWebsiteAPI.Models;
using TrackmaniaWebsiteAPI.Services;

namespace TrackmaniaWebsiteAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OAuth2AccountController(IApiTokensService apiTokensService) : ControllerBase
    {
        [HttpGet("GetAccountId")]
        public async Task<ActionResult> GetUbisoftAccountId(string accountName)
        {
            var accessToken = await apiTokensService.RetrieveTokenAsync(TokenTypes.OAuth2Access);
            var requestUri =
                $"https://api.trackmania.com/api/display-names/account-ids?displayName[]={accountName}";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                $"{accessToken}"
            );

            using var client = new HttpClient();
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return Problem(response.ReasonPhrase);
            }
            var responseBody = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<JsonElement>(responseBody);
            return Ok(obj);
        }
    }
}
