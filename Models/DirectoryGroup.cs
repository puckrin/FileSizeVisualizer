// Copyright (c) 2026 Shaun Puckrin. Licensed under the MIT License.
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using FileSizeVisualizer.Utilities;

namespace FileSizeVisualizer.Models;

public class DirectoryGroup : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _fullPath = string.Empty;
    private long _totalSize;
    private double _percentage;
    private Brush _color = Brushes.Gray;
    private List<FileItem> _files = new();
    private double _chartHeight = 200; // Default height

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TooltipText));
        }
    }

    public string FullPath
    {
        get => _fullPath;
        set
        {
            _fullPath = value;
            OnPropertyChanged();
        }
    }

    public long TotalSize
    {
        get => _totalSize;
        set
        {
            _totalSize = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TooltipText));
        }
    }

    public double Percentage
    {
        get => _percentage;
        set
        {
            _percentage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TooltipText));
        }
    }

    public Brush Color
    {
        get => _color;
        set
        {
            _color = value;
            OnPropertyChanged();
        }
    }

    public List<FileItem> Files
    {
        get => _files;
        set
        {
            _files = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ChartFiles));
        }
    }

    public double ChartHeight
    {
        get => _chartHeight;
        set
        {
            if (Math.Abs(_chartHeight - value) > 0.1)
            {
                _chartHeight = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ChartFiles));
                OnPropertyChanged(nameof(ChartLabel));
                OnPropertyChanged(nameof(HasVisibleIndividualFiles));
            }
        }
    }

    // Minimum height in pixels for a file segment to be visible
    private const double MinSegmentHeight = 3.0;

    /// <summary>
    /// Minimum percentage a file needs to be visible at current chart height.
    /// </summary>
    private double MinVisiblePercentage => (_chartHeight > 0) ? (MinSegmentHeight / _chartHeight) * 100 : 1.0;

    private Brush GetLighterShade(Brush baseBrush)
    {
        if (baseBrush is SolidColorBrush solidBrush)
        {
            var baseColor = solidBrush.Color;
            // Make it slightly lighter by blending with white (factor 0.15 = 15% lighter)
            var lighter = new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                (byte)(baseColor.R + (255 - baseColor.R) * 0.15),
                (byte)(baseColor.G + (255 - baseColor.G) * 0.15),
                (byte)(baseColor.B + (255 - baseColor.B) * 0.15)
            ));
            lighter.Freeze();
            return lighter;
        }
        return baseBrush;
    }

    public IEnumerable<FileItem> ChartFiles
    {
        get
        {
            if (_files.Count == 0)
                return _files;

            var visibleFiles = new List<FileItem>();
            var smallerFiles = new List<FileItem>();

            foreach (var file in _files)
            {
                if (file.PercentageInDirectory >= MinVisiblePercentage)
                {
                    visibleFiles.Add(file);
                }
                else
                {
                    smallerFiles.Add(file);
                }
            }

            // If there are smaller files, add a combined item
            if (smallerFiles.Count > 0)
            {
                var remainingSize = smallerFiles.Sum(f => f.Size);
                var remainingPercentageInDir = smallerFiles.Sum(f => f.PercentageInDirectory);
                var remainingPercentageTotal = smallerFiles.Sum(f => f.Percentage);

                var smallerFilesItem = new FileItem
                {
                    Name = $"({smallerFiles.Count} smaller files)",
                    FileName = $"({smallerFiles.Count} smaller files)",
                    Size = remainingSize,
                    Percentage = remainingPercentageTotal,
                    PercentageInDirectory = remainingPercentageInDir,
                    Color = GetLighterShade(_color)
                };

                visibleFiles.Add(smallerFilesItem);
            }

            return visibleFiles;
        }
    }

    public string TooltipText => $"{Name}\n{FileSizeFormatter.Format(TotalSize)} ({Percentage:F1}%)\n{Files.Count} files";

    public string ChartLabel => $"{FileSizeFormatter.Format(TotalSize)}\n{Files.Count} files";

    /// <summary>
    /// Returns true if at least one individual file is visible (not just "smaller files").
    /// </summary>
    public bool HasVisibleIndividualFiles => _files.Count > 0 && _files.Any(f => f.PercentageInDirectory >= MinVisiblePercentage);

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
