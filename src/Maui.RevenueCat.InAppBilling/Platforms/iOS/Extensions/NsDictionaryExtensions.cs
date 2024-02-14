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

    internal static string? ToJson<T, U>(this NSDictionary<T, U> dictionary)
        where T : NSObject
        where U : NSObject
    {
        if (dictionary.IsNullOrEmpty())
        {
            return null;
        }

        var jsonData = NSJsonSerialization.Serialize(dictionary, NSJsonWritingOptions.PrettyPrinted, out var error);

        return error is null
            ? NSString.FromData(jsonData, NSStringEncoding.UTF8)
            : null;
    }
}