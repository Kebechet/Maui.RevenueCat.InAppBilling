namespace Maui.RevenueCat.InAppBilling.Extensions;

internal class EnumExtensions
{

    /// <summary>
    /// Returns name of enum value
    /// </summary>
    /// <param name="enumType"></param>
    /// <param name="enumValue"></param>
    /// <returns>Text representation of enum value</returns>
    internal static string EnumToTextValue(Type enumType, int enumValue)
    {
        string[] names = Enum.GetNames(enumType);
        int[] values = (int[])Enum.GetValues(enumType);
        for (int i = 0; i < names.Length; i++)
        {
            if (values[i] == enumValue)
                return names[i];
        }

        return string.Empty;
    }
}
