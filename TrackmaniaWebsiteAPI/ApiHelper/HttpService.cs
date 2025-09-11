using System.Text.Json;
using TrackmaniaWebsiteAPI.RequestQueue;

namespace TrackmaniaWebsiteAPI.ApiHelper;

public class HttpService(IApiRequestQueue queue) : IHttpService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<T?> SendRequestAsync<T>(HttpRequestMessage request)
    {
        var response = await queue.QueueRequest(client => client.SendAsync(request));

        response.EnsureSuccessStatusCode();

        string jsonString = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<T>(jsonString, _jsonSerializerOptions);
    }
}
