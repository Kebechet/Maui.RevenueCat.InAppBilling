using System.Globalization;
using Maui.RevenueCat.InAppBilling.Extensions;
using Xunit;

namespace DemoHarness.Tests;

public class PackageDtoExtensionsTests
{
    private static string FormatUnderCulture(string cultureName, string currencyCode, decimal price)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(cultureName);
            return PackageDtoExtensions.GetLocalizedPrice(currencyCode, price);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void GetLocalizedPrice_CzkUnderEnglishCulture_PutsSymbolAfterAmount()
    {
        var localizedPrice = FormatUnderCulture("en-US", "CZK", 1999.99m);

        Assert.Equal("1,999.99 Kč", localizedPrice);
    }

    [Fact]
    public void GetLocalizedPrice_EurUnderEnglishCulture_PutsSymbolAfterAmount()
    {
        var localizedPrice = FormatUnderCulture("en-US", "EUR", 4.99m);

        Assert.Equal("4.99 €", localizedPrice);
    }

    [Fact]
    public void GetLocalizedPrice_UsdUnderEnglishCulture_KeepsSymbolBeforeAmount()
    {
        var localizedPrice = FormatUnderCulture("en-US", "USD", 1999.99m);

        Assert.Equal("$1,999.99", localizedPrice);
    }

    [Fact]
    public void GetLocalizedPrice_WholeAmount_OmitsDecimals()
    {
        var localizedPrice = FormatUnderCulture("en-US", "CZK", 2000m);

        Assert.Equal("2,000 Kč", localizedPrice);
    }

    [Fact]
    public void GetLocalizedPrice_UnknownCurrencyCode_UsesIsoCodeAsSymbol()
    {
        var localizedPrice = FormatUnderCulture("en-US", "XXY", 9.99m);

        Assert.Equal("XXY9.99", localizedPrice);
    }
}
