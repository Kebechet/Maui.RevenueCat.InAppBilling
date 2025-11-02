using Java.Util;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class JavaDateExtensions
{
    private static readonly DateTime _epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    internal static DateTime? ToDateTime(this Date? date)
    {
        if (date is null)
        {
            return null;
        }

        return _epoch.AddMilliseconds(date.Time);
    }

    internal static DateTime ToDateTime(this long milliseconds)
    {
        return _epoch.AddMilliseconds(milliseconds);
    }
}
