using System.Globalization;
using Common.Services;

namespace Common.Tests;

public class TestDateTimeService : IDateTimeService
{
    private string dateStr = "2023-05-15";
    private string dateTimeStr = "2023-05-15 14:30:02";
    public DateTime Now
    {
        get
        {
            return DateTime.ParseExact(dateTimeStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);;
        }
    }

    public DateTime Today
    {
        get
        {
            return DateTime.Parse(dateStr, CultureInfo.InvariantCulture);
        }
    }
}