using System.Net.Http.Headers;
using System.Text;

namespace TrackmaniaWebsiteAPI.Services;

public class NadeoTokenService
{
    private static readonly HttpClient _client = new HttpClient();

    private readonly string _ubisoftEmail;
    private readonly string _ubisoftPassword;

    public NadeoTokenService(IConfiguration configuration)
    {
        _ubisoftEmail = configuration["UbisoftEmail"];
        _ubisoftPassword = configuration["UbisoftPassword"];
    }

    public async Task<string> GetNadeoToken()
    {
        if (IsNadeoTokenExpired() == true)
        {
            await RequestNewNadeoTokensAsync(_ubisoftEmail, _ubisoftPassword);
        }

        return "Token";
    }

    public bool IsNadeoTokenExpired()
    {
        return 
    }

    public async Task<string> RequestNewNadeoTokensAsync(string email, string password)
    {
        var requestUri = "https://public-ubiservices.ubi.com/v3/profiles/sessions";

        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

        request.Headers.Add("Ubi-AppId", "86263886-327a-4328-ac69-527f0d20a237");

        request.Headers.Add("User-Agent", $"TrackmaniaWebsiteAPI/{_ubisoftEmail}");

        var credentials = $"{_ubisoftEmail}:{_ubisoftPassword}";
        var credentialsBytes = Encoding.UTF8.GetBytes(credentials);
        var base64Credentials = Convert.ToBase64String(credentialsBytes);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);

        request.Content = new StringContent("", Encoding.UTF8, "application/json");

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();

        return responseBody;
    }
}
