using DemoApp.Harness;
using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.InAppBilling.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Types.Result;
using Xunit;

namespace DemoHarness.Tests;

public class BillingResultTests
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
        ManagementUrl = null,
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
    public async Task StandardPlatformStub_EveryFallibleOperation_ReportsSuccess()
    {
        var billing = _standardPlatformBilling.Value;

        Result<PurchaseErrorStatus>[] results =
        [
            await billing.CanMakePayments(CancellationToken.None),
            await billing.CheckTrialOrIntroDiscountEligibility([], CancellationToken.None),
            await billing.GetOfferings(false, CancellationToken.None),
            await billing.PurchaseProduct(new PackageDto(), CancellationToken.None),
            await billing.GetActiveSubscriptions(CancellationToken.None),
            await billing.GetAllPurchasedIdentifiers(CancellationToken.None),
            await billing.GetPurchaseDateForProductIdentifier("sku", CancellationToken.None),
            await billing.GetManagementSubscriptionUrl(CancellationToken.None),
            await billing.Login("harness", CancellationToken.None),
            await billing.Logout(CancellationToken.None),
            await billing.RestoreTransactions(CancellationToken.None),
            await billing.GetCustomerInfo(CancellationToken.None),
            await billing.GetStorefrontCountryCode(CancellationToken.None),
        ];

        Assert.Equal(13, results.Length);
        Assert.All(results, result =>
        {
            Assert.True(result.IsSuccess);
            Assert.False(result.IsError);
            Assert.Null(result.Error);
            Assert.Null(result.ErrorException);
        });
    }

    [Fact]
    public async Task StandardPlatformStub_CustomerInfoOperations_CarryCustomerInfo()
    {
        var billing = _standardPlatformBilling.Value;

        DataResult<CustomerInfoDto, PurchaseErrorStatus>[] results =
        [
            await billing.RestoreTransactions(CancellationToken.None),
            await billing.Login("harness", CancellationToken.None),
            await billing.Logout(CancellationToken.None),
            await billing.GetCustomerInfo(CancellationToken.None),
        ];

        Assert.All(results, result =>
        {
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value.ActiveSubscriptions);
        });
    }

    [Fact]
    public async Task StandardPlatformStub_NoStoreSubscription_SucceedsWithNullValue()
    {
        var managementUrlResult = await _standardPlatformBilling.Value.GetManagementSubscriptionUrl(CancellationToken.None);

        Assert.True(managementUrlResult.IsSuccess);
        Assert.Null(managementUrlResult.Value);
    }

    [Fact]
    public async Task StandardPlatformStub_Purchase_ReportsNoTransaction()
    {
        var purchaseResult = await _standardPlatformBilling.Value.PurchaseProduct(new PackageDto(), CancellationToken.None);

        Assert.True(purchaseResult.IsSuccess);
        Assert.Null(purchaseResult.Transaction);
    }

    [Theory]
    [MemberData(nameof(CustomerInfoOperations))]
    public void FormatCustomerInfoResult_Success_RendersCustomerInfo(string operationName)
    {
        var result = new DataResult<CustomerInfoDto, PurchaseErrorStatus> { Value = CreateCustomerInfo() };

        Assert.Equal(
            $"PASS {operationName}: 1 active sub(s), 1 purchased, entitlements: pro",
            HarnessFormatter.FormatCustomerInfoResult(result, operationName));
    }

    [Theory]
    [MemberData(nameof(CustomerInfoOperations))]
    public void FormatCustomerInfoResult_BackendRejection_SurfacesErrorStatusAndSdkMessage(string operationName)
    {
        var result = new DataResult<CustomerInfoDto, PurchaseErrorStatus>
        {
            Error = PurchaseErrorStatus.UnknownBackendError,
            ErrorException = new InvalidOperationException("There was a problem with the store. (7255 alias limit reached) code: 16"),
        };

        Assert.False(result.IsSuccess);
        Assert.True(result.IsError);
        Assert.Null(result.Value);
        Assert.Equal(
            $"FAIL {operationName}: UnknownBackendError: There was a problem with the store. (7255 alias limit reached) code: 16",
            HarnessFormatter.FormatCustomerInfoResult(result, operationName));
    }

    [Fact]
    public void FormatError_ErrorWithoutException_LeavesNoDanglingSeparator()
    {
        var result = new DataResult<CustomerInfoDto, PurchaseErrorStatus> { Error = PurchaseErrorStatus.PurchaseCancelledError };

        Assert.Equal("PurchaseCancelledError", HarnessFormatter.FormatError(result));
    }

    [Fact]
    public void FormatCustomerInfoResult_SuccessWithoutCustomerInfo_DoesNotThrow()
    {
        var result = new DataResult<CustomerInfoDto, PurchaseErrorStatus>();

        Assert.Equal(
            "PASS Login: null",
            HarnessFormatter.FormatCustomerInfoResult(result, nameof(IRevenueCatBilling.Login)));
    }

    [Fact]
    public void PurchaseResultDto_PendingTransaction_IsErrorButStillCarriesTransaction()
    {
        var purchaseResult = new PurchaseResultDto
        {
            Error = PurchaseErrorStatus.PaymentPendingError,
            Transaction = new StoreTransactionDto
            {
                TransactionIdentifier = "tx-1",
                ProductIdentifier = "demo_sub_monthly",
                PurchaseDate = DateTime.UnixEpoch,
                Quantity = 1,
            },
            Value = CreateCustomerInfo(),
        };

        Assert.True(purchaseResult.IsError);
        Assert.False(purchaseResult.IsSuccess);
        Assert.Equal("tx-1", purchaseResult.Transaction.TransactionIdentifier);
        Assert.NotNull(purchaseResult.Value);
    }

    [Fact]
    public void PurchaseResultDto_ShareTheCustomerInfoResultShape()
    {
        Assert.True(typeof(DataResult<CustomerInfoDto, PurchaseErrorStatus>).IsAssignableFrom(typeof(PurchaseResultDto)));
    }
}
