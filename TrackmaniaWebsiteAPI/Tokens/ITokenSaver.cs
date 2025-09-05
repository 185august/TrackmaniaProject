namespace TrackmaniaWebsiteAPI.Tokens;

public interface ITokenSaver
{
    void UpdateAndSaveTokens(TokenTypesNew tokenType, TokenData newTokenData);
}
