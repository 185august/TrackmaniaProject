namespace TrackmaniaWebsiteAPI.Tokens;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TrackmaniaWebsiteAPI.RequestQueue;
using JsonSerializer = System.Text.Json.JsonSerializer;

public class ApiTokensServiceRefactor : ITokenFetcher, ITokenRefresher, ITokenSaver
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
    private readonly string _identifier;

    [JsonIgnore]
    private readonly string _secret;

    public string UbisoftTicket { get; set; }

    public TokenData? LiveApiTokens { get; set; }
    public TokenData? CoreApiTokens { get; set; }

    public string OAuth2AccessToken { get; set; }
    public DateTime OAuth2AccessTokenExpiresAt { get; set; }

    [JsonIgnore]
    private readonly ApiRequestQueue? _queue;

    public ApiTokensServiceRefactor(IConfiguration configuration, ApiRequestQueue queue)
    {
        _ubisoftEmail = configuration["UbisoftEmail"]!;
        _ubisoftPassword = configuration["UbisoftPassword"]!;
        _identifier = configuration["OAuth2Identifier"]!;
        _secret = configuration["OAuth2Secret"]!;
        _queue = queue;
    }

    [JsonConstructor]
    public ApiTokensServiceRefactor()
    {
        _ubisoftEmail = string.Empty;
        _ubisoftPassword = string.Empty;
        _identifier = string.Empty;
        _secret = string.Empty;
    }

    private const string TokensFilePath = "tokens2.json";

    public async Task<string> RequestTicket()
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

        using var client = new HttpClient();

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

            UpdateUbisoftTicket(ticket);
            return ticket;
        }
        catch (JsonException e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    public async Task<JsonElement> RequestNadeoTokenAsync(string nadeoAudience)
    {
        string ticket = await RequestTicket();

        const string requestUri =
            "https://prod.trackmania.core.nadeo.online/v2/authentication/token/ubiservices";

        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

        request.Headers.Authorization = new AuthenticationHeaderValue("ubi_v1", $"t={ticket}");

        request.Headers.UserAgent.Add(
            new ProductInfoHeaderValue("TrackmaniaWebisiteAPI", "Schoolproject")
        );

        var content = new StringContent(nadeoAudience, Encoding.UTF8, "application/json");
        request.Content = content;

        if (_queue is null)
        {
            throw new InvalidOperationException("_queue is not initialized");
        }

        var response = await _queue.QueueRequest(httpClient => httpClient.SendAsync(request));

        var responseBody = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<JsonElement>(responseBody);
    }

    public async Task<JsonElement> RefreshNadeoTokenAsync(string refreshToken)
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

        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(responseBody);
    }

    private ApiTokensServiceRefactor GetTokens()
    {
        if (!File.Exists(TokensFilePath))
            return new ApiTokensServiceRefactor();
        string jsonString = File.ReadAllText(TokensFilePath);

        try
        {
            return JsonSerializer.Deserialize<ApiTokensServiceRefactor>(
                    jsonString,
                    _jsonSerializerOptions
                ) ?? new ApiTokensServiceRefactor();
        }
        catch (Exception e)
        {
            Console.WriteLine($"JSON EXCEPTION: {e.Message} \n StackTrace: {e.StackTrace}");
            throw;
        }
    }

    private void SaveTokens(ApiTokensServiceRefactor tokens)
    {
        try
        {
            var tokenPath = "tokens2.json";
            string jsonString = JsonSerializer.Serialize(tokens, _jsonSerializerOptions);
            File.WriteAllText(tokenPath, jsonString);
        }
        catch (Exception e)
        {
            Console.WriteLine($"JSON EXCEPTION {e.Message} \n STACK TRACE: {e.StackTrace}");
        }
    }

    public bool IsTokenExpired(DateTime? time)
    {
        return time < DateTime.Now;
        // return tokenType switch
        // {
        //     TokenTypesNew.Live => tokens.LiveApiTokens.AccessExpiresAt < DateTime.Now,
        //     TokenTypes.LiveRefresh => tokens.LiveApiTokens.RefreshExpiresAt < DateTime.Now,
        //     TokenTypes.CoreAccess => tokens.CoreApiTokens.AccessExpiresAt < DateTime.Now,
        //     TokenTypes.CoreRefresh => tokens.CoreApiTokens.RefreshExpiresAt < DateTime.Now,
        //     TokenTypes.OAuth2Access => tokens.OAuth2AccessTokenExpiresAt < DateTime.Now,
        //     _ => throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null),
        // };
    }

    private async Task<TokenData> GetNewNadeoTokens(string audience, TokenData token)
    {
        var newLiveTokens = await RequestNadeoTokenAsync(audience);
        var newTokenData = new TokenData(
            AccessToken: newLiveTokens.GetProperty("accessToken").GetString()!,
            RefreshToken: newLiveTokens.GetProperty("refreshToken").GetString()!,
            AccessExpiresAt: DateTime.Now.AddHours(1),
            RefreshExpiresAt: DateTime.Now.AddDays(1)
        );
        return newTokenData;
    }

    public async Task<string> RetrieveTokenAsync(TokenTypesNew tokenType)
    {
        var tokens = GetTokens();

        switch (tokenType)
        {
            case TokenTypesNew.Live:
            {
                if (!IsTokenExpired(tokens.LiveApiTokens.AccessExpiresAt))
                {
                    return tokens.LiveApiTokens.AccessToken;
                    // if (IsTokenExpired(tokens.LiveApiTokens.RefreshExpiresAt))
                    // {
                    //     const string liveAccessBody = "{ \"audience\": \"NadeoLiveServices\"}";
                    //     await GetNewNadeoTokens(liveAccessBody, LiveApiTokens);
                    //     var newLiveTokens = await RequestNadeoTokenAsync(liveAccessBody);
                    //     var accessToken = newLiveTokens.GetProperty("accessToken");
                    //     var refreshToken = newLiveTokens.GetProperty("refreshToken");
                    //     UpdateToken(TokenTypes.LiveAccess, accessToken.ToString());
                    //     UpdateToken(TokenTypes.LiveRefresh, refreshToken.ToString());
                    //     return accessToken.ToString();
                    // }
                    // else
                    // {
                    //     var newLiveTokens = await RefreshNadeoTokenAsync(
                    //         tokens.LiveApiRefreshToken
                    //     );
                    //     var accessToken = newLiveTokens.GetProperty("accessToken");
                    //     var refreshToken = newLiveTokens.GetProperty("refreshToken");
                    //     UpdateToken(TokenTypes.LiveAccess, accessToken.ToString());
                    //     UpdateToken(TokenTypes.LiveRefresh, refreshToken.ToString());
                    //     return accessToken.ToString();
                    // }
                }

                if (IsTokenExpired(tokens.LiveApiTokens.RefreshExpiresAt))
                {
                    return await GetNewNadeoTokens()
                }
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
            default:
                throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null);
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
        var tokenData = new TokenData(
            AccessToken: obj.GetProperty("access_token").GetString()!,
            AccessExpiresAt: DateTime.Now.AddHours(1),
            RefreshToken: null,
            RefreshExpiresAt: null
        );

        UpdateToken(TokenTypesNew.OAuth, tokenData);
        return tokenData.AccessToken;
    }

    public void UpdateToken(TokenTypesNew tokenType, TokenData newTokenData)
    {
        var tokens = GetTokens();

        switch (tokenType)
        {
            case TokenTypesNew.Live:
                tokens.LiveApiTokens = newTokenData;
                break;
            case TokenTypesNew.Core:
                tokens.CoreApiTokens = newTokenData;
                break;
            case TokenTypesNew.OAuth:
                tokens.OAuth2AccessToken = newTokenData.AccessToken;
                tokens.OAuth2AccessTokenExpiresAt = newTokenData.AccessExpiresAt;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType, null);
        }
        SaveTokens(tokens);
    }
}
