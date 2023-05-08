using Foundation;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;

internal static class NsDateExtensions
{
    internal static DateTime ToDateTime(this NSDate? date)
    {
        if (date is null)
        {
            return DateTime.MinValue;
        }

        return (DateTime)date;
    }
}
