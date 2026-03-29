// Copyright (c) 2026 Shaun Puckrin. Licensed under the MIT License.
using System.Globalization;
using System.Windows.Data;

namespace FileSizeVisualizer.Converters;

public class BarWidthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
            return 0.0;

        if (values[0] is double relativeSize && values[1] is double containerWidth)
        {
            // relativeSize is 0-100, representing percentage
            // Subtract a small margin for padding
            double maxWidth = Math.Max(0, containerWidth - 8);
            return relativeSize / 100.0 * maxWidth;
        }

        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
