# FileSizeVisualizer

A Windows desktop application for visualizing disk usage across your file system. Quickly scan any folder or drive and see exactly where your storage is going, with an interactive Marimekko-style chart and sortable file list.

![Platform](https://img.shields.io/badge/platform-Windows-blue) ![.NET](https://img.shields.io/badge/.NET-8.0-purple) ![Framework](https://img.shields.io/badge/framework-WPF-blueviolet) ![Vibe Coded with Claude](https://img.shields.io/badge/vibe%20coded%20with-Claude-orange)

---

![Screenshot](screenshots/FileSizeVisualizer%20v1.0.0.png)

## Features

- **Visual disk usage chart** — Color-coded Marimekko chart groups files by directory so you can spot large folders at a glance
- **Sortable file list** — Browse all files with their sizes, paths, and percentage of total disk usage
- **Cross-highlighting** — Click a segment in the chart to highlight the corresponding file in the list, and vice versa
- **Search** — Filter the file list in real time by filename
- **Scan caching** — Results are cached in memory so revisiting a folder is instant
- **Rescan** — Invalidate the cache and re-scan a folder with one click
- **Open files** — Launch any file directly from the app using the system default program
- **Delete to Recycle Bin** — Safely remove files; they go to the Recycle Bin rather than being permanently deleted
- **Progress dialog** — A live progress indicator shows scan status and lets you cancel long-running scans

## Requirements

- Windows 10 or later
- [.NET 8 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (Desktop Runtime)

## Getting Started

### Run from source

1. Clone the repository:
   ```
   git clone https://github.com/your-username/FileSizeVisualizer.git
   cd FileSizeVisualizer
   ```

2. Build and run:
   ```
   dotnet run
   ```

### Download the executable

A pre-built executable is available on the [Releases](https://github.com/puckrin/FileSizeVisualizer/releases) page.

1. Download `FileSizeVisualizer.exe` from the latest release
2. Run it directly — no installation required

## Usage

1. Launch the application
2. Select a drive or folder from the left-hand panel
3. Wait for the scan to complete — a progress dialog will show the current status
4. Explore the chart and file list:
   - Click a **chart segment** to highlight the file in the list
   - Click a **row in the list** to highlight the segment in the chart
   - Use the **search bar** to filter files by name
   - Right-click or use the toolbar buttons to **open** or **delete** a file
5. Use the **Rescan** button to refresh results for the current folder

## Project Structure

```
FileSizeVisualizer/
├── Models/
│   ├── FileItem.cs          # Represents a single file with size and display metadata
│   ├── FolderItem.cs        # Represents a drive or folder in the navigation panel
│   └── DirectoryGroup.cs    # Groups files by their parent directory for chart rendering
├── ViewModels/
│   └── MainViewModel.cs     # Core application logic, scanning, caching, and filtering
├── Converters/              # WPF value converters for data binding
├── MainWindow.xaml(.cs)     # Main application window
├── ProgressDialog.xaml(.cs) # Scan progress overlay
└── App.xaml(.cs)            # Application entry point
```

## Architecture

The app follows the **MVVM** (Model-View-ViewModel) pattern:

- **Models** hold plain data (file metadata, groupings)
- **ViewModel** handles all business logic — scanning the file system asynchronously, preparing chart data, managing the in-memory cache, and filtering the file list
- **Views** (XAML) bind to the ViewModel with no logic of their own, except for UI-specific event routing

Scanning runs on a background thread to keep the UI responsive, and results are dispatched back to the UI thread via the WPF `Dispatcher`.

## License

This proj