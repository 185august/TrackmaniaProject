namespace TrackmaniaWebsiteAPI.Services;

public class ApiRequestQueue
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private DateTime _lastRequestTime = DateTime.MinValue;
    private readonly TimeSpan _minDelay = TimeSpan.FromMilliseconds(600); // ~1.6 req/s
    private readonly HttpClient _httpClient;

    public ApiRequestQueue(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

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
            var response = await reqFn(_httpClient);
            _lastRequestTime = DateTime.UtcNow;
            return response;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // private readonly ApiRequestQueue _queue;
    //
    // public TrackmaniaService(ApiRequestQueue queue)
    // {
    //     _queue = queue;
    // }
    //
    // public async Task<string> GetLeaderboard(string mapUid)
    // {
    //     var response = await _queue.QueueRequest(http => http.GetAsync($"url"));
    //     response.EnsureSuccessStatusCode();
    //     return await response.Content.ReadAsStringAsync();
    // }
}
