// Copyright (c) 2026 Shaun Puckrin. Licensed under the MIT License.
using System.Globalization;
using System.Windows.Data;

namespace FileSizeVisualizer.Converters;

public class PercentageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double percentage)
        {
            return percentage < 0.1 ? "<0.1%" : $"{percentage:F1}%";
        }
        return "0%";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
