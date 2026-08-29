using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.InAppBilling.Services;

namespace DemoApp.Harness;

/// <summary>
/// Renders <see cref="IRevenueCatBilling"/> results as harness log lines. Lives here rather than
/// in the page so the formatting is reachable from tests - the interactive flows can't be driven
/// by <see cref="HarnessRunner"/>.
/// </summary>
public static class HarnessFormatter
{
    public static string FormatCustomerInfo(CustomerInfoDto? customerInfo)
    {
        if (customerInfo is null)
        {
            return "null";
        }

        var entitlementIdentifiers = customerInfo.Entitlements.Any()
            ? string.Join(", ", customerInfo.Entitlements.Select(x => x.Identifier))
            : "none";

        return $"{customerInfo.ActiveSubscriptions.Count} active sub(s), {customerInfo.AllPurchasedIdentifiers.Count} purchased, entitlements: {entitlementIdentifiers}";
    }

    public static string FormatCustomerInfoResult(CustomerInfoResultDto customerInfoResult, string operationName)
    {
        if (customerInfoResult.IsSuccess)
        {
            return $"PASS {operationName}: {FormatCustomerInfo(customerInfoResult.CustomerInfo)}";
        }

        var errorDetail = string.IsNullOrEmpty(customerInfoResult.ErrorMessage)
            ? $"{customerInfoResult.ErrorStatus}"
            : $"{customerInfoResult.ErrorStatus}: {customerInfoResult.ErrorMessage}";

        return $"FAIL {operationName}: {errorDetail}";
    }
}
