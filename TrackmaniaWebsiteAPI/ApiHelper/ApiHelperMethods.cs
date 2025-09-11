using System.Net.Http.Headers;
using TrackmaniaWebsiteAPI.Tokens;

namespace TrackmaniaWebsiteAPI.ApiHelper;

public class ApiHelperMethods(ITokenFetcher apiTokensService, IHttpService httpService)
    : IApiHelperMethods
{
    public async Task<HttpRequestMessage> CreateRequestWithAuthorization(
        TokenTypes tokenType,
        string requestUriValue,
        AuthorizationHeaderValue authorizationHeaderValue = AuthorizationHeaderValue.Nadeo
    )
    {
        string accessToken = await apiTokensService.RetrieveAccessTokenAsync(tokenType);

        var request = new HttpRequestMessage(HttpMethod.Get, requestUriValue);
        if (authorizationHeaderValue == AuthorizationHeaderValue.Nadeo)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "nadeo_v1",
                $"t={accessToken}"
            );
        }
        else
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return request;
    }

    public async Task<T?> SendRequestAsync<T>(HttpRequestMessage request)
    {
        return await httpService.SendRequestAsync<T>(request);
    }
}
