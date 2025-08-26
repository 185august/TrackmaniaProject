namespace TrackmaniaWebsiteAPI.Services;

public class TimeCalculationService : ITimeCalculationService
{
    public double ConvertTimeInMsToMinutes(double time)
    {
        return time / 1000 / 60;
    }

    public double CalculateTimeDifference(double time1, double time2)
    {
        return Math.Max(time1, time2) - Math.Min(time1, time2);
    }
}
