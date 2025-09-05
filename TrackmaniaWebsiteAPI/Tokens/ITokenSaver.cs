namespace TrackmaniaWebsiteAPI.Tokens;

public interface ITokenSaver
{
    void UpdateUbisoftTicket(string ubisoftTicket);
    void UpdateToken(TokenTypes tokenType, string newToken);
}
