using System.Collections.Immutable;
using System.Text.Json;

namespace Maui.RevenueCat.InAppBilling.Extensions;

internal static class MetadataExtensions
{
    internal static IReadOnlyDictionary<string, JsonElement> ToMetadataDictionary(this string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return ImmutableDictionary<string, JsonElement>.Empty;
        }

        using var document = JsonDocument.Parse(json);
        return document.RootElement.EnumerateObject()
            .ToDictionary(x => x.Name, x => x.Value.Clone());
    }
}
