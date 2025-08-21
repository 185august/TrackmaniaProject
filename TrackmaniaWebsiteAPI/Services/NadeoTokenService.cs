using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TrackmaniaWebsiteAPI.Services;

public class NadeoTokenService(IConfiguration configuration) : INadeoTokenService
{
    private readonly string _ubisoftEmail = configuration["UbisoftEmail"]!;
    private readonly string _ubisoftPassword = configuration["UbisoftPassword"]!;

    public async Task<JsonElement> RequestNadeoTokenAsync(string ticket, string nadeoAudience)
    {
        string requestUri =
            "https://prod.trackmania.core.nadeo.online/v2/authentication/token/ubiservices";

        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

        request.Headers.Authorization = new AuthenticationHeaderValue("ubi_v1", $"t={ticket}");

        //request.Headers.Add("Ubi-AppId", "86263886-327a-4328-ac69-527f0d20a237");
        request.Headers.UserAgent.Add(
            new System.Net.Http.Headers.ProductInfoHeaderValue(
                "TrackmaniaWebisiteAPI",
                "Schoolproject"
            )
        );

        var content = new StringContent(nadeoAudience, Encoding.UTF8, "application/json");
        request.Content = content;

        using var client = new HttpClient();

        var repsonse = await client.SendAsync(request);

        var responseBody = await repsonse.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<JsonElement>(responseBody);
    }

    public async Task<JsonElement> RefreshNadeoTokenAsync(string refreshToken)
    {
        string requestUri =
            "https://prod.trackmania.core.nadeo.online/v2/authentication/token/refresh";
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "nadeo_v1",
            $"t={refreshToken}"
        );
        var client = new HttpClient();
        var response = await client.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(responseBody);
    }
}
