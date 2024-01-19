using Java.Util;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class JavaDateExtensions
{
    internal static DateTime? ToDateTime(this Date? date)
    {
        if (date is null)
        {
            return null;
        }

        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return epoch.AddMilliseconds(date.Time);
    }
}
