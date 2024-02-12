using Foundation;
using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;

internal static class RCOfferingsExtensions
{
    public static List<OfferingDto> ToOfferingDtoList(this RCOfferings offerings)
    {
        var offeringDtos = new List<OfferingDto>();

        foreach (var offer in offerings.All.Values)
        {
            var offeringDto = new OfferingDto()
            {
                Identifier = offer.Identifier,
                AvailablePackages = offer.AvailablePackages.ToPackageDtoList(),
                IsCurrent = offer.Identifier == offerings.Current?.Identifier,
                Metadata = offer.Metadata.ToDictionary<KeyValuePair<NSString, NSObject>, string, object?>(item => item.Key,
                    item => item.Value.ToDotNetObject())
            };
            offeringDtos.Add(offeringDto);
        }

        return offeringDtos;
    }

    private static object? ToDotNetObject(this NSObject nsObj) =>
        nsObj switch
        {
            NSString stringValue => stringValue.ToString(), // string
            NSDecimalNumber d => (decimal)d.NSDecimalValue, // decimal
            NSNumber numberValue => numberValue.ObjCType switch
            {
                "c" when numberValue.Class.Name == "__NSCFBoolean" => numberValue.BoolValue, // ObjC bool
                "c" => numberValue.SByteValue, // signed char
                "i" => numberValue.Int32Value, // signed int
                "s" => numberValue.Int16Value, // signed short
                "l" => numberValue.Int32Value, // signed long (32 bit)
                "q" => numberValue.Int64Value, // signed long long (64 bit)
                "C" => numberValue.ByteValue, // unsigned char
                "I" => numberValue.UInt32Value, // unsigned int
                "S" => numberValue.UInt16Value, // unsigned short
                "L" => numberValue.UInt32Value, // unsigned long (32 bit)
                "Q" => numberValue.UInt64Value, // unsigned long long (64 bit)
                "f" => numberValue.FloatValue, // float
                "d" => numberValue.DoubleValue, // double
                "B" => numberValue.BoolValue, // C++ bool or C99 _Bool
                _ => numberValue.ToString() // fallback
            },
            NSArray arrayValue => arrayValue.ToDotNetList(),
            NSDictionary dictValue => dictValue.ToDotNetDictionary(),
            NSNull => null,
            _ => nsObj.ToString() // fallback
        };

    private static Dictionary<object, object?> ToDotNetDictionary(this NSDictionary nsDictionary)
    {
        var dotNetDictionary = new Dictionary<object, object?>();

        foreach (var key in nsDictionary.Keys)
        {
            var value = nsDictionary[key];
            dotNetDictionary.Add(key.ToDotNetObject() ?? throw new NullReferenceException("Key cannot be null."), value.ToDotNetObject());
        }

        return dotNetDictionary;
    }

    private static List<object?> ToDotNetList(this NSArray nsArray)
    {
        var dotNetList = new List<object?>();

        for (nuint i = 0; i < nsArray.Count; i++)
        {
            var nsObj = nsArray.GetItem<NSObject>(i);
            dotNetList.Add(nsObj.ToDotNetObject());
        }

        return dotNetList;
    }
}