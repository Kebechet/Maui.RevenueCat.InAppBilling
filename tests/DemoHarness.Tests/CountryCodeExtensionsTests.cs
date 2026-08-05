using Maui.RevenueCat.InAppBilling.Extensions;
using Xunit;

namespace DemoHarness.Tests;

public class CountryCodeExtensionsTests
{
    [Theory]
    [InlineData("USA", "US")]
    [InlineData("GBR", "GB")]
    [InlineData("CZE", "CZ")]
    [InlineData("DEU", "DE")]
    public void ToIsoAlpha2CountryCode_Alpha3Code_ReturnsAlpha2(string alpha3CountryCode, string expectedAlpha2CountryCode)
    {
        var alpha2CountryCode = alpha3CountryCode.ToIsoAlpha2CountryCode();

        Assert.Equal(expectedAlpha2CountryCode, alpha2CountryCode);
    }

    [Fact]
    public void ToIsoAlpha2CountryCode_AlreadyAlpha2_ReturnsUnchanged()
    {
        var alpha2CountryCode = "US".ToIsoAlpha2CountryCode();

        Assert.Equal("US", alpha2CountryCode);
    }

    [Fact]
    public void ToIsoAlpha2CountryCode_UnknownCode_ReturnsUnchanged()
    {
        var unknownCountryCode = "XXX".ToIsoAlpha2CountryCode();

        Assert.Equal("XXX", unknownCountryCode);
    }

    [Fact]
    public void ToIsoAlpha2CountryCode_EmptyString_ReturnsEmpty()
    {
        var emptyCountryCode = string.Empty.ToIsoAlpha2CountryCode();

        Assert.Equal(string.Empty, emptyCountryCode);
    }
}
