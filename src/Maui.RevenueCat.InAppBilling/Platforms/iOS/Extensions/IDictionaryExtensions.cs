using Foundation;

namespace Maui.RevenueCat.Platforms.iOS.Extensions;

internal static class IDictionaryExtensions
{
    internal static NSDictionary<NSString, NSString> ToNSDictionary(this IDictionary<string, string> dictionary)
    {
        if (dictionary is null || !dictionary.Any())
        {
            return new NSDictionary<NSString, NSString>();
        }

        var nsDictionary = new NSMutableDictionary<NSString, NSString>();
        foreach (var kvp in dictionary)
        {
            nsDictionary.Add(new NSString(kvp.Key), new NSString(kvp.Value));
        }

        return NSDictionary<NSString, NSString>.FromObjectsAndKeys(
            nsDictionary.Values.ToArray(),
            nsDictionary.Keys.ToArray(),
            (nint)nsDictionary.Count
        );
    }
}
