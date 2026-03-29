// Copyright (c) 2026 Shaun Puckrin. Licensed under the MIT License.
using System.Globalization;
using System.Windows.Data;

namespace FileSizeVisualizer.Converters;

public class FileSizeConverter : IValueConverter
{
    private static readonly string[] SizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB" };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not long bytes)
            return "0 B";

        if (bytes == 0)
            return "0 B";

        int magnitude = (int)Math.Floor(Math.Log(bytes, 1024));
        magnitude = Math.Min(magnitude, SizeSuffixes.Length - 1);

        double adjustedSize = bytes / Math.Pow(1024, magnitude);

        return magnitude == 0
            ? $"{adjustedSize:N0} {SizeSuffixes[magnitude]}"
            : $"{adjustedSize:N2} {SizeSuffixes[magnitude]}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
