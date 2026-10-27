using Maui.RevenueCat.InAppBilling.Platforms.Android;
using Xunit;

namespace DemoHarness.Tests;

public class SubscriptionReplacementRulesTests
{
    private const long _yearlyMicros = 120_000_000;
    private const long _monthlyMicros = 15_000_000;

    [Fact]
    public void FindSubscriptionToReplace_OneActiveSubscriptionOfAnotherProduct_ReturnsIt()
    {
        var basic = new ActivePlayStoreSubscription("basic", "monthly", IsSandbox: false);

        var subscriptionToReplace = SubscriptionReplacementRules.FindSubscriptionToReplace([basic], "premium");

        Assert.Equal(basic, subscriptionToReplace);
    }

    [Fact]
    public void FindSubscriptionToReplace_NoActiveSubscription_ReturnsNull()
    {
        var subscriptionToReplace = SubscriptionReplacementRules.FindSubscriptionToReplace([], "premium");

        Assert.Null(subscriptionToReplace);
    }

    [Fact]
    public void FindSubscriptionToReplace_ActiveSubscriptionOfSameProduct_ReturnsNull()
    {
        var premiumMonthly = new ActivePlayStoreSubscription("premium", "monthly", IsSandbox: false);

        var subscriptionToReplace = SubscriptionReplacementRules.FindSubscriptionToReplace([premiumMonthly], "premium");

        Assert.Null(subscriptionToReplace);
    }

    [Fact]
    public void FindSubscriptionToReplace_SeveralActiveSubscriptions_ReturnsNull()
    {
        var basic = new ActivePlayStoreSubscription("basic", "monthly", IsSandbox: false);
        var storage = new ActivePlayStoreSubscription("storage", "monthly", IsSandbox: false);

        var subscriptionToReplace = SubscriptionReplacementRules.FindSubscriptionToReplace([basic, storage], "premium");

        Assert.Null(subscriptionToReplace);
    }

    [Theory]
    [InlineData("basic", "monthly", true)]
    [InlineData("basic", "yearly", false)]
    [InlineData("premium", "monthly", false)]
    public void IsProductOf_KnownBasePlan_MatchesOnlyThatBasePlan(string subscriptionId, string basePlanId, bool expectedIsProductOf)
    {
        var basicMonthly = new ActivePlayStoreSubscription("basic", "monthly", IsSandbox: false);

        Assert.Equal(expectedIsProductOf, SubscriptionReplacementRules.IsProductOf(basicMonthly, subscriptionId, basePlanId));
    }

    [Fact]
    public void IsProductOf_UnknownBasePlan_MatchesAnyBasePlanOfTheSubscription()
    {
        var basic = new ActivePlayStoreSubscription("basic", BasePlanId: null, IsSandbox: false);

        Assert.True(SubscriptionReplacementRules.IsProductOf(basic, "basic", "yearly"));
    }

    [Fact]
    public void IsProductOf_ProductWithoutBasePlan_DoesNotMatchKnownBasePlan()
    {
        var basicMonthly = new ActivePlayStoreSubscription("basic", "monthly", IsSandbox: false);

        Assert.False(SubscriptionReplacementRules.IsProductOf(basicMonthly, "basic", basePlanId: null));
    }

    [Fact]
    public void GetComparablePrice_Production_ReturnsPricePerMonth()
    {
        var comparablePrice = SubscriptionReplacementRules.GetComparablePrice(_yearlyMicros, periodInMonths: 12, isSandbox: false);

        Assert.Equal(10_000_000, comparablePrice);
    }

    [Theory]
    [InlineData(12, 4_000_000)]
    [InlineData(6, 8_000_000)]
    [InlineData(3, 12_000_000)]
    [InlineData(1, 24_000_000)]
    public void GetComparablePrice_Sandbox_ReturnsPricePerAcceleratedRenewalMinute(double periodInMonths, long expectedComparablePrice)
    {
        var comparablePrice = SubscriptionReplacementRules.GetComparablePrice(_yearlyMicros, periodInMonths, isSandbox: true);

        Assert.Equal(expectedComparablePrice, comparablePrice);
    }

    [Fact]
    public void GetComparablePrice_NoPeriod_ReturnsNull()
    {
        var comparablePrice = SubscriptionReplacementRules.GetComparablePrice(_yearlyMicros, periodInMonths: null, isSandbox: false);

        Assert.Null(comparablePrice);
    }

    [Fact]
    public void IsUpgrade_MonthlyToYearlyInProduction_IsDowngrade()
    {
        var monthly = SubscriptionReplacementRules.GetComparablePrice(_monthlyMicros, periodInMonths: 1, isSandbox: false);
        var yearly = SubscriptionReplacementRules.GetComparablePrice(_yearlyMicros, periodInMonths: 12, isSandbox: false);

        Assert.False(SubscriptionReplacementRules.IsUpgrade(oldComparablePrice: monthly, newComparablePrice: yearly));
    }

    [Fact]
    public void IsUpgrade_MonthlyToYearlyInSandbox_IsUpgrade()
    {
        var monthly = SubscriptionReplacementRules.GetComparablePrice(_monthlyMicros, periodInMonths: 1, isSandbox: true);
        var yearly = SubscriptionReplacementRules.GetComparablePrice(_yearlyMicros, periodInMonths: 12, isSandbox: true);

        Assert.True(SubscriptionReplacementRules.IsUpgrade(oldComparablePrice: monthly, newComparablePrice: yearly));
    }

    [Theory]
    [InlineData(10L, 20L, true)]
    [InlineData(20L, 10L, false)]
    [InlineData(10L, 10L, false)]
    [InlineData(null, 10L, false)]
    [InlineData(10L, null, false)]
    public void IsUpgrade_ComparesPrices(long? oldComparablePrice, long? newComparablePrice, bool expectedIsUpgrade)
    {
        Assert.Equal(expectedIsUpgrade, SubscriptionReplacementRules.IsUpgrade(oldComparablePrice, newComparablePrice));
    }

    [Theory]
    [InlineData(10L, 20L)]
    [InlineData(20L, 10L)]
    [InlineData(null, null)]
    public void ChooseReplacementMode_PrepaidPlan_ChargesFullPrice(long? oldComparablePrice, long? newComparablePrice)
    {
        var replacementMode = SubscriptionReplacementRules.ChooseReplacementMode(isNewPlanPrepaid: true, oldComparablePrice, newComparablePrice);

        Assert.Equal(SubscriptionReplacementMode.ChargeFullPrice, replacementMode);
    }

    [Fact]
    public void ChooseReplacementMode_Upgrade_ChargesProratedPrice()
    {
        var replacementMode = SubscriptionReplacementRules.ChooseReplacementMode(isNewPlanPrepaid: false, oldComparablePrice: 10, newComparablePrice: 20);

        Assert.Equal(SubscriptionReplacementMode.ChargeProratedPrice, replacementMode);
    }

    [Theory]
    [InlineData(20L, 10L)]
    [InlineData(10L, 10L)]
    [InlineData(null, 10L)]
    [InlineData(10L, null)]
    public void ChooseReplacementMode_NotAnUpgrade_Defers(long? oldComparablePrice, long? newComparablePrice)
    {
        var replacementMode = SubscriptionReplacementRules.ChooseReplacementMode(isNewPlanPrepaid: false, oldComparablePrice, newComparablePrice);

        Assert.Equal(SubscriptionReplacementMode.Deferred, replacementMode);
    }
}
