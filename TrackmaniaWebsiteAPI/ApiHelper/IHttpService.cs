namespace TrackmaniaWebsiteAPI.ApiHelper;

public interface IHttpService
{
    Task<T?> SendRequestAsync<T>(HttpRequestMessage request);
}
