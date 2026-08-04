using System.Globalization;
using DemoApp.Harness;

namespace DemoApp.Converter;

public class HarnessCheckStatusToGlyphConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (HarnessCheckStatus?)value switch
        {
            HarnessCheckStatus.Running => "⏳",
            HarnessCheckStatus.Passed => "✅",
            HarnessCheckStatus.Failed => "❌",
            HarnessCheckStatus.TimedOut => "⏱",
            _ => throw new NotImplementedException(),
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
