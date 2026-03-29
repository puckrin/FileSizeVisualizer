// Copyright (c) 2026 Shaun Puckrin. Licensed under the MIT License.
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Media;
using FileSizeVisualizer.Models;

namespace FileSizeVisualizer.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private string _selectedPath = string.Empty;
    private ObservableCollection<FileItem> _files = new();
    private ObservableCollection<FolderItem> _drives = new();
    private ObservableCollection<DirectoryGroup> _directoryGroups = new();
    private FolderItem? _selectedFolder;
    private long _totalSize;
    private string _searchText = string.Empty;
    private ICollectionView? _filteredFiles;
    private FileItem? _selectedFile;

    // Memory cache for scan results
    private readonly Dictionary<string, CachedScanResult> _scanCache = new(StringComparer.OrdinalIgnoreCase);

    private class CachedScanResult
    {
        public required List<FileItem> Files { get; init; }
        public required List<DirectoryGroup> Groups { get; init; }
        public required long TotalSize { get; init; }
        public DateTime CachedAt { get; init; } = DateTime.Now;
    }

    // Color palette for directories (frozen for cross-thread access)
    private static readonly Brush[] DirectoryColorPalette = CreateFrozenColorPalette();

    private static Brush[] CreateFrozenColorPalette()
    {
        var brushes = new Brush[]
        {
            new SolidColorBrush(Color.FromRgb(66, 133, 244)),   // Blue
            new SolidColorBrush(Color.FromRgb(234, 67, 53)),    // Red
            new SolidColorBrush(Color.FromRgb(251, 188, 5)),    // Yellow
            new SolidColorBrush(Color.FromRgb(52, 168, 83)),    // Green
            new SolidColorBrush(Color.FromRgb(255, 109, 0)),    // Orange
            new SolidColorBrush(Color.FromRgb(156, 39, 176)),   // Purple
            new SolidColorBrush(Color.FromRgb(0, 188, 212)),    // Cyan
            new SolidColorBrush(Color.FromRgb(233, 30, 99)),    // Pink
            new SolidColorBrush(Color.FromRgb(63, 81, 181)),    // Indigo
            new SolidColorBrush(Color.FromRgb(139, 195, 74)),   // Light Green
            new SolidColorBrush(Color.FromRgb(121, 85, 72)),    // Brown
            new SolidColorBrush(Color.FromRgb(96, 125, 139)),   // Blue Grey
        };

        foreach (var brush in brushes)
        {
            brush.Freeze();
        }

        return brushes;
    }

    public MainViewModel()
    {
        LoadDrives();
    }

    public string SelectedPath
    {
        get => _selectedPath;
        set
        {
            if (_selectedPath != value)
            {
                _selectedPath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanRescan));
            }
        }
    }

    /// <summary>
    /// Returns true if the current path can be rescanned (has been scanned before).
    /// </summary>
    public bool CanRescan => !string.IsNullOrEmpty(_selectedPath) && _scanCache.ContainsKey(_selectedPath);

    public ObservableCollection<FileItem> Files
    {
        get => _files;
        set
        {
            _files = value;
            OnPropertyChanged();
            UpdateFilteredFiles();
        }
    }

    public ICollectionView? FilteredFiles
    {
        get => _filteredFiles;
        private set
        {
            _filteredFiles = value;
            OnPropertyChanged();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                FilteredFiles?.Refresh();
                OnPropertyChanged(nameof(FilteredFileCount));
            }
        }
    }

    public int FilteredFileCount => FilteredFiles?.Cast<FileItem>().Count() ?? 0;

    public FileItem? SelectedFile
    {
        get => _selectedFile;
        set
        {
            if (_selectedFile != value)
            {
                // Clear highlight on previous selection
                if (_selectedFile != null)
                {
                    _selectedFile.IsHighlighted = false;
                }

                _selectedFile = value;

                // Set highlight on new selection
                if (_selectedFile != null)
                {
                    _selectedFile.IsHighlighted = true;
                }

                OnPropertyChanged();
            }
        }
    }

    private void UpdateFilteredFiles()
    {
        FilteredFiles = CollectionViewSource.GetDefaultView(_files);
        FilteredFiles.Filter = FilterFile;

        // Default sort by size descending (largest first)
        FilteredFiles.SortDescriptions.Clear();
        FilteredFiles.SortDescriptions.Add(new SortDescription(nameof(FileItem.Size), ListSortDirection.Descending));

        OnPropertyChanged(nameof(FilteredFileCount));
    }

    private bool FilterFile(object obj)
    {
        if (string.IsNullOrWhiteSpace(_searchText))
            return true;

        if (obj is FileItem file)
        {
            return file.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                   file.FileName.Contains(_searchText, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    public ObservableCollection<FolderItem> Drives
    {
        get => _drives;
        set
        {
            _drives = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<DirectoryGroup> DirectoryGroups
    {
        get => _directoryGroups;
        set
        {
            _directoryGroups = value;
            OnPropertyChanged();
        }
    }

    public FolderItem? SelectedFolder
    {
        get => _selectedFolder;
        set
        {
            _selectedFolder = value;
            OnPropertyChanged();

            if (_selectedFolder != null && _selectedFolder.FullPath != "DUMMY")
            {
                SelectedPath = _selectedFolder.FullPath;
            }
        }
    }

    public long TotalSize
    {
        get => _totalSize;
        set
        {
            _totalSize = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Returns true when any directory column has no visible individual files (only "smaller files").
    /// </summary>
    public bool ShowExpandChartMessage =>
        DirectoryGroups.Count > 0 && DirectoryGroups.Any(g => !g.HasVisibleIndividualFiles);

    private void LoadDrives()
    {
        Drives.Clear();

        foreach (var drive in DriveInfo.GetDrives())
        {
            if (drive.IsReady)
            {
                var driveItem = new FolderItem
                {
                    Name = $"{drive.Name} ({drive.VolumeLabel})",
                    FullPath = drive.RootDirectory.FullName
                };
                driveItem.AddDummyChild();
                Drives.Add(driveItem);
            }
        }
    }

    public void ClearFiles()
    {
        SearchText = string.Empty;
        Files.Clear();
        DirectoryGroups.Clear();
        TotalSize = 0;
    }

    /// <summary>
    /// Gets cached scan result if available.
    /// </summary>
    public (List<FileItem> files, List<DirectoryGroup> groups, long totalSize)? GetCachedScan(string path)
    {
        if (_scanCache.TryGetValue(path, out var cached))
        {
            System.Diagnostics.Debug.WriteLine($"[Cache] Hit for {path} (cached {(DateTime.Now - cached.CachedAt).TotalSeconds:F1}s ago)");
            return (cached.Files, cached.Groups, cached.TotalSize);
        }
        return null;
    }

    /// <summary>
    /// Stores scan result in cache.
    /// </summary>
    public void CacheScanResult(string path, List<FileItem> files, List<DirectoryGroup> groups, long totalSize)
    {
        _scanCache[path] = new CachedScanResult
        {
            Files = files,
            Groups = groups,
            TotalSize = totalSize
        };
        OnPropertyChanged(nameof(CanRescan));
        System.Diagnostics.Debug.WriteLine($"[Cache] Stored {path} ({files.Count} files)");
    }

    /// <summary>
    /// Removes a specific path from the cache.
    /// </summary>
    public void InvalidateCache(string path)
    {
        if (_scanCache.Remove(path))
        {
            OnPropertyChanged(nameof(CanRescan));
            System.Diagnostics.Debug.WriteLine($"[Cache] Invalidated {path}");
        }
    }

    /// <summary>
    /// Updates the chart height for all directory groups to recalculate visible files.
    /// </summary>
    public void UpdateChartHeight(double height)
    {
        foreach (var group in DirectoryGroups)
        {
            group.ChartHeight = height;
        }
        OnPropertyChanged(nameof(ShowExpandChartMessage));
    }

    public void RemoveFile(FileItem file)
    {
        // Remove from Files collection
        Files.Remove(file);

        // Find and update the DirectoryGroup
        foreach (var group in DirectoryGroups)
        {
            if (group.Files.Remove(file))
            {
                // Update group totals
                group.TotalSize -= file.Size;
                group.Percentage = TotalSize > file.Size ? (double)group.TotalSize / (TotalSize - file.Size) * 100 : 0;
                break;
            }
        }

        // Update total size
        TotalSize -= file.Size;

        // Clear selection if this was the selected file
        if (SelectedFile == file)
        {
            SelectedFile = null;
        }

        // Invalidate cache since data has changed
        InvalidateCache(_selectedPath);

        // Refresh filtered view count
        OnPropertyChanged(nameof(FilteredFileCount));
    }

    public async Task<List<FileItem>?> ScanFilesAsync(
        string path,
        Action<int, string> progressCallback,
        Func<bool> isCancelledCallback,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            return null;

        var fileItems = new List<FileItem>();

        try
        {
            var options = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true
            };

            await Task.Run(() =>
            {
                foreach (var filePath in Directory.EnumerateFiles(path, "*", options))
                {
                    if (cancellationToken.IsCancellationRequested || isCancelledCallback())
                    {
                        return;
                    }

                    try
                    {
                        var file = new FileInfo(filePath);
                        if ((file.Attributes & FileAttributes.Hidden) == 0)
                        {
                            var relativePath = Path.GetRelativePath(path, file.FullName);

                            fileItems.Add(new FileItem
                            {
                                Name = relativePath,
                                FileName = file.Name,
                                FullPath = file.FullName,
                                Size = file.Length
                            });

                            // Report progress every 100 files
                            if (fileItems.Count % 100 == 0)
                            {
                                progressCallback(fileItems.Count, filePath);
                            }
                        }
                    }
                    catch
                    {
                        // Skip files we can't access
                    }
                }
            }, cancellationToken);

            if (cancellationToken.IsCancellationRequested || isCancelledCallback())
            {
                return null;
            }

            return fileItems;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Prepares all data structures in the background thread. Call this off the UI thread.
    /// Returns plain Lists to avoid WPF thread affinity issues with ObservableCollections.
    /// </summary>
    public (List<FileItem> files, List<DirectoryGroup> groups, long totalSize) PrepareFilesInBackground(
        List<FileItem> fileItems,
        string selectedPath)
    {
        var totalSize = fileItems.Sum(f => f.Size);

        // Group files by their immediate subdirectory (first level only)
        var groupedFiles = fileItems
            .GroupBy(f => GetFirstLevelDirectory(f.Name))
            .OrderByDescending(g => g.Sum(f => f.Size))
            .ToList();

        var preparedFiles = new List<FileItem>();
        var preparedGroups = new List<DirectoryGroup>();

        int colorIndex = 0;
        foreach (var group in groupedFiles)
        {
            var dirColor = DirectoryColorPalette[colorIndex % DirectoryColorPalette.Length];
            var dirTotalSize = group.Sum(f => f.Size);

            var directoryGroup = new DirectoryGroup
            {
                Name = string.IsNullOrEmpty(group.Key) ? "(root)" : group.Key,
                FullPath = string.IsNullOrEmpty(group.Key) ? selectedPath : Path.Combine(selectedPath, group.Key),
                TotalSize = dirTotalSize,
                Percentage = totalSize > 0 ? (double)dirTotalSize / totalSize * 100 : 0,
                Color = dirColor
            };

            // Sort files within group by size descending
            var sortedFiles = group.OrderByDescending(f => f.Size).ToList();

            foreach (var file in sortedFiles)
            {
                file.Percentage = totalSize > 0 ? (double)file.Size / totalSize * 100 : 0;
                file.PercentageInDirectory = dirTotalSize > 0 ? (double)file.Size / dirTotalSize * 100 : 0;
                file.Color = GetShadeOfColor(dirColor, file.PercentageInDirectory);

                directoryGroup.Files.Add(file);
                preparedFiles.Add(file);
            }

            preparedGroups.Add(directoryGroup);
            colorIndex++;
        }

        return (preparedFiles, preparedGroups, totalSize);
    }

    /// <summary>
    /// Applies pre-prepared data to the UI asynchronously in chunks.
    /// This keeps the UI responsive during large data loads.
    /// </summary>
    public async Task ApplyPreparedFilesAsync(
        List<FileItem> filesList,
        List<DirectoryGroup> groupsList,
        long totalSize,
        System.Windows.Threading.Dispatcher dispatcher)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        TotalSize = totalSize;

        // Clear existing collections
        Files.Clear();
        DirectoryGroups.Clear();

        System.Diagnostics.Debug.WriteLine($"[Perf] Clear: {sw.ElapsedMilliseconds}ms");
        sw.Restart();

        // Load directory groups in one batch (chart) - this was the slow part with chunking
        DirectoryGroups = new ObservableCollection<DirectoryGroup>(groupsList);
        OnPropertyChanged(nameof(ShowExpandChartMessage));

        System.Diagnostics.Debug.WriteLine($"[Perf] DirectoryGroups ({groupsList.Count} groups): {sw.ElapsedMilliseconds}ms");
        sw.Restart();

        // Yield to let chart render
        await dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Render);

        System.Diagnostics.Debug.WriteLine($"[Perf] Chart render yield: {sw.ElapsedMilliseconds}ms");
        sw.Restart();

        // Load files in one batch (DataGrid has virtualization, so this should be fast)
        Files = new ObservableCollection<FileItem>(filesList);

        System.Diagnostics.Debug.WriteLine($"[Perf] Files ({filesList.Count} files): {sw.ElapsedMilliseconds}ms");
        sw.Restart();

        // Update filtered view
        UpdateFilteredFiles();

        System.Diagnostics.Debug.WriteLine($"[Perf] UpdateFilteredFiles: {sw.ElapsedMilliseconds}ms");
        System.Diagnostics.Debug.WriteLine($"[Perf] === Total groups: {groupsList.Count}, Total files: {filesList.Count} ===");
    }

    private static string GetFirstLevelDirectory(string relativePath)
    {
        var parts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return parts.Length > 1 ? parts[0] : "";
    }

    private static Brush GetShadeOfColor(Brush baseBrush, double percentage)
    {
        if (baseBrush is SolidColorBrush solidBrush)
        {
            var baseColor = solidBrush.Color;
            // Create varying shades - darker for larger files, lighter for smaller
            var factor = 0.6 + (percentage / 100.0) * 0.4; // Range from 0.6 to 1.0
            var shadeBrush = new SolidColorBrush(Color.FromRgb(
                (byte)(baseColor.R * factor),
                (byte)(baseColor.G * factor),
                (byte)(baseColor.B * factor)
            ));
            shadeBrush.Freeze(); // Freeze for cross-thread access
            return shadeBrush;
        }
        return baseBrush;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
