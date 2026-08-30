using DemoApp.Harness;
using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.InAppBilling.Services;
using NSubstitute;
using Xunit;

namespace DemoHarness.Tests;

public class HarnessRunnerTests
{
    private static IRevenueCatBilling CreateHappyBilling()
    {
        var revenueCatBilling = Substitute.For<IRevenueCatBilling>();
        revenueCatBilling.IsInitialized().Returns(true);
        revenueCatBilling.IsAnonymous().Returns(true);
        revenueCatBilling.GetAppUserId().Returns("$RCAnonymousID:abc");
        revenueCatBilling.CanMakePayments(Arg.Any<CancellationToken>())
            .Returns(new CanMakePaymentsResultDto { Value = true });
        revenueCatBilling.GetOfferings(Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new OfferingsResultDto
            {
                Value =
                [
                    new()
                    {
                        Identifier = "default",
                        IsCurrent = true,
                        AvailablePackages =
                        [
                            new()
                            {
                                Identifier = "$rc_monthly",
                                OfferingIdentifier = "default",
                                Product = new ProductDto { Sku = "demo_sub_monthly" },
                            },
                        ],
                    },
                ],
            });
        revenueCatBilling.CheckTrialOrIntroDiscountEligibility(Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(new IntroEligibilityResultDto { Value = [] });
        revenueCatBilling.GetCustomerInfo(Arg.Any<CancellationToken>())
            .Returns(new CustomerInfoResultDto
            {
                Value = new CustomerInfoDto
                {
                    ActiveSubscriptions = [],
                    AllPurchasedIdentifiers = [],
                    NonConsumablePurchases = [],
                    FirstSeen = null,
                    LatestExpirationDate = null,
                    ManagementUrl = null,
                    Entitlements = [],
                },
            });
        revenueCatBilling.GetActiveSubscriptions(Arg.Any<CancellationToken>())
            .Returns(new ProductIdentifiersResultDto { Value = [] });
        revenueCatBilling.GetAllPurchasedIdentifiers(Arg.Any<CancellationToken>())
            .Returns(new ProductIdentifiersResultDto { Value = [] });
        revenueCatBilling.GetPurchaseDateForProductIdentifier(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new PurchaseDateResultDto());
        revenueCatBilling.GetManagementSubscriptionUrl(Arg.Any<CancellationToken>())
            .Returns(new ManagementUrlResultDto());
        revenueCatBilling.GetStorefrontCountryCode(Arg.Any<CancellationToken>())
            .Returns(new StorefrontResultDto { Value = "US" });
        return revenueCatBilling;
    }

    [Fact]
    public async Task RunAllChecks_AllMethodsSucceed_EveryCheckPassed()
    {
        var harnessRunner = new HarnessRunner(CreateHappyBilling(), new HarnessLog());

        var harnessCheckResults = await harnessRunner.RunAllChecksAsync();

        Assert.NotEmpty(harnessCheckResults);
        Assert.All(harnessCheckResults, x => Assert.Equal(HarnessCheckStatus.Passed, x.Status));
    }

    [Fact]
    public async Task RunAllChecks_GetOfferingsThrows_ReportsFailedAndContinues()
    {
        var revenueCatBilling = CreateHappyBilling();
        revenueCatBilling.GetOfferings(false, Arg.Any<CancellationToken>())
            .Returns(x => Task.FromException<OfferingsResultDto>(new InvalidOperationException("boom")));
        var harnessRunner = new HarnessRunner(revenueCatBilling, new HarnessLog());

        var harnessCheckResults = await harnessRunner.RunAllChecksAsync();

        var failedCheck = harnessCheckResults.First(x => x.Status == HarnessCheckStatus.Failed);
        Assert.Contains("boom", failedCheck.Error);
        Assert.Contains(harnessCheckResults, x => x.Status == HarnessCheckStatus.Passed);
    }

    [Fact]
    public async Task RunAllChecks_GetOfferingsReturnsErrorResult_ReportsFailedWithErrorStatus()
    {
        var revenueCatBilling = CreateHappyBilling();
        revenueCatBilling.GetOfferings(false, Arg.Any<CancellationToken>())
            .Returns(new OfferingsResultDto
            {
                Error = PurchaseErrorStatus.NetworkError,
                ErrorException = new InvalidOperationException("offline"),
            });
        var harnessRunner = new HarnessRunner(revenueCatBilling, new HarnessLog());

        var harnessCheckResults = await harnessRunner.RunAllChecksAsync();

        var failedCheck = harnessCheckResults.First(x => x.Status == HarnessCheckStatus.Failed);
        Assert.Contains(nameof(PurchaseErrorStatus.NetworkError), failedCheck.Error);
        Assert.Contains("offline", failedCheck.Error);
    }

    [Fact]
    public async Task RunAllChecks_MethodHangs_ReportsTimedOut()
    {
        var revenueCatBilling = CreateHappyBilling();
        revenueCatBilling.GetStorefrontCountryCode(Arg.Any<CancellationToken>())
            .Returns(new TaskCompletionSource<StorefrontResultDto>().Task);
        var harnessRunner = new HarnessRunner(revenueCatBilling, new HarnessLog(), TimeSpan.FromMilliseconds(100));

        var harnessCheckResults = await harnessRunner.RunAllChecksAsync();

        Assert.Contains(harnessCheckResults, x => x.Status == HarnessCheckStatus.TimedOut);
    }

    [Fact]
    public async Task RunAllChecks_OfferingsLoaded_EligibilityUsesLoadedProductSkus()
    {
        var revenueCatBilling = CreateHappyBilling();
        var harnessRunner = new HarnessRunner(revenueCatBilling, new HarnessLog());

        await harnessRunner.RunAllChecksAsync();

        await revenueCatBilling.Received().CheckTrialOrIntroDiscountEligibility(
            Arg.Is<List<string>>(x => x.Contains("demo_sub_monthly")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAllChecks_Progress_ReportsRunningThenTerminalPerCheck()
    {
        var reportedResults = new List<HarnessCheckResult>();
        var progress = new Progress<HarnessCheckResult>(reportedResults.Add);
        var harnessRunner = new HarnessRunner(CreateHappyBilling(), new HarnessLog());

        await harnessRunner.RunAllChecksAsync(progress);
        await Task.Delay(100); // Progress<T> posts callbacks asynchronously

        Assert.Contains(reportedResults, x => x.Status == HarnessCheckStatus.Running);
        Assert.Contains(reportedResults, x => x.Status == HarnessCheckStatus.Passed);
    }

    [Fact]
    public async Task RunAllChecks_MethodThrowsNotImplemented_ReportsSkipped()
    {
        var revenueCatBilling = CreateHappyBilling();
        revenueCatBilling.CheckTrialOrIntroDiscountEligibility(Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns<Task<IntroEligibilityResultDto>>(x => throw new NotImplementedException("This method is iOS Only"));
        var harnessRunner = new HarnessRunner(revenueCatBilling, new HarnessLog());

        var harnessCheckResults = await harnessRunner.RunAllChecksAsync();

        var skippedCheck = harnessCheckResults.First(x => x.Status == HarnessCheckStatus.Skipped);
        Assert.Contains("iOS Only", skippedCheck.Summary);
        Assert.DoesNotContain(harnessCheckResults, x => x.Status == HarnessCheckStatus.Failed);
    }

    [Fact]
    public async Task RunAllChecks_CheckHonoringCancellation_TimesOutViaToken()
    {
        var revenueCatBilling = CreateHappyBilling();
        revenueCatBilling.GetStorefrontCountryCode(Arg.Any<CancellationToken>())
            .Returns(async x =>
            {
                await Task.Delay(System.Threading.Timeout.Infinite, x.Arg<CancellationToken>());
                return new StorefrontResultDto { Value = "US" };
            });
        var harnessRunner = new HarnessRunner(revenueCatBilling, new HarnessLog(), TimeSpan.FromMilliseconds(100));

        var harnessCheckResults = await harnessRunner.RunAllChecksAsync();

        Assert.Contains(harnessCheckResults, x => x.Status == HarnessCheckStatus.TimedOut);
    }
}
