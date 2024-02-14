using System.Collections;
using Maui.RevenueCat.InAppBilling.Extensions;
using Org.Json;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class IDictonaryExtensions
{
    internal static string? ToJson<T, U>(this IDictionary<T, U> dictionary)
    {
        return dictionary.IsNullOrEmpty() ? null : new JSONObject((IDictionary)dictionary).ToString();
    }
}