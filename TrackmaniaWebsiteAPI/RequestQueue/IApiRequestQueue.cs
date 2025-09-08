namespace TrackmaniaWebsiteAPI.RequestQueue;

public interface IApiRequestQueue
{
    Task<HttpResponseMessage> QueueRequest(Func<HttpClient, Task<HttpResponseMessage>> requestFunc);
}
