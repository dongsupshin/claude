# FocusGuard — Smart Focus Timer & Eye Care

A Windows desktop application that helps you stay productive using the **Pomodoro Technique** and protects your eyes with the **20-20-20 Rule**.

![.NET 8](https://img.shields.io/badge/.NET-8.0-blue)
![WPF](https://img.shields.io/badge/UI-WPF-purple)
![License](https://img.shields.io/badge/License-MIT-green)

## Features

### Pomodoro Focus Timer
- Configurable work sessions (default: 25 minutes)
- Automatic short breaks (5 min) and long breaks (15 min after every 4 sessions)
- Start, pause, reset, and skip controls
- Session counter and progress tracking

### Eye Care (20-20-20 Rule)
- Automatic reminders every 20 minutes during focus sessions
- Gentle overlay prompts you to look at something 20 feet away for 20 seconds
- Helps prevent digital eye strain — backed by optometrist recommendations

### Statistics Dashboard
- Daily and weekly focus time charts (powered by LiveCharts2)
- Session completion rate
- Focus streak tracking (consecutive days)
- Total focus minutes overview

### User Experience
- Dark-themed modern UI
- Built-in Quick Start Guide (4-page onboarding walkthrough)
- Sidebar navigation (Timer / Statistics / Settings)
- System sounds for notifications
- All settings customizable and persisted locally as JSON

## Screenshots

*Screenshots will be added after the first build.*

## Getting Started

### Prerequisites
- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (recommended) or VS Code with C# extension

### Build & Run

```bash
# Clone the repository
git clone https://github.com/your-username/FocusGuard.git
cd FocusGuard

# Restore NuGet packages
dotnet restore

# Build
dotnet build

# Run
dotnet run --project FocusGuard
```

### For Testers (MS Store Review)

1. **First Launch**: The app shows a 4-page Quick Start Guide. Read through it — it explains every feature.
2. **Timer Page**: Click the purple ▶ Play button to start a focus session. The timer counts down from 25:00.
3. **Eye Care**: During a focus session, after 20 minutes you'll see a blue overlay asking you to look away. Wait for the countdown or click "Dismiss".
4. **Breaks**: When a work session ends, a break starts automatically. After 4 sessions, you get a longer break.
5. **Statistics**: Navigate to Statistics from the sidebar to see your focus history charts.
6. **Settings**: Customize timer durations, enable/disable eye care, toggle sounds, etc.
7. **Quick Guide**: Reopen the guide anytime by clicking "Quick Guide" at the bottom of the sidebar.

## Project Structure

```
FocusGuard/
├── FocusGuard.sln
├── FocusGuard/
│   ├── FocusGuard.csproj
│   ├── App.xaml / App.xaml.cs          # Application startup
│   ├── MainWindow.xaml / .cs           # Main window with sidebar
│   ├── Models/
│   │   ├── FocusSession.cs             # Session data model
│   │   └── UserSettings.cs             # Settings model
│   ├── ViewModels/
│   │   ├── BaseViewModel.cs            # MVVM base class
│   │   ├── MainViewModel.cs            # Navigation & orchestration
│   │   ├── TimerViewModel.cs           # Pomodoro + eye care logic
│   │   ├── StatsViewModel.cs           # Statistics & charts
│   │   └── SettingsViewModel.cs        # Settings management
│   ├── Views/
│   │   ├── TimerView.xaml / .cs        # Timer UI
│   │   ├── StatsView.xaml / .cs        # Statistics UI
│   │   ├── SettingsView.xaml / .cs     # Settings UI
│   │   └── GuideWindow.xaml / .cs      # Onboarding guide
│   ├── Services/
│   │   ├── SessionDataService.cs       # JSON persistence
│   │   └── SoundService.cs             # System sounds
│   ├── Helpers/
│   │   └── RelayCommand.cs             # ICommand implementation
│   └── Converters/
│       └── BoolToVisibilityConverter.cs
├── README.md
└── LICENSE
```

## Tech Stack

- **Framework**: .NET 8, WPF (Windows Presentation Foundation)
- **Architecture**: MVVM (Model-View-ViewModel)
- **Charts**: [LiveCharts2](https://github.com/beto-rodriguez/LiveCharts2) (MIT License)
- **System Tray**: [H.NotifyIcon.Wpf](https://github.com/HavenDV/H.NotifyIcon) (MIT License)
- **Storage**: Local JSON files in `%APPDATA%/FocusGuard/`
- **No paid APIs or external services required**

## Data Storage

All data is stored locally on your machine:
- `%APPDATA%/FocusGuard/settings.json` — User preferences
- `%APPDATA%/FocusGuard/sessions.json` — Focus session history (last 90 days)

No data is sent to any server. Your focus data stays on your computer.

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Commit your changes (`git commit -m 'Add my feature'`)
4. Push to the branch (`git push origin feature/my-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
