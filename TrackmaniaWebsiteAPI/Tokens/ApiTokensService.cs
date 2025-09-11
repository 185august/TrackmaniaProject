using Microsoft.DotNet.Scaffolding.Shared;

namespace TrackmaniaWebsiteAPI.Tokens;

using System.IO.Abstractions;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using RequestQueue;
using JsonSerializer = System.Text.Json.JsonSerializer;

public class ApiTokensService : ITokenFetcher
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        IncludeFields = true,
    };

    private readonly string _ubisoftEmail;
    private readonly string _ubisoftPassword;
    private readonly string _identifier;
    private readonly string _secret;
    private readonly string _tokensFilePath;

    private readonly IHttpClientFactory _httpClient;
    private readonly IFileSystem _fileSystem;

    private readonly IApiRequestQueue? _queue;
    private TokensData _inMemoryTokens;

    public ApiTokensService(
        IConfiguration configuration,
        IApiRequestQueue queue,
        IHttpClientFactory httpClient
    )
    {
        _ubisoftEmail = configuration["UbisoftEmail"]!;
        _ubisoftPassword = configuration["UbisoftPassword"]!;
        _identifier = configuration["OAuth2Identifier"]!;
        _secret = configuration["OAuth2Secret"]!;
        _tokensFilePath = configuration["TokensFilePath"] ?? "tokens.json";
        _queue = queue;
        _inMemoryTokens = GetTokensFromFile();
        _httpClient = httpClient;
    }

    private async Task<string> RequestTicket()
    {
        const string requestUri = "https://public-ubiservices.ubi.com/v3/profiles/sessions";

        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

        request.Headers.Add("Ubi-AppId", "86263886-327a-4328-ac69-527f0d20a237");
        request.Headers.UserAgent.Add(
            new ProductInfoHeaderValue("TrackmaniaWebsiteAPI", "School-project")
        );
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue($"({_ubisoftEmail})"));

        string credentials = $"{_ubisoftEmail}:{_ubisoftPassword}";
        byte[] credentialsBytes = Encoding.UTF8.GetBytes(credentials);
        string base64Credentials = Convert.ToBase64String(credentialsBytes);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);

        request.Content = new StringContent("", Encoding.UTF8, "application/json");

        if (_queue is null)
        {
            throw new InvalidOperationException("_queue is not initialized");
        }
        var response = await _queue.QueueRequest(httpClient => httpClient.SendAsync(request));

        string responseBody = await response.Content.ReadAsStringAsync();
        try
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(responseBody);

            string ticket = obj.GetProperty("ticket").GetString()!;

            _inMemoryTokens.UbisoftTicket = ticket;
            SaveTokensToFile(_inMemoryTokens);
            return ticket;
        }
        catch (JsonException e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    private async Task<JsonElement> RequestNadeoTokenAsync(string nadeoAudience)
    {
        string ticket = await RequestTicket();

        const string requestUri =
            "https://prod.trackmania.core.nadeo.online/v2/authentication/token/ubiservices";

        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

        request.Headers.Authorization = new AuthenticationHeaderValue("ubi_v1", $"t={ticket}");

        request.Headers.UserAgent.Add(
            new ProductInfoHeaderValue("TrackmaniaWebisiteAPI", "Schoolproject")
        );

        var payload = new { audience = nadeoAudience };
        string jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        request.Content = content;

        if (_queue is null)
            throw new InvalidOperationException("_queue is not initialized");

        var response = await _queue.QueueRequest(httpClient => httpClient.SendAsync(request));

        string responseBody = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<JsonElement>(responseBody);
    }

    private async Task<IndividualTokenData> RefreshNadeoTokenAsync(string refreshToken)
    {
        const string requestUri =
            "https://prod.trackmania.core.nadeo.online/v2/authentication/token/refresh";
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "nadeo_v1",
            $"t={refreshToken}"
        );
        if (_queue is null)
        {
            throw new InvalidOperationException("_queue is not initialized");
        }
        var response = await _queue.QueueRequest(httpClient => httpClient.SendAsync(request));

        string responseBody = await response.Content.ReadAsStringAsync();
        return ExtractTokenDataFromJson(JsonSerializer.Deserialize<JsonElement>(responseBody));
    }

    public async Task<string> RetrieveAccessTokenAsync(TokenTypes tokenType)
    {
        return tokenType switch
        {
            TokenTypes.Live => await GetNadeoAccessTokenAsync("NadeoLiveServices"),
            TokenTypes.Core => await GetNadeoAccessTokenAsync("NadeoServices"),
            TokenTypes.OAuth => await GetOauthAccessTokenAsync(),
            _ => throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null),
        };
    }

    private TokensData GetTokensFromFile()
    {
        if (!File.Exists(_tokensFilePath))
            return new TokensData();
        string jsonString = File.ReadAllText(_tokensFilePath);

        return JsonSerializer.Deserialize<TokensData>(jsonString, _jsonSerializerOptions)
            ?? new TokensData();
    }

    private void SaveTokensToFile(TokensData tokens)
    {
        string jsonString = JsonSerializer.Serialize(tokens, _jsonSerializerOptions);
        File.WriteAllText(_tokensFilePath, jsonString);
    }

    private static bool IsTokenTimeExpired(DateTime? time)
    {
        return time < DateTime.Now || time is null;
    }

    private static IndividualTokenData ExtractTokenDataFromJson(JsonElement json)
    {
        return new IndividualTokenData(
            AccessToken: json.GetProperty("accessToken").GetString()!,
            RefreshToken: json.GetProperty("refreshToken").GetString()!,
            AccessExpiresAt: DateTime.Now.AddHours(1),
            RefreshExpiresAt: DateTime.Now.AddDays(1)
        );
    }

    private async Task<string> GetNadeoAccessTokenAsync(string audience)
    {
        var tokens = _inMemoryTokens;
        var currentTokens =
            (audience == "NadeoLiveServices") ? tokens.LiveApiTokens : tokens.CoreApiTokens;
        var tokenType = (audience == "NadeoLiveServices") ? TokenTypes.Live : TokenTypes.Core;

        if (currentTokens is not null && !IsTokenTimeExpired(currentTokens.AccessExpiresAt))
            return currentTokens.AccessToken;
        if (currentTokens is null || IsTokenTimeExpired(currentTokens.RefreshExpiresAt))
        {
            var newTokensJson = await RequestNadeoTokenAsync(audience);
            currentTokens = ExtractTokenDataFromJson(newTokensJson);
        }
        else
        {
            currentTokens = await RefreshNadeoTokenAsync(currentTokens.RefreshToken!);
        }
        UpdateAndSaveTokens(tokenType, currentTokens);
        return currentTokens.AccessToken;
    }

    private async Task<string> GetOauthAccessTokenAsync()
    {
        var tokens = _inMemoryTokens;
        if (
            tokens.OAuth2Tokens is not null
            && !IsTokenTimeExpired(tokens.OAuth2Tokens.AccessExpiresAt)
        )
        {
            return tokens.OAuth2Tokens.AccessToken;
        }
        return await AcquireOAuthToken();
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
        var requestUri = "https://api.trackmania.com/api/access_token";
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Content = content;

        var response = await _queue.QueueRequest(httpClient => httpClient.SendAsync(request));
        // var client = _httpClient.CreateClient();
        //
        // var response = await client.PostAsync(
        //     "https://api.trackmania.com/api/access_token",
        //     content
        // );

        string body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            return $"{response.StatusCode}, error = {body}";
        }

        var obj = JsonSerializer.Deserialize<JsonElement>(body);
        var tokenData = new IndividualTokenData(
            AccessToken: obj.GetProperty("access_token").GetString()!,
            AccessExpiresAt: DateTime.Now.AddHours(1),
            RefreshToken: null,
            RefreshExpiresAt: null
        );

        UpdateAndSaveTokens(TokenTypes.OAuth, tokenData);
        return tokenData.AccessToken;
    }

    private void UpdateAndSaveTokens(
        TokenTypes tokenType,
        IndividualTokenData newIndividualTokenData
    )
    {
        switch (tokenType)
        {
            case TokenTypes.Live:
                _inMemoryTokens.LiveApiTokens = newIndividualTokenData;
                break;
            case TokenTypes.Core:
                _inMemoryTokens.CoreApiTokens = newIndividualTokenData;
                break;
            case TokenTypes.OAuth:
                _inMemoryTokens.OAuth2Tokens = newIndividualTokenData;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null);
        }
        SaveTokensToFile(_inMemoryTokens);
    }
}

public enum TokenTypes
{
    Live,
    Core,
    OAuth,
}
