using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrackmaniaWebsiteAPI.Models;
using TrackmaniaWebsiteAPI.Services;

namespace TrackmaniaWebsiteAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OAuthTokenController(
        IConfiguration configuration,
        IApiTokensService apiTokensService
    ) : ControllerBase
    {
        private readonly string? _identifier = configuration["OAuth2Identifier"];
        private readonly string? _secret = configuration["OAuth2Secret"];

        [HttpPost("AcquireOAuthToken")]
        public async Task<ActionResult> AcquireOAuthToken()
        {
            var content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = _identifier,
                    ["client_secret"] = _secret,
                }
            );
            using var client = new HttpClient();

            var response = await client.PostAsync(
                "https://api.trackmania.com/api/access_token",
                content
            );
            string body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest(new { status = Response.StatusCode, error = body });
            }

            var obj = JsonSerializer.Deserialize<JsonElement>(body);

            string? accessToken = obj.GetProperty("access_token").GetString();
            if (accessToken is null)
                return Problem("No valid access token");

            apiTokensService.UpdateToken(TokenTypes.OAuth2Access, accessToken);
            return Ok(accessToken);
        }
    }
}
