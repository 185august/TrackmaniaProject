using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrackmaniaWebsiteAPI.Data;
using TrackmaniaWebsiteAPI.Models;
using TrackmaniaWebsiteAPI.Services;
using JsonElement = System.Text.Json.JsonElement;

namespace TrackmaniaWebsiteAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ApiTokensController(
        IConfiguration configuration,
        IApiTokensService nadeoTokenService,
        IApiTokensService apiTokens,
        ApiRequestQueue queue
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

            //var response = await client.SendAsync(request);
            var response = await queue.QueueRequest(
                    httpClient => httpClient.SendAsync(request));

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
            const string liveAccessAudience = "{ \"audience\": \"NadeoLiveServices\"}";

            var tokens = await apiTokens.RequestNadeoTokenAsync(liveAccessAudience);

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
            const string coreAccessAudience = "{ \"audience\": \"NadeoServices\" }";

            var tokens = await apiTokens.RequestNadeoTokenAsync(coreAccessAudience);

            var accessToken = tokens.GetProperty("accessToken");
            var refreshToken = tokens.GetProperty("refreshToken");
            apiTokens.UpdateToken(TokenTypes.CoreAccess, accessToken.ToString());
            apiTokens.UpdateToken(TokenTypes.CoreRefresh, refreshToken.ToString());
            return Ok(new { accessToken, refreshToken });
        }

        [HttpPost("RefreshLiveToken")]
        public async Task<ActionResult> RefreshLiveAccessToken()
        {
            var refreshToken = apiTokens.RetrieveTokenAsync(TokenTypes.LiveRefresh).ToString();
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Problem("Token is not valid");
            }
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
            var refreshToken = apiTokens.RetrieveTokenAsync(TokenTypes.CoreRefresh).ToString();
            var newTokens = await nadeoTokenService.RefreshNadeoTokenAsync(refreshToken);
            var newAccessToken = newTokens.GetProperty("accessToken");
            var newRefreshToken = newTokens.GetProperty("refreshToken");
            apiTokens.UpdateToken(TokenTypes.CoreAccess, newAccessToken.ToString());
            apiTokens.UpdateToken(TokenTypes.CoreRefresh, newRefreshToken.ToString());
            return Ok(new { newAccessToken, newRefreshToken });
        }
    }
}
