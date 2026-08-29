using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.InAppBilling.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DemoHarness.Tests;

public class RestoreTransactionsTests
{
    // RevenueCatBilling refuses a second instance per process (static flag, never reset),
    // so every test needing the real class has to share this one.
    private static readonly Lazy<RevenueCatBilling> _standardPlatformBilling =
        new(() => new RevenueCatBilling(NullLogger<RevenueCatBilling>.Instance));

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
        Assert.Empty(restoreResult.CustomerInfo.Entitlements);
    }

    [Fact]
    public async Task RestoreTransactions_StandardPlatformStub_ReportsNoTransaction()
    {
        var restoreResult = await _standardPlatformBilling.Value.RestoreTransactions(CancellationToken.None);

        Assert.Null(restoreResult.Transaction);
    }

    [Fact]
    public void PurchaseResultDto_ErrorStatusSet_IsErrorAndCarriesDiagnosticMessage()
    {
        var restoreResult = new PurchaseResultDto
        {
            ErrorStatus = PurchaseErrorStatus.NetworkError,
            ErrorMessage = "There was a problem with the store."
        };

        Assert.True(restoreResult.IsError);
        Assert.False(restoreResult.IsSuccess);
        Assert.Equal(PurchaseErrorStatus.NetworkError, restoreResult.ErrorStatus);
        Assert.Equal("There was a problem with the store.", restoreResult.ErrorMessage);
        Assert.Null(restoreResult.CustomerInfo);
    }

    [Fact]
    public void PurchaseResultDto_CancelledWithoutMessage_IsErrorWithCancelledStatusOnly()
    {
        var restoreResult = new PurchaseResultDto
        {
            ErrorStatus = PurchaseErrorStatus.PurchaseCancelledError
        };

        Assert.True(restoreResult.IsError);
        Assert.Null(restoreResult.ErrorMessage);
    }
}
