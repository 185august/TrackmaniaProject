using System.Text;

namespace TrackmaniaWebsiteAPI.MapTimes;

public class TimeCalculationService : ITimeCalculationService
{
    public int CalculateTimeDifferenceWr(int time1, int time2)
    {
        return Math.Max(time1, time2) - Math.Min(time1, time2);
    }

    public string FormatTime(int time)
    {
        var timeSpan = TimeSpan.FromMilliseconds(time);
        return timeSpan.ToString(timeSpan.TotalHours >= 1 ? @"hh\:mm\:ss\.fff" : @"mm\:ss\.fff");
    }
}
