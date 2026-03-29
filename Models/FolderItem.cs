// Copyright (c) 2026 Shaun Puckrin. Licensed under the MIT License.
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace FileSizeVisualizer.Models;

public class FolderItem : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _fullPath = string.Empty;
    private bool _isExpanded;
    private ObservableCollection<FolderItem> _children = new();
    private bool _hasLoadedChildren;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
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

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            _isExpanded = value;
            OnPropertyChanged();

            if (_isExpanded && !_hasLoadedChildren)
            {
                LoadChildren();
            }
        }
    }

    public ObservableCollection<FolderItem> Children
    {
        get => _children;
        set
        {
            _children = value;
            OnPropertyChanged();
        }
    }

    public void LoadChildren()
    {
        if (_hasLoadedChildren) return;
        _hasLoadedChildren = true;

        Children.Clear();

        try
        {
            var directories = Directory.GetDirectories(FullPath);
            foreach (var dir in directories)
            {
                try
                {
                    var dirInfo = new DirectoryInfo(dir);
                    if ((dirInfo.Attributes & FileAttributes.Hidden) == 0)
                    {
                        var child = new FolderItem
                        {
                            Name = Path.GetFileName(dir),
                            FullPath = dir
                        };
                        child.AddDummyChild();
                        Children.Add(child);
                    }
                }
                catch
                {
                    // Skip folders we can't access
                }
            }
        }
        catch
        {
            // Skip if we can't enumerate the directory
        }
    }

    public void AddDummyChild()
    {
        try
        {
            if (Directory.GetDirectories(FullPath).Length > 0)
            {
                Children.Add(new FolderItem { FullPath = "DUMMY", Name = "Loading..." });
            }
        }
        catch
        {
            // Ignore access errors
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
