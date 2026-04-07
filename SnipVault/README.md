# SnipVault — Smart Snippet Manager

A Windows desktop app to **save, organize, and quickly paste** your frequently used text and code snippets.

![.NET 8](https://img.shields.io/badge/.NET-8.0-blue)
![WPF](https://img.shields.io/badge/UI-WPF-purple)
![License](https://img.shields.io/badge/License-MIT-green)
![Zero Dependencies](https://img.shields.io/badge/Dependencies-None-brightgreen)

## Features

### Snippet Management
- Create, edit, and delete text/code snippets
- Assign categories and programming language tags
- One-click copy to clipboard with usage counter
- Pin important snippets to the top of the list
- Mark favorites with a star for quick filtering

### Search & Organization
- Real-time search across titles, content, categories, and languages
- Filter by category or favorites-only mode
- Auto-sorted: pinned first, then by last modified date
- 19 built-in language tags (C#, Python, JavaScript, SQL, etc.)

### Import / Export
- Export all snippets as JSON to clipboard
- Import snippets from clipboard JSON (automatic de-duplication)
- Perfect for backup or sharing between machines

### User Experience
- Dark-themed modern UI
- Built-in 3-page Quick Start Guide for testers
- Monospace code editor with tab support
- Status bar with real-time feedback
- All data persisted locally as JSON — no cloud, no accounts

## For Testers (MS Store Review)

1. **First Launch**: A 3-page Quick Start Guide appears. Read through it.
2. **Create a Snippet**: Click "+ New", enter a title and content, click "💾 Save".
3. **Copy to Clipboard**: Select any snippet, click the green "📋 Copy" button.
4. **Organize**: Try adding categories, starring favorites, and pinning snippets.
5. **Search**: Type in the search box to filter snippets in real time.
6. **Import/Export**: Click "📤 Export" to copy all snippets as JSON, "📥 Import" to restore.
7. **Reopen Guide**: Click "📖 Guide" in the bottom bar anytime.

## Getting Started

### Prerequisites
- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Build & Run
```bash
git clone https://github.com/your-username/SnipVault.git
cd SnipVault
dotnet build
dotnet run --project SnipVault
```

### Run Tests
```bash
dotnet test
```

## Project Structure
```
SnipVault/
├── SnipVault.sln
├── SnipVault/                    # WPF Application
│   ├── Models/                   # Snippet, AppSettings
│   ├── ViewModels/               # MVVM ViewModels
│   ├── Views/                    # GuideWindow
│   ├── Services/                 # JSON persistence
│   ├── Helpers/                  # RelayCommand
│   ├── Converters/               # WPF value converters
│   ├── MainWindow.xaml           # Main UI
│   └── App.xaml                  # Startup + error handling
├── SnipVault.Tests/              # xUnit test project
│   └── DataServiceTests.cs       # 20 unit tests
├── README.md
└── LICENSE
```

## Tech Stack

- **Framework**: .NET 8, WPF
- **Architecture**: MVVM
- **Testing**: xUnit
- **Storage**: Local JSON in `%APPDATA%/SnipVault/`
- **External dependencies**: None (zero NuGet packages in main project)

## Data Storage

All data is stored locally:
- `%APPDATA%/SnipVault/settings.json` — User preferences
- `%APPDATA%/SnipVault/snippets.json` — All saved snippets

No data is sent to any server.

## License

MIT License — see [LICENSE](LICENSE).
