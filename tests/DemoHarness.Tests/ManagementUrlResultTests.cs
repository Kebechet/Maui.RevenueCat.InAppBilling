using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.InAppBilling.Services;
using NSubstitute;
using Xunit;

namespace DemoHarness.Tests;

public class ManagementUrlResultTests
{
    [Fact]
    public async Task GetManagementSubscriptionUrl_NoStoreSubscription_IsSuccessWithNullUrl()
    {
        var revenueCatBilling = Substitute.For<IRevenueCatBilling>();
        revenueCatBilling.GetManagementSubscriptionUrl(Arg.Any<CancellationToken>())
            .Returns(new ManagementUrlResultDto { IsSuccess = true });

        var managementUrlResult = await revenueCatBilling.GetManagementSubscriptionUrl();

        Assert.True(managementUrlResult.IsSuccess);
        Assert.False(managementUrlResult.IsError);
        Assert.Null(managementUrlResult.ManagementUrl);
        Assert.Null(managementUrlResult.ErrorStatus);
    }

    [Fact]
    public async Task GetManagementSubscriptionUrl_Failure_IsDistinguishableFromNoSubscription()
    {
        var revenueCatBilling = Substitute.For<IRevenueCatBilling>();
        revenueCatBilling.GetManagementSubscriptionUrl(Arg.Any<CancellationToken>())
            .Returns(new ManagementUrlResultDto
            {
                ErrorStatus = PurchaseErrorStatus.NetworkError,
                ErrorMessage = "offline",
            });

        var managementUrlResult = await revenueCatBilling.GetManagementSubscriptionUrl();

        Assert.False(managementUrlResult.IsSuccess);
        Assert.True(managementUrlResult.IsError);
        Assert.Null(managementUrlResult.ManagementUrl);
        Assert.Equal(PurchaseErrorStatus.NetworkError, managementUrlResult.ErrorStatus);
        Assert.Equal("offline", managementUrlResult.ErrorMessage);
    }

    [Fact]
    public async Task GetManagementSubscriptionUrl_ActiveStoreSubscription_ReturnsTheUrl()
    {
        var revenueCatBilling = Substitute.For<IRevenueCatBilling>();
        revenueCatBilling.GetManagementSubscriptionUrl(Arg.Any<CancellationToken>())
            .Returns(new ManagementUrlResultDto { IsSuccess = true, ManagementUrl = "https://apps.apple.com/account/subscriptions" });

        var managementUrlResult = await revenueCatBilling.GetManagementSubscriptionUrl();

        Assert.True(managementUrlResult.IsSuccess);
        Assert.Equal("https://apps.apple.com/account/subscriptions", managementUrlResult.ManagementUrl);
    }
}
