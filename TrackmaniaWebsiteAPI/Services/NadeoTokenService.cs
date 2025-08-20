using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TrackmaniaWebsiteAPI.Services;

public class NadeoTokenService(IConfiguration configuration) : INadeoTokenService
{
    private readonly string _ubisoftEmail = configuration["UbisoftEmail"]!;
    private readonly string _ubisoftPassword = configuration["UbisoftPassword"]!;

    public async Task<string> GetNadeoToken()
    {
        if (IsNadeoTokenExpired() == true) { }

        return "Token";
    }

    public bool IsNadeoTokenExpired()
    {
        return true;
    }

    /*
    public async Task<string> RequestUbisoftTicketAsync()
    {
         string ubisoftEmail = configuration["UbisoftEmail"]!;
            string ubisoftPassword = configuration["UbisoftPassword"]!;
            var requestUri = "https://public-ubiservices.ubi.com/v3/profiles/sessions";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            request.Headers.Add("Ubi-AppId", "86263886-327a-4328-ac69-527f0d20a237");
            request.Headers.UserAgent.Add(
                new ProductInfoHeaderValue("TrackmaniaWebisiteAPI", "Schoolproject")
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
    }*/
    public async Task<JsonElement> RequestUbisoftTicketAsync(string ticket, string jsonBody)
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

        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        request.Content = content;

        using var client = new HttpClient();

        var repsonse = await client.SendAsync(request);

        var responseBody = await repsonse.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<JsonElement>(responseBody);
    }
}
