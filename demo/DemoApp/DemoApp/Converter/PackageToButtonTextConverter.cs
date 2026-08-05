using System.Globalization;
using Maui.RevenueCat.InAppBilling.Models;

namespace DemoApp.Converter;

public class PackageToButtonTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // During BindableLayout template inflation the binding can apply before the item
        // BindingContext is set, so value is transiently null or an inherited parent context.
        if (value is not PackageDto packageDto)
        {
            return string.Empty;
        }
        return $"Buy {packageDto.Identifier} for {packageDto.Product.Pricing.PriceLocalized}";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
