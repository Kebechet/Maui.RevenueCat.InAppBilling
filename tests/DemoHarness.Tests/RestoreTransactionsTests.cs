using DemoApp.Harness;
using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.InAppBilling.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace DemoHarness.Tests;

public class RestoreTransactionsTests
{
    // RevenueCatBilling refuses a second instance per process (static flag, never reset),
    // so every test needing the real class has to share this one.
    private static readonly Lazy<RevenueCatBilling> _standardPlatformBilling =
        new(() => new RevenueCatBilling(NullLogger<RevenueCatBilling>.Instance));

    private static CustomerInfoDto CreateCustomerInfo() => new()
    {
        ActiveSubscriptions = ["demo_sub_monthly"],
        AllPurchasedIdentifiers = ["demo_sub_monthly"],
        NonConsumablePurchases = [],
        FirstSeen = null,
        LatestExpirationDate = null,
        ManagementURL = null,
        Entitlements = [CreateEntitlement("pro")],
    };

    private static EntitlementInfoDto CreateEntitlement(string identifier) => new()
    {
        BillingIssueDetectedAt = null,
        ExpirationDate = null,
        Identifier = identifier,
        IsActive = true,
        IsSandbox = false,
        LatestPurchaseDate = null,
        OriginalPurchaseDate = null,
        OwnershipType = OwnershipType.Purchased,
        PeriodType = PeriodType.Normal,
        ProductIdentifier = "demo_sub_monthly",
        ProductPlanIdentifier = string.Empty,
        Store = StoreType.AppStore,
        UnsubscribeDetectedAt = null,
        WillRenew = true,
    };

    [Fact]
    public async Task RestoreTransactions_StandardPlatformStub_ReturnsSuccessCarryingCustomerInfo()
    {
        var restoreResult = await _standardPlatformBilling.Value.RestoreTransactions(CancellationToken.None);

        Assert.True(restoreResult.IsSuccess);
        Assert.False(restoreResult.IsError);
        Assert.Null(restoreResult.ErrorStatus);
        Assert.Null(restoreResult.ErrorMessage);
        Assert.NotNull(restoreResult.CustomerInfo);
        Assert.Empty(restoreResult.CustomerInfo.ActiveSubscriptions);
        Assert.Null(restoreResult.Transaction);
    }

    [Fact]
    public async Task RestoreTransactions_CallerBranchesOnResult_SuccessSurfacesCustomerInfo()
    {
        var revenueCatBilling = Substitute.For<IRevenueCatBilling>();
        revenueCatBilling.RestoreTransactions(Arg.Any<CancellationToken>())
            .Returns(new PurchaseResultDto { IsSuccess = true, CustomerInfo = CreateCustomerInfo() });

        var logLine = HarnessFormatter.FormatRestoreResult(await revenueCatBilling.RestoreTransactions());

        Assert.Equal("PASS RestoreTransactions: 1 active sub(s), 1 purchased, entitlements: pro", logLine);
    }

    [Fact]
    public async Task RestoreTransactions_BackendRejection_SurfacesErrorStatusAndSdkMessage()
    {
        var revenueCatBilling = Substitute.For<IRevenueCatBilling>();
        revenueCatBilling.RestoreTransactions(Arg.Any<CancellationToken>())
            .Returns(new PurchaseResultDto
            {
                ErrorStatus = PurchaseErrorStatus.UnknownBackendError,
                ErrorMessage = "There was a problem with the store. (7255 alias limit reached) code: 16",
            });

        var restoreResult = await revenueCatBilling.RestoreTransactions();

        Assert.False(restoreResult.IsSuccess);
        Assert.True(restoreResult.IsError);
        Assert.Null(restoreResult.CustomerInfo);
        Assert.Equal(
            "FAIL RestoreTransactions: UnknownBackendError: There was a problem with the store. (7255 alias limit reached) code: 16",
            HarnessFormatter.FormatRestoreResult(restoreResult));
    }

    [Fact]
    public async Task RestoreTransactions_Cancelled_SurfacesStatusWithoutDanglingSeparator()
    {
        var revenueCatBilling = Substitute.For<IRevenueCatBilling>();
        revenueCatBilling.RestoreTransactions(Arg.Any<CancellationToken>())
            .Returns(new PurchaseResultDto { ErrorStatus = PurchaseErrorStatus.PurchaseCancelledError });

        var logLine = HarnessFormatter.FormatRestoreResult(await revenueCatBilling.RestoreTransactions());

        Assert.Equal("FAIL RestoreTransactions: PurchaseCancelledError", logLine);
    }

    [Fact]
    public void FormatRestoreResult_SuccessWithoutCustomerInfo_DoesNotThrow()
    {
        var logLine = HarnessFormatter.FormatRestoreResult(new PurchaseResultDto { IsSuccess = true });

        Assert.Equal("PASS RestoreTransactions: null", logLine);
    }
}
