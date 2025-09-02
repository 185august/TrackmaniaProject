using System.Text;

namespace TrackmaniaWebsiteAPI.Services;

public class TimeCalculationService : ITimeCalculationService
{
    public double ConvertTimeInMsToMinutes(double time)
    {
        return time / 1000 ;
    }

    public double CalculateTimeDifference(double time1, double time2)
    {
        return Math.Max(time1, time2) - Math.Min(time1, time2);
    }

    public string FormatTime(int time)
    {
        StringBuilder sb = new();
        int hours = 0;
        int minutes = 0;
        int seconds = 0;
        int ms = 0;
        while (time>=3600000)
        {
            hours++;
            time -= 3600000;
        }

        while (time >= 60000)
        {
            minutes++;
            time -= 60000;
        }

        while (time>= 1000)
        {
            seconds++;
            time -= 1000;
        }

        ms = time;
        if (hours > 0)
        {
            if (hours < 10)
            {
                sb.Append($"0{hours}:");
            } else
            {
                sb.Append($"{hours}:");
            }
        }
        if (minutes < 10)
        {
            sb.Append($"0{minutes}:");
        } else
        {
            sb.Append($"{minutes}:");
        }

        if (seconds < 10)
        {
            sb.Append($"0{seconds}.");
        } else
        {
            sb.Append($"{seconds}.");
        }

        if (ms < 100)
        {
            sb.Append($"0{ms}");
        } else
        {
            sb.Append($"{ms}");
        }
        return sb.ToString();
    } 
}
