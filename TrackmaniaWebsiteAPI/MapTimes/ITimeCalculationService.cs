namespace TrackmaniaWebsiteAPI.MapTimes;

public interface ITimeCalculationService
{
    int CalculateTimeDifferenceWr(int time1, int time2);
    string FormatTime(int time);
}
