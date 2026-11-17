using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.iOS;

namespace Maui.RevenueCat.InAppBilling.Platforms.iOS.Extensions;

internal static class RCStoreProductDiscountExtensions
{
    /// <summary>
    /// The trial's length when the introductory discount is a free trial, otherwise <c>null</c>. StoreKit gives a
    /// discount as a period and a number of periods, so a trial of two 1-week periods is 2 weeks.
    /// </summary>
    internal static SubscriptionPeriodDto? ToFreeTrialPeriodDto(this RCStoreProductDiscount? introductoryDiscount)
    {
        if (introductoryDiscount is null || introductoryDiscount.PaymentMode != RCPaymentMode.FreeTrial)
        {
            return null;
        }

        var discountPeriod = introductoryDiscount.SubscriptionPeriod.ToSubscriptionPeriodDto();
        if (discountPeriod is null)
        {
            return null;
        }

        return discountPeriod with
        {
            Value = discountPeriod.Value * (int)introductoryDiscount.NumberOfPeriods,
        };
    }
}
