// Copyright (c) 2026 Shaun Puckrin. Licensed under the MIT License.
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using FileSizeVisualizer.Models;
using FileSizeVisualizer.ViewModels;
using Microsoft.VisualBasic.FileIO;

namespace FileSizeVisualizer;

public partial class MainWindow : Window
{
    private CancellationTokenSource? _cancellationTokenSource;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async Task ShowLoadingOverlayAsync(Func<Task> action)
    {
        var originalCursor = Cursor;
        Cursor = System.Windows.Input.Cursors.Wait;
        LoadingOverlay.Visibility = Visibility.Visible;
        await Task.Delay(50); // Allow overlay to render

        try
        {
            await action();
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            Cursor = originalCursor;
        }
    }

    private async void StartScan(string path)
    {
        if (DataContext is not MainViewModel vm)
            return;

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        vm.ClearFiles();
        vm.SelectedPath = path;

        if (string.IsNullOrEmpty(path))
            return;

        // Check cache first
        var cached = vm.GetCachedScan(path);
        if (cached.HasValue)
        {
            await ShowLoadingOverlayAsync(async () =>
                await vm.ApplyPreparedFilesAsync(cached.Value.files, cached.Value.groups, cached.Value.totalSize, Dispatcher));
            return;
        }

        // Create and show progress dialog
        var progressDialog = new ProgressDialog { Owner = this };

        var scanTask = Task.Run(async () =>
        {
            return await vm.ScanFilesAsync(
                path,
                (count, currentPath) => progressDialog.UpdateProgress(count, currentPath),
                () => progressDialog.IsCancelled,
                token
            );
        }, token);

        progressDialog.Show();

        try
        {
            var files = await scanTask;

            if (files != null && !progressDialog.IsCancelled && !token.IsCancellationRequested)
            {
                progressDialog.SetProcessingState(files.Count);
                progressDialog.UpdateLayout();
                await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Render);

                var result = await Task.Run(() => vm.PrepareFilesInBackground(files, path));
                vm.CacheScanResult(path, result.files, result.groups, result.totalSize);
                progressDialog.Close();

                await ShowLoadingOverlayAsync(async () =>
                    await vm.ApplyPreparedFilesAsync(result.files, result.groups, result.totalSize, Dispatcher));
            }
            else
            {
                progressDialog.Close();
            }
        }
        catch
        {
            progressDialog.Close();
        }
    }

    private void FolderItem_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is FolderItem folder)
        {
            if (folder.FullPath != "DUMMY")
            {
                StartScan(folder.FullPath);
                e.Handled = true;
            }
        }
    }

    private void ClearSearch_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.SearchText = string.Empty;
        }
    }

    private void RescanButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm && !string.IsNullOrEmpty(vm.SelectedPath))
        {
            // Invalidate cache for current path and rescan
            vm.InvalidateCache(vm.SelectedPath);
            StartScan(vm.SelectedPath);
        }
    }

    private void MarimekkoChart_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (DataContext is MainViewModel vm && e.NewSize.Height > 0)
        {
            vm.UpdateChartHeight(e.NewSize.Height);
        }
    }

    private void ChartSegment_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is FileItem file)
        {
            if (DataContext is MainViewModel vm)
            {
                // Select the file in the ViewModel (this will also highlight the chart segment)
                vm.SelectedFile = file;

                // Scroll the DataGrid to show the selected item
                FileDataGrid.ScrollIntoView(file);
            }
            e.Handled = true;
        }
    }

    private void OpenFile_Click(object sender, RoutedEventArgs e)
    {
        if (FileDataGrid.SelectedItem is FileItem file)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = file.FullPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void DeleteFile_Click(object sender, RoutedEventArgs e)
    {
        if (FileDataGrid.SelectedItem is FileItem file)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to move this file to the Recycle Bin?\n\n{file.FullPath}",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Move to recycle bin instead of permanent delete
                    FileSystem.DeleteFile(file.FullPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

                    // Remove from the view model and invalidate cache
                    if (DataContext is MainViewModel vm)
                    {
                        vm.RemoveFile(file);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not delete file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        base.OnClosed(e);
    }
}
