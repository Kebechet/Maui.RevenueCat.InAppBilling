using Foundation;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;

public static class NsDictionaryExtensions
{
    public static bool IsNullOrEmpty<T, U>(this NSDictionary<T, U> dictionary)
        where T : NSObject
        where U : NSObject
    {
        return dictionary is null || dictionary.Count == 0;
    }
}