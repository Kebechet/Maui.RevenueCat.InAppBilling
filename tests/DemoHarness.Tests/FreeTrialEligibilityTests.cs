using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.InAppBilling.Models;
using Xunit;

namespace DemoHarness.Tests;

public class FreeTrialEligibilityTests
{
    private const string Monthly = "monthly";
    private const string Annual = "annual";

    [Fact]
    public void From_EveryAnswerDefinite_KeepsOnlyTheEligibleAndIsDefinite()
    {
        var statuses = new Dictionary<string, IntroElegibilityStatus>
        {
            [Monthly] = IntroElegibilityStatus.Ineligible,
            [Annual] = IntroElegibilityStatus.Eligible,
        };

        var freeTrialEligibility = FreeTrialEligibility.From([Monthly, Annual], statuses);

        Assert.Equal([Annual], freeTrialEligibility.EligibleProductIdentifiers);
        Assert.True(freeTrialEligibility.IsDefinite);
    }

    /// <summary>
    /// RevenueCat answers unknown for every product when StoreKit fails. Keeping that answer would hide the trial from
    /// an eligible user for the rest of the session, so it has to be asked again.
    /// </summary>
    [Fact]
    public void From_AnyAnswerUnknown_IsNotDefinite()
    {
        var statuses = new Dictionary<string, IntroElegibilityStatus>
        {
            [Monthly] = IntroElegibilityStatus.Unknown,
            [Annual] = IntroElegibilityStatus.Eligible,
        };

        var freeTrialEligibility = FreeTrialEligibility.From([Monthly, Annual], statuses);

        Assert.Equal([Annual], freeTrialEligibility.EligibleProductIdentifiers);
        Assert.False(freeTrialEligibility.IsDefinite);
    }

    [Fact]
    public void From_CheckFailed_HasNoEligibleProductAndIsNotDefinite()
    {
        var freeTrialEligibility = FreeTrialEligibility.From([Monthly, Annual], statuses: null);

        Assert.Empty(freeTrialEligibility.EligibleProductIdentifiers);
        Assert.False(freeTrialEligibility.IsDefinite);
    }

    [Fact]
    public void From_ProductMissingFromTheAnswer_IsNotDefinite()
    {
        var statuses = new Dictionary<string, IntroElegibilityStatus>
        {
            [Annual] = IntroElegibilityStatus.Eligible,
        };

        var freeTrialEligibility = FreeTrialEligibility.From([Monthly, Annual], statuses);

        Assert.False(freeTrialEligibility.IsDefinite);
    }

    [Fact]
    public void From_NoProductHasAFreeTrial_IsDefinite()
    {
        var freeTrialEligibility = FreeTrialEligibility.From([], new Dictionary<string, IntroElegibilityStatus>());

        Assert.Empty(freeTrialEligibility.EligibleProductIdentifiers);
        Assert.True(freeTrialEligibility.IsDefinite);
    }
}
