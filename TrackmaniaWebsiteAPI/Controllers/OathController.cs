using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace TrackmaniaWebsiteAPI.Controllers;

public class OathController : ControllerBase
{
    private readonly string redirect_uri = "https://localhost:7068/signin-trackmania";
    private readonly string identifier = "7e8231f3dda692c747ae";
    private readonly string secret = "c0401f990ffd0bc034624964d952ec51bd89de62"; //ikke la denne ligge her kek

    [HttpGet("login")]
    public IActionResult Login()
    {
        var state = Guid.NewGuid().ToString("N");
        HttpContext.Session.SetString("oauth_state", state);

        var url =
            $"https://api.trackmania.com/oauth/authorize"
            + $"?response_type=code"
            + $"&client_id={Uri.EscapeDataString(identifier)}"
            + $"&redirect_uri={Uri.EscapeDataString(redirect_uri)}"
            + $"&scope={Uri.EscapeDataString("read_favorite clubs")}"
            + $"&state={state}";

        return Redirect(url);
    }

    [HttpGet("signin-trackmania")]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
    {
        var expectedState = HttpContext.Session.GetString("oauth_state");
        if (state != expectedState)
            return BadRequest("Invalid OAuth ellerno");

        var content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = identifier,
                ["client_secret"] = secret,
                ["code"] = code,
                ["redirect_uri"] = redirect_uri,
            }
        );

        using var http = new HttpClient();

        var response = await http.PostAsync("https://api.trackmania.com/api/access_token", content);
        string body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            return BadRequest(new { status = response.StatusCode, error = body });

        //var json = await response.Content.ReadAsStringAsync();
        var obj = JsonSerializer.Deserialize<JsonElement>(body);

        var accessToken = obj.GetProperty("access_token").GetString();
        var expiresIn = obj.GetProperty("expires_in").GetInt64();
        var refreshToken = obj.GetProperty("refresh_token").GetString();

        HttpContext.Session.SetString("access_token", accessToken);
        HttpContext.Session.SetString("refresh_token", refreshToken);

        //Console.WriteLine("Token expires in " + expiresIn / 60 + " minutes");
        return Ok(new { accessToken, refreshToken });
    }
}
