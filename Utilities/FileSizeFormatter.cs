// Copyright (c) 2026 Shaun Puckrin. Licensed under the MIT License.
namespace FileSizeVisualizer.Utilities;

public static class FileSizeFormatter
{
    private static readonly string[] Suffixes = { "B", "KB", "MB", "GB", "TB" };

    public static string Format(long bytes)
    {
        if (bytes == 0) return "0 B";

        int magnitude = (int)Math.Floor(Math.Log(bytes, 1024));
        magnitude = Math.Min(magnitude, Suffixes.Length - 1);
        double adjustedSize = bytes / Math.Pow(1024, magnitude);

        return magnitude == 0
            ? $"{adjustedSize:N0} {Suffixes[magnitude]}"
            : $"{adjustedSize:N2} {Suffixes[magnitude]}";
    }
}
