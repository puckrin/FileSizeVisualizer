// Copyright (c) 2026 Shaun Puckrin. Licensed under the MIT License.
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using FileSizeVisualizer.Utilities;

namespace FileSizeVisualizer.Models;

public class FileItem : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _fileName = string.Empty;
    private string _fullPath = string.Empty;
    private long _size;
    private double _percentage;
    private double _percentageInDirectory;
    private Brush _color = Brushes.Gray;
    private bool _isHighlighted;

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

    public string FileName
    {
        get => _fileName;
        set
        {
            _fileName = value;
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

    public long Size
    {
        get => _size;
        set
        {
            _size = value;
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

    public double PercentageInDirectory
    {
        get => _percentageInDirectory;
        set
        {
            _percentageInDirectory = value;
            OnPropertyChanged();
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

    public bool IsHighlighted
    {
        get => _isHighlighted;
        set
        {
            if (_isHighlighted != value)
            {
                _isHighlighted = value;
                OnPropertyChanged();
            }
        }
    }

    public string TooltipText => $"{FileName}\n{FileSizeFormatter.Format(Size)} ({Percentage:F1}% of total)";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
