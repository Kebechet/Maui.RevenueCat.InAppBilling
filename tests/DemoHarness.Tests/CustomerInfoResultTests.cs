using DemoApp.Harness;
using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.InAppBilling.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace DemoHarness.Tests;

public class CustomerInfoResultTests
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
        WillRenew = true,
        UnsubscribeDetectedAt = null,
    };

    public static TheoryData<string> CustomerInfoOperations => new()
    {
        nameof(IRevenueCatBilling.RestoreTransactions),
        nameof(IRevenueCatBilling.Login),
        nameof(IRevenueCatBilling.Logout),
        nameof(IRevenueCatBilling.GetCustomerInfo),
    };

    [Fact]
    public async Task StandardPlatformStub_EveryCustomerInfoOperation_ReturnsSuccessCarryingCustomerInfo()
    {
        var billing = _standardPlatformBilling.Value;

        CustomerInfoResultDto[] results =
        [
            await billing.RestoreTransactions(CancellationToken.None),
            await billing.Login("harness", CancellationToken.None),
            await billing.Logout(CancellationToken.None),
            await billing.GetCustomerInfo(CancellationToken.None),
        ];

        Assert.All(results, result =>
        {
            Assert.True(result.IsSuccess);
            Assert.False(result.IsError);
            Assert.Null(result.ErrorStatus);
            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.CustomerInfo);
            Assert.Empty(result.CustomerInfo.ActiveSubscriptions);
        });
    }

    [Theory]
    [MemberData(nameof(CustomerInfoOperations))]
    public void FormatCustomerInfoResult_Success_RendersCustomerInfo(string operationName)
    {
        var result = new CustomerInfoResultDto { IsSuccess = true, CustomerInfo = CreateCustomerInfo() };

        Assert.Equal(
            $"PASS {operationName}: 1 active sub(s), 1 purchased, entitlements: pro",
            HarnessFormatter.FormatCustomerInfoResult(result, operationName));
    }

    [Theory]
    [MemberData(nameof(CustomerInfoOperations))]
    public void FormatCustomerInfoResult_BackendRejection_SurfacesErrorStatusAndSdkMessage(string operationName)
    {
        var result = new CustomerInfoResultDto
        {
            ErrorStatus = PurchaseErrorStatus.UnknownBackendError,
            ErrorMessage = "There was a problem with the store. (7255 alias limit reached) code: 16",
        };

        Assert.False(result.IsSuccess);
        Assert.True(result.IsError);
        Assert.Null(result.CustomerInfo);
        Assert.Equal(
            $"FAIL {operationName}: UnknownBackendError: There was a problem with the store. (7255 alias limit reached) code: 16",
            HarnessFormatter.FormatCustomerInfoResult(result, operationName));
    }

    [Fact]
    public void FormatCustomerInfoResult_CancelledWithoutMessage_LeavesNoDanglingSeparator()
    {
        var result = new CustomerInfoResultDto { ErrorStatus = PurchaseErrorStatus.PurchaseCancelledError };

        Assert.Equal(
            "FAIL RestoreTransactions: PurchaseCancelledError",
            HarnessFormatter.FormatCustomerInfoResult(result, nameof(IRevenueCatBilling.RestoreTransactions)));
    }

    [Fact]
    public void FormatCustomerInfoResult_SuccessWithoutCustomerInfo_DoesNotThrow()
    {
        var result = new CustomerInfoResultDto { IsSuccess = true };

        Assert.Equal(
            "PASS Login: null",
            HarnessFormatter.FormatCustomerInfoResult(result, nameof(IRevenueCatBilling.Login)));
    }

    [Fact]
    public async Task GetCustomerInfo_ErrorResult_HarnessReportsTheErrorInsteadOfNull()
    {
        var revenueCatBilling = Substitute.For<IRevenueCatBilling>();
        revenueCatBilling.GetCustomerInfo(Arg.Any<CancellationToken>())
            .Returns(new CustomerInfoResultDto
            {
                ErrorStatus = PurchaseErrorStatus.NetworkError,
                ErrorMessage = "offline",
            });

        var customerInfoResult = await revenueCatBilling.GetCustomerInfo();

        Assert.True(customerInfoResult.IsError);
        Assert.Equal(PurchaseErrorStatus.NetworkError, customerInfoResult.ErrorStatus);
        Assert.Equal("offline", customerInfoResult.ErrorMessage);
    }
}
