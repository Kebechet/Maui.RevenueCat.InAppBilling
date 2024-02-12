using Android.Runtime;
using Com.Revenuecat.Purchases;
using Maui.RevenueCat.InAppBilling.Models;

namespace Maui.RevenueCat.InAppBilling.Platforms.Android.Extensions;

internal static class OfferingsExtensions
{
    internal static List<OfferingDto> ToOfferingDtoList(this Offerings offerings)
    {
        var offeringDtos = new List<OfferingDto>();

        foreach (var offer in offerings.All.Values)
        {
            var offerDto = new OfferingDto()
            {
                Identifier = offer.Identifier,
                AvailablePackages = offer.AvailablePackages.ToPackageDtoList(),
                IsCurrent = offer.Identifier == offerings?.Current?.Identifier,
                Metadata = offer.Metadata.ToDictionary(item=>item.Key, item=>item.Value.ToDotNetObject())
            };

            offeringDtos.Add(offerDto);
        }

        return offeringDtos;
    }

    private static object? ToDotNetObject(this Java.Lang.Object? jObj) =>
        jObj switch
        {
            Java.Lang.String => jObj.ToString(),
            Java.Lang.Integer javaInteger => javaInteger.IntValue(),
            Java.Lang.Double javaDouble => javaDouble.DoubleValue(),
            Java.Lang.Boolean javaBoolean => javaBoolean.BooleanValue(),
            Java.Lang.Byte javaByte=>javaByte.ByteValue(),
            Java.Lang.Character javaChar=>javaChar.CharValue(),
            Java.Lang.Float javaFloat=>javaFloat.FloatValue(),
            Java.Lang.Long javaLong=>javaLong.LongValue(),
            JavaList javaList => javaList.ToDotNetList(),
            Java.Util.IMap javaMap => javaMap.ToDotNetDictionary(),
            null => null,
            _ => jObj.ToString() // fallback
        };

    private static Dictionary<object, object?> ToDotNetDictionary(this Java.Util.IMap javaMap)
    {
        var dotNetDictionary = new Dictionary<object, object?>();
        var entries = javaMap.EntrySet().Cast<Java.Util.AbstractMap.SimpleImmutableEntry>();

        foreach (var entry in entries)
        {
            dotNetDictionary.Add(entry.Key.ToDotNetObject() ?? throw new NullReferenceException("Key cannot be null."), entry.Value.ToDotNetObject());
        }

        return dotNetDictionary;
    }

    private static List<object?> ToDotNetList(this JavaList javaList)
    {
        var dotNetList = new List<object?>();

        foreach (var item in javaList.ToArray())
        {
            dotNetList.Add(item.ToDotNetObject());
        }

        return dotNetList;
    }
}