// Copyright (c) 2026 Shaun Puckrin. Licensed under the MIT License.
using System.Globalization;
using System.Windows.Data;

namespace FileSizeVisualizer.Converters;

public class HeightConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
            return 0.0;

        if (values[0] is double percentage && values[1] is double containerHeight)
        {
            // percentage is 0-100 (percentage within directory), convert to height
            double height = percentage / 100.0 * containerHeight;
            return Math.Max(1, height); // Minimum 1px for visibility
        }

        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
