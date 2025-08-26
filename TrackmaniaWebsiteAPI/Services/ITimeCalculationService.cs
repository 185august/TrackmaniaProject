namespace TrackmaniaWebsiteAPI.Services;

public interface ITimeCalculationService
{
    double ConvertTimeInMsToMinutes(double time);
    double CalculateTimeDifference(double time1, double time2);
}
