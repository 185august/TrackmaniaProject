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

    private const string TokensFilePath = "tokens.json";

    private ApiTokensService GetTokens()
    {
        if (!File.Exists(TokensFilePath))
            return new ApiTokensService();
        string jsonString = File.ReadAllText(TokensFilePath);
        return JsonSerializer.Deserialize<ApiTokensService>(jsonString, _jsonSerializerOptions)
            ?? new ApiTokensService();
    }

    private void SaveTokens(ApiTokensService tokens)
    {
        string jsonString = JsonSerializer.Serialize(tokens, _jsonSerializerOptions);
        File.WriteAllText(TokensFilePath, jsonString);
    }

    public string? GetUbisoftTicket() => GetTokens().UbisoftTicket;

    public string? GetToken(TokenTypes tokenType)
    {
        var tokens = GetTokens();
        switch (tokenType)
        {
            case TokenTypes.LiveAccess:

                if (tokens.LiveApiAccessTokenExpiresAt > DateTime.Now)
                {
                    return tokens.LiveApiAccessToken;
                }
                else
                {
                    //Call to get new tokens
                }
                break;
            case TokenTypes.LiveRefresh:
                if (tokens.LiveApiRefreshTokenExpiresAt > DateTime.Now)
                {
                    return tokens.LiveApiRefreshToken;
                }
                else
                {
                    //Call to get new tokens
                }
                break;
            case TokenTypes.CoreAccess:
                if (tokens.CoreApiAccessTokenExpiresAt > DateTime.Now)
                {
                    return tokens.CoreApiAccessToken;
                }
                else
                {
                    //Call to get new tokens
                }
                break;
            case TokenTypes.CoreRefresh:
                if (tokens.CoreApiAccessTokenExpiresAt > DateTime.Now)
                {
                    return tokens.CoreApiRefreshToken;
                }
                else
                {
                    //Call to get new tokens
                }
                break;
            case TokenTypes.OAuth2Access:
                if (tokens.OAuth2AccessTokenExpiresAt > DateTime.Now)
                {
                    return tokens.OAuth2AccessToken;
                }
                else
                {
                    //Call to refresh token
                }
                break;
        }

        return "No valid tokens";
    }

    public void UpdateUbisoftTicket(string ubisoftTicket)
    {
        var tokens = GetTokens();
        tokens.UbisoftTicket = ubisoftTicket;
        SaveTokens(tokens);
    }

    public void UpdateToken(TokenTypes tokenType, string newToken)
    {
        var tokens = GetTokens();

        switch (tokenType)
        {
            case TokenTypes.LiveAccess:
                tokens.LiveApiAccessToken = newToken;
                tokens.LiveApiAccessTokenExpiresAt = DateTime.Now.AddHours(1);
                Console.WriteLine(tokens.LiveApiAccessTokenExpiresAt);
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
