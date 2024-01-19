using Foundation;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;

internal static class NsSetExtensions
{
    public static List<string> ToStringList(this NSSet<NSString> setCollection)
    {
        var list = new List<string>();
        foreach (var item in setCollection)
        {
            list.Add(item.ToString());
        }

        return list;
    }
}
