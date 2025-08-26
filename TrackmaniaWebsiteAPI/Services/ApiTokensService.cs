using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TrackmaniaWebsiteAPI.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace TrackmaniaWebsiteAPI.Services;

public class ApiTokensService : IApiTokensService
{
    [JsonIgnore]
    private JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        IncludeFields = true,
    };

    [JsonIgnore]
    private readonly string _ubisoftEmail;

    [JsonIgnore]
    private readonly string _ubisoftPassword;

    [JsonIgnore]
    private readonly string? _identifier;

    [JsonIgnore]
    private readonly string? _secret;

    public string? UbisoftTicket { get; set; }

    public string? LiveApiAccessToken { get; set; }
    public string? LiveApiRefreshToken { get; set; }
    public DateTime LiveApiAccessTokenExpiresAt { get; set; }
    public DateTime LiveApiRefreshTokenExpiresAt { get; set; }

    public string? CoreApiAccessToken { get; set; }
    public string? CoreApiRefreshToken { get; set; }
    public DateTime CoreApiAccessTokenExpiresAt { get; set; }
    public DateTime CoreApiRefreshTokenExpiresAt { get; set; }

    public string? OAuth2AccessToken { get; set; }
    public DateTime OAuth2AccessTokenExpiresAt { get; set; }

    public ApiTokensService(IConfiguration configuration)
    {
        _ubisoftEmail = configuration?["UbisoftEmail"]!;
        _ubisoftPassword = configuration?["UbisoftPassword"]!;
        _identifier = configuration?["OAuth2Identifier"];
        _secret = configuration?["OAuth2Secret"];
    }

    [JsonConstructor]
    public ApiTokensService()
    {
        _ubisoftEmail = string.Empty;
        _ubisoftPassword = string.Empty;
        _identifier = string.Empty;
        _secret = string.Empty;
    }

    private const string TokensFilePath = "tokens.json";

    public async Task<string> RequestTicket()
    {
        var requestUri = "https://public-ubiservices.ubi.com/v3/profiles/sessions";

        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

        request.Headers.Add("Ubi-AppId", "86263886-327a-4328-ac69-527f0d20a237");
        request.Headers.UserAgent.Add(
            new ProductInfoHeaderValue("TrackmaniaWebsiteAPI", "School-project")
        );
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("(spaceboysurf@hotmail.com)"));

        var credentials = $"{_ubisoftEmail}:{_ubisoftPassword}";
        var credentialsBytes = Encoding.UTF8.GetBytes(credentials);
        var base64Credentials = Convert.ToBase64String(credentialsBytes);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);

        request.Content = new StringContent("", Encoding.UTF8, "application/json");

        using var client = new HttpClient();

        var response = await client.SendAsync(request);

        var responseBody = await response.Content.ReadAsStringAsync();

        var obj = JsonSerializer.Deserialize<JsonElement>(responseBody);

        var ticket = obj.GetProperty("ticket").GetString();
        UpdateUbisoftTicket(ticket);
        return ticket;
    }

    public async Task<JsonElement> RequestNadeoTokenAsync(string nadeoAudience)
    {
        string? ticket = await RequestTicket();

        string requestUri =
            "https://prod.trackmania.core.nadeo.online/v2/authentication/token/ubiservices";

        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

        request.Headers.Authorization = new AuthenticationHeaderValue("ubi_v1", $"t={ticket}");

        //request.Headers.Add("Ubi-AppId", "86263886-327a-4328-ac69-527f0d20a237");
        request.Headers.UserAgent.Add(
            new ProductInfoHeaderValue("TrackmaniaWebisiteAPI", "Schoolproject")
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

    private ApiTokensService GetTokens()
    {
        if (!File.Exists(TokensFilePath))
            return new ApiTokensService();
        string jsonString = File.ReadAllText(TokensFilePath);

        try
        {
            return JsonSerializer.Deserialize<ApiTokensService>(jsonString, _jsonSerializerOptions)
                ?? new ApiTokensService();
        }
        catch (Exception e)
        {
            Console.WriteLine($"JSON EXCEPTION: {e.Message} \n StackTrace: {e.StackTrace}");
            throw;
        }
    }

    private void SaveTokens(ApiTokensService tokens)
    {
        try
        {
            string jsonString = JsonSerializer.Serialize(tokens, _jsonSerializerOptions);
            File.WriteAllText(TokensFilePath, jsonString);
        }
        catch (Exception e)
        {
            Console.WriteLine($"JSON EXCEPTION {e.Message} \n STACK TRACE: {e.StackTrace}");
        }
    }

    public string? GetUbisoftTicket() => GetTokens().UbisoftTicket;

    public bool IsTokenExpired(TokenTypes tokenType, ApiTokensService tokens)
    {
        switch (tokenType)
        {
            case TokenTypes.LiveAccess:
                return tokens.LiveApiAccessTokenExpiresAt < DateTime.Now;
            case TokenTypes.LiveRefresh:
                return tokens.LiveApiRefreshTokenExpiresAt < DateTime.Now;
            case TokenTypes.CoreAccess:
                return tokens.CoreApiAccessTokenExpiresAt < DateTime.Now;
            case TokenTypes.CoreRefresh:
                return tokens.CoreApiRefreshTokenExpiresAt < DateTime.Now;
            case TokenTypes.OAuth2Access:
                return tokens.OAuth2AccessTokenExpiresAt < DateTime.Now;
            default:
                throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null);
        }
    }

    public async Task<string> RetrieveTokenAsync(TokenTypes tokenType)
    {
        var tokens = GetTokens();

        switch (tokenType)
        {
            case TokenTypes.LiveAccess:
            {
                if (IsTokenExpired(TokenTypes.LiveAccess, tokens))
                {
                    if (IsTokenExpired(TokenTypes.LiveRefresh, tokens))
                    {
                        const string liveAccessBody = "{ \"audience\": \"NadeoLiveServices\"}";
                        var newLiveTokens = await RequestNadeoTokenAsync(liveAccessBody);
                        var accessToken = newLiveTokens.GetProperty("accessToken");
                        var refreshToken = newLiveTokens.GetProperty("refreshToken");
                        UpdateToken(TokenTypes.LiveAccess, accessToken.ToString());
                        UpdateToken(TokenTypes.LiveRefresh, refreshToken.ToString());
                        return accessToken.ToString();
                    }
                    else
                    {
                        var newLiveTokens = await RefreshNadeoTokenAsync(
                            tokens.LiveApiRefreshToken
                        );
                        var accessToken = newLiveTokens.GetProperty("accessToken");
                        var refreshToken = newLiveTokens.GetProperty("refreshToken");
                        UpdateToken(TokenTypes.LiveAccess, accessToken.ToString());
                        UpdateToken(TokenTypes.LiveRefresh, refreshToken.ToString());
                        return accessToken.ToString();
                    }
                }
                if (tokens.LiveApiAccessToken != null)
                    return tokens.LiveApiAccessToken;
                break;
            }
            case TokenTypes.CoreAccess:
            {
                if (IsTokenExpired(TokenTypes.CoreAccess, tokens))
                {
                    if (IsTokenExpired(TokenTypes.CoreRefresh, tokens))
                    {
                        const string coreAccessBody = "{ \"audience\": \"NadeoServices\"}";
                        var newCoreTokens = await RequestNadeoTokenAsync(coreAccessBody);
                        var accessToken = newCoreTokens.GetProperty("accessToken");
                        var refreshToken = newCoreTokens.GetProperty("refreshToken");
                        UpdateToken(TokenTypes.CoreAccess, accessToken.ToString());
                        UpdateToken(TokenTypes.CoreRefresh, refreshToken.ToString());
                        return accessToken.ToString();
                    }
                    else
                    {
                        var newLiveTokens = await RefreshNadeoTokenAsync(
                            tokens.CoreApiRefreshToken
                        );
                        var accessToken = newLiveTokens.GetProperty("accessToken");
                        var refreshToken = newLiveTokens.GetProperty("refreshToken");
                        UpdateToken(TokenTypes.CoreAccess, accessToken.ToString());
                        UpdateToken(TokenTypes.CoreRefresh, refreshToken.ToString());
                        return accessToken.ToString();
                    }
                }
                if (tokens.CoreApiAccessToken != null)
                    return tokens.CoreApiAccessToken;
                break;
            }
            case TokenTypes.OAuth2Access:
                if (IsTokenExpired(TokenTypes.OAuth2Access, tokens))
                {
                    string accessToken = await AcquireOAuthToken();
                    return accessToken;
                }
                return tokens.OAuth2AccessToken;
        }
        return "No valid tokens";
    }

    public void UpdateUbisoftTicket(string ubisoftTicket)
    {
        var tokens = GetTokens();
        tokens.UbisoftTicket = ubisoftTicket;
        SaveTokens(tokens);
    }

    private async Task<string> AcquireOAuthToken()
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
            return $"{response.StatusCode}, error = {body}";
        }

        var obj = JsonSerializer.Deserialize<JsonElement>(body);

        string? accessToken = obj.GetProperty("access_token").GetString();

        if (accessToken is null)
            return "No valid access token";

        UpdateToken(TokenTypes.OAuth2Access, accessToken);
        return accessToken;
    }

    public void UpdateToken(TokenTypes tokenType, string newToken)
    {
        var tokens = GetTokens();

        switch (tokenType)
        {
            case TokenTypes.LiveAccess:
                tokens.LiveApiAccessToken = newToken;
                tokens.LiveApiAccessTokenExpiresAt = DateTime.Now.AddHours(1);
                break;
            case TokenTypes.LiveRefresh:
                tokens.LiveApiRefreshToken = newToken;
                tokens.LiveApiRefreshTokenExpiresAt = DateTime.Now.AddDays(1);
                break;
            case TokenTypes.CoreAccess:
                tokens.CoreApiAccessToken = newToken;
                tokens.CoreApiAccessTokenExpiresAt = DateTime.Now.AddHours(1);
                break;
            case TokenTypes.CoreRefresh:
                tokens.CoreApiRefreshToken = newToken;
                tokens.CoreApiRefreshTokenExpiresAt = DateTime.Now.AddDays(1);
                break;
            case TokenTypes.OAuth2Access:
                tokens.OAuth2AccessToken = newToken;
                tokens.OAuth2AccessTokenExpiresAt = DateTime.Now.AddHours(1);
                break;
        }
        SaveTokens(tokens);
    }
}
