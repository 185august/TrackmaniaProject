namespace TrackmaniaWebsiteAPI.RequestQueue;

public class ApiRequestQueue(HttpClient httpClient) : IApiRequestQueue
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private DateTime _lastRequestTime = DateTime.MinValue;
    private readonly TimeSpan _minDelay = TimeSpan.FromMilliseconds(600); // ~1.6 req/s

    public async Task<HttpResponseMessage> QueueRequest(
        Func<HttpClient, Task<HttpResponseMessage>> reqFn
    )
    {
        await _semaphore.WaitAsync();
        try
        {
            var elapsed = DateTime.UtcNow - _lastRequestTime;
            if (elapsed < _minDelay)
                await Task.Delay(_minDelay - elapsed);
            var response = await reqFn(httpClient);
            _lastRequestTime = DateTime.UtcNow;
            return response;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
