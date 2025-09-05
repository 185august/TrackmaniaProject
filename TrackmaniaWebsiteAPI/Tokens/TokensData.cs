namespace TrackmaniaWebsiteAPI.Tokens;

public class TokensData
{
    //For Json Serializer
    public string? UbisoftTicket { get; set; }
    public TokenData? LiveApiTokens { get; set; }
    public TokenData? CoreApiTokens { get; set; }
    public TokenData? OAuth2Tokens { get; set; }
}
