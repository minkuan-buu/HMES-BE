namespace HMES.Business.Utilities.TimeZoneHelper;

public static class TimeZoneHelper
{
    private static readonly TimeZoneInfo HoChiMinhTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    public static DateTime GetCurrentHoChiMinhTime()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, HoChiMinhTimeZone);
    }

    public static DateTime ConvertToHoChiMinhTime(DateTime utcDateTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, HoChiMinhTimeZone);
    } 
}