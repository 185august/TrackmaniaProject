namespace TrackmaniaWebsiteAPI.Tokens;

public class TokensData
{
    //For Json Serializer
    public string? UbisoftTicket { get; set; }
    public IndividualTokenData? LiveApiTokens { get; set; }
    public IndividualTokenData? CoreApiTokens { get; set; }
    public IndividualTokenData? OAuth2Tokens { get; set; }
}
