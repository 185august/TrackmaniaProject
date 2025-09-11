using System.Net.Http.Headers;
using System.Text.Json;
using TrackmaniaWebsiteAPI.RequestQueue;
using TrackmaniaWebsiteAPI.Tokens;

namespace TrackmaniaWebsiteAPI.ApiHelper;

public class ApiHelperMethods(ITokenFetcher apiTokensService, IHttpService httpService)
    : IApiHelperMethods
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<HttpRequestMessage> CreateRequestWithAuthorization(
        TokenTypes tokenType,
        string requestUriValue
    )
    {
        string accessToken = await apiTokensService.RetrieveAccessTokenAsync(tokenType);

        var request = new HttpRequestMessage(HttpMethod.Get, requestUriValue);

        request.Headers.Authorization = new AuthenticationHeaderValue(
            "nadeo_v1",
            $"t={accessToken}"
        );
        return request;
    }

    public async Task<T?> SendRequestAndReturnDataTypeAsync<T>(HttpRequestMessage request)
    {
        return await httpService.SendRequestAsync<T>(request);
    }
}
