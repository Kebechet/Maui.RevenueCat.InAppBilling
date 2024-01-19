using System.Globalization;

namespace DemoApp.Converter;

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !((bool)value);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
        //throw new NotImplementedException();
    }
}