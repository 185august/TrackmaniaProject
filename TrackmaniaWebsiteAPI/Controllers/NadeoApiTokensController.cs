using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrackmaniaWebsiteAPI.Models;
using TrackmaniaWebsiteAPI.Services;
using JsonElement = System.Text.Json.JsonElement;

namespace TrackmaniaWebsiteAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NadeoApiTokensController(
        IConfiguration configuration,
        INadeoTokenService nadeoTokenService,
        IApiTokensService apiTokens
    ) : ControllerBase
    {
        [HttpPost("UbisoftTicket")]
        public async Task<ActionResult> RequestTicket()
        {
            string ubisoftEmail = configuration["UbisoftEmail"]!;
            string ubisoftPassword = configuration["UbisoftPassword"]!;
            var requestUri = "https://public-ubiservices.ubi.com/v3/profiles/sessions";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            request.Headers.Add("Ubi-AppId", "86263886-327a-4328-ac69-527f0d20a237");
            request.Headers.UserAgent.Add(
                new ProductInfoHeaderValue("TrackmaniaWebsiteAPI", "School-project")
            );
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("(spaceboysurf@hotmail.com)"));

            var credentials = $"{ubisoftEmail}:{ubisoftPassword}";
            var credentialsBytes = Encoding.UTF8.GetBytes(credentials);
            var base64Credentials = Convert.ToBase64String(credentialsBytes);
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Basic",
                base64Credentials
            );

            request.Content = new StringContent("", Encoding.UTF8, "application/json");

            using var client = new HttpClient();

            var response = await client.SendAsync(request);

            var responseBody = await response.Content.ReadAsStringAsync();

            var obj = JsonSerializer.Deserialize<JsonElement>(responseBody);

            var ticket = obj.GetProperty("ticket").GetString();
            HttpContext.Session.SetString("ticket", ticket);
            apiTokens.UpdateUbisoftTicket(ticket);
            return Ok(ticket);
        }

        // To get access to the live api
        [HttpPost("GetLiveApiToken")]
        public async Task<ActionResult> AcquireLiveApiToken()
        {
            string? ticket = apiTokens.GetUbisoftTicket();
            if (ticket is null)
            {
                await RequestTicket();
                ticket = apiTokens.GetUbisoftTicket();
            }
            const string jsonBody = "{ \"audience\": \"NadeoLiveServices\"}";

            var tokens = await nadeoTokenService.RequestNadeoTokenAsync(ticket, jsonBody);

            var accessToken = tokens.GetProperty("accessToken");
            var refreshToken = tokens.GetProperty("refreshToken");
            apiTokens.UpdateToken(TokenTypes.LiveAccess, accessToken.ToString());
            apiTokens.UpdateToken(TokenTypes.LiveRefresh, refreshToken.ToString());
            return Ok(new { accessToken, refreshToken });
        }

        // To get access to the Core api
        [HttpPost("GetCoreApiToken")]
        public async Task<ActionResult> AcquireCoreApiTokens()
        {
            string? ticket = apiTokens.GetUbisoftTicket();
            if (ticket is null)
            {
                await RequestTicket();
                ticket = apiTokens.GetUbisoftTicket();
            }

            const string jsonBody = "{ \"audience\": \"NadeoServices\" }";

            if (ticket == null)
                return Problem("No valid ticket");

            var tokens = await nadeoTokenService.RequestNadeoTokenAsync(ticket, jsonBody);

            var accessToken = tokens.GetProperty("accessToken");
            var refreshToken = tokens.GetProperty("refreshToken");
            apiTokens.UpdateToken(TokenTypes.CoreAccess, accessToken.ToString());
            apiTokens.UpdateToken(TokenTypes.CoreRefresh, refreshToken.ToString());
            return Ok(new { accessToken, refreshToken });
        }

        [HttpPost("RefreshLiveToken")]
        public async Task<ActionResult> RefreshLiveAccessToken()
        {
            var refreshToken = apiTokens.GetToken(TokenTypes.LiveRefresh);
            var newTokens = await nadeoTokenService.RefreshNadeoTokenAsync(refreshToken);
            var newAccessToken = newTokens.GetProperty("accessToken");
            var newRefreshToken = newTokens.GetProperty("refreshToken");
            apiTokens.UpdateToken(TokenTypes.LiveAccess, newAccessToken.ToString());
            apiTokens.UpdateToken(TokenTypes.LiveRefresh, newRefreshToken.ToString());
            return Ok(new { newAccessToken, newRefreshToken });
        }

        [HttpPost("RefreshCoreToken")]
        public async Task<ActionResult> RefreshCoreAccessToken()
        {
            var refreshToken = apiTokens.GetToken(TokenTypes.CoreRefresh);
            var newTokens = await nadeoTokenService.RefreshNadeoTokenAsync(refreshToken);
            var newAccessToken = newTokens.GetProperty("accessToken");
            var newRefreshToken = newTokens.GetProperty("refreshToken");
            apiTokens.UpdateToken(TokenTypes.CoreAccess, newAccessToken.ToString());
            apiTokens.UpdateToken(TokenTypes.CoreRefresh, newRefreshToken.ToString());
            return Ok(new { newAccessToken, newRefreshToken });
        }
    }
}
