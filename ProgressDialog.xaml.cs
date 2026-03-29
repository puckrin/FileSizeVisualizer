// Copyright (c) 2026 Shaun Puckrin. Licensed under the MIT License.
using System.Windows;

namespace FileSizeVisualizer;

public partial class ProgressDialog : Window
{
    public bool IsCancelled { get; private set; }

    public ProgressDialog()
    {
        InitializeComponent();
    }

    public void UpdateProgress(int filesFound, string currentPath, double? progressPercent = null)
    {
        Dispatcher.Invoke(() =>
        {
            ProgressText.Text = $"{filesFound:N0} files found";
            StatusText.Text = currentPath.Length > 50
                ? "..." + currentPath.Substring(currentPath.Length - 47)
                : currentPath;

            if (progressPercent.HasValue)
            {
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Value = progressPercent.Value;
            }
            else
            {
                ProgressBar.IsIndeterminate = true;
            }
        });
    }

    public void SetStatus(string status)
    {
        Dispatcher.Invoke(() =>
        {
            StatusText.Text = status;
        });
    }

    public void SetProcessingState(int fileCount)
    {
        Dispatcher.Invoke(() =>
        {
            Title = "Processing Files...";
            StatusText.Text = $"Processing {fileCount:N0} files...";
            ProgressText.Text = "Building chart...";
            ProgressBar.IsIndeterminate = true;
            UpdateLayout();
        });
    }

    public void SetComplete()
    {
        Dispatcher.Invoke(() =>
        {
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Value = 100;
            StatusText.Text = "Processing complete";
        });
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        IsCancelled = true;
        CancelButton.IsEnabled = false;
        CancelButton.Content = "Cancelling...";
        StatusText.Text = "Cancelling scan...";
    }
}
