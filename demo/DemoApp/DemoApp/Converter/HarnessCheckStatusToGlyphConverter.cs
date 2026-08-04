using System.Globalization;
using DemoApp.Harness;

namespace DemoApp.Converter;

public class HarnessCheckStatusToGlyphConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // During BindableLayout template inflation the binding can apply before the item
        // BindingContext is set, so value is transiently null or an inherited parent context.
        if (value is not HarnessCheckStatus harnessCheckStatus)
        {
            return string.Empty;
        }
        return harnessCheckStatus switch
        {
            HarnessCheckStatus.Running => "⏳",
            HarnessCheckStatus.Passed => "✅",
            HarnessCheckStatus.Failed => "❌",
            HarnessCheckStatus.TimedOut => "⏱",
            HarnessCheckStatus.Skipped => "➖",
            _ => throw new NotImplementedException(),
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
