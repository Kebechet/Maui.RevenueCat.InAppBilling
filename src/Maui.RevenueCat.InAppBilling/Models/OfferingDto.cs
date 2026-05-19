using System.Collections.Immutable;
using System.Text.Json;

namespace Maui.RevenueCat.InAppBilling.Models;

public sealed record OfferingDto
{
    public string Identifier { get; init; } = string.Empty;
    public List<PackageDto> AvailablePackages { get; init; } = new();
    public bool IsCurrent { get; init; }
    public IReadOnlyDictionary<string, JsonElement> Metadata { get; init; } = ImmutableDictionary<string, JsonElement>.Empty;
}
