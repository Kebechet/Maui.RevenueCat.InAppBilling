using Maui.RevenueCat.InAppBilling.Enums;
using Types.Result;

namespace Maui.RevenueCat.InAppBilling.Models;

/// <summary>
/// Outcome of a trial / introductory discount eligibility check, keyed by product identifier.
/// </summary>
public class IntroEligibilityResultDto : DataResult<Dictionary<string, IntroElegibilityStatus>, PurchaseErrorStatus>
{
}
