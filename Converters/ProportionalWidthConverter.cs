// Copyright (c) 2026 Shaun Puckrin. Licensed under the MIT License.
using System.Globalization;
using System.Windows.Data;

namespace FileSizeVisualizer.Converters;

public class ProportionalWidthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
            return 0.0;

        if (values[0] is double percentage && values[1] is double containerWidth)
        {
            // percentage is 0-100, convert to width
            // Subtract margin for gaps between segments
            double width = percentage / 100.0 * (containerWidth - 10);
            return Math.Max(0, width);
        }

        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
