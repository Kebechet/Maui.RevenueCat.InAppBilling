using Maui.RevenueCat.InAppBilling.Enums;

namespace Maui.RevenueCat.InAppBilling.Models;

/// <summary>
/// Which products' free trial the user can still start, from RevenueCat's eligibility answers. Free of Apple types so
/// the tests can reach it.
/// </summary>
/// <param name="EligibleProductIdentifiers">Only an <see cref="IntroElegibilityStatus.Eligible"/> answer keeps a trial.</param>
/// <param name="IsDefinite">
/// False when the check failed or any free-trial product came back <see cref="IntroElegibilityStatus.Unknown"/> or not
/// at all. Such an answer is not kept for the session: RevenueCat reports unknown for every product when StoreKit fails,
/// so keeping it would hide the trial from an eligible user until the app restarts.
/// </param>
internal sealed record FreeTrialEligibility(IReadOnlySet<string> EligibleProductIdentifiers, bool IsDefinite)
{
    internal static FreeTrialEligibility NotChecked { get; } = new(new HashSet<string>(), IsDefinite: false);

    internal static FreeTrialEligibility From(IReadOnlyCollection<string> freeTrialProductIdentifiers, IReadOnlyDictionary<string, IntroElegibilityStatus>? statuses)
    {
        if (statuses is null)
        {
            return NotChecked;
        }

        var eligibleProductIdentifiers = statuses
            .Where(x => x.Value == IntroElegibilityStatus.Eligible)
            .Select(x => x.Key)
            .ToHashSet();
        var isDefinite = freeTrialProductIdentifiers.All(x => statuses.TryGetValue(x, out var status) && status != IntroElegibilityStatus.Unknown);

        return new(eligibleProductIdentifiers, isDefinite);
    }
}
