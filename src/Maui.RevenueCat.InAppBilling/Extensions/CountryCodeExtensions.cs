using System.Globalization;

namespace Maui.RevenueCat.InAppBilling.Extensions;

internal static class CountryCodeExtensions
{
    private static readonly Lazy<Dictionary<string, string>> _alpha2ByAlpha3 = new(BuildAlpha2ByAlpha3Map);

    /// <summary>
    /// Normalizes an ISO 3166-1 alpha-3 country code (e.g. "USA", as returned by StoreKit's
    /// SKStorefront) to alpha-2 ("US"). Codes that are already alpha-2, unknown, or empty are
    /// returned unchanged.
    /// </summary>
    internal static string ToIsoAlpha2CountryCode(this string countryCode)
    {
        if (countryCode.Length != 3)
        {
            return countryCode;
        }
        return _alpha2ByAlpha3.Value.TryGetValue(countryCode, out var alpha2CountryCode)
            ? alpha2CountryCode
            : countryCode;
    }

    private static Dictionary<string, string> BuildAlpha2ByAlpha3Map()
    {
        var alpha2ByAlpha3 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var specificCulture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            try
            {
                var regionInfo = new RegionInfo(specificCulture.Name);
                alpha2ByAlpha3[regionInfo.ThreeLetterISORegionName] = regionInfo.TwoLetterISORegionName;
            }
            catch (ArgumentException)
            {
                // Cultures without a resolvable region (neutral or synthetic ones) are skipped.
            }
        }
        return alpha2ByAlpha3;
    }
}
