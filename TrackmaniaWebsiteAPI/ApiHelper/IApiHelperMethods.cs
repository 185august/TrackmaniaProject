using TrackmaniaWebsiteAPI.Tokens;

namespace TrackmaniaWebsiteAPI.ApiHelper;

public interface IApiHelperMethods
{
    Task<HttpRequestMessage> CreateRequestWithAuthorization(
        TokenTypes tokenType,
        string requestUriValue
    );
}
