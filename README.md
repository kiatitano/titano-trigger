# Titano Trigger

A Persian-first Windows desktop utility for streamers. Titano Trigger checks the public Reymit API for new donations and maps configured donation amount ranges to timed keyboard or mouse input.

**Created by:** kiatitano & titanoteam  
**Instagram / YouTube:** kiatitano

## Features

- Connects to the official Reymit API using a token entered by the user at runtime.
- Map Toman donation ranges to a keyboard key or mouse button and a hold duration from 0.1 to 10 seconds.
- Processes only new donations after live control starts, with duplicate-ID protection and a sequential action queue.
- Emergency stop: `Ctrl + Shift + F12`.
- Minimize to the Windows system tray; monitoring continues until the user chooses Exit.
- Persian right-to-left interface with Vazirmatn fonts.

## Download and run

Download the Windows package from the repository's **Releases** section. Extract the complete ZIP archive and run `TitanoTrigger.exe` from its folder. The files next to the executable are required.

Requirements:

- Windows 10/11 x64
- [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [Microsoft Edge WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/)

## Setup

1. In Reymit settings, open **Miscellaneous** and create an API token. Enter it in the app.
2. Configure donation amount ranges, input actions, and hold durations.
3. To test, leave the target game or app behind Titano Trigger and click **Test**. The utility minimizes and sends input to the previously active window.
4. Click **Start Live Control** and keep the target game or app active.

The window's close button minimizes the app to the system tray. Double-click the tray icon to reopen it. To quit completely, right-click the icon and choose **Exit**.

## Behavior and limitations

The app checks the 50 most recent Reymit donations approximately every two seconds. It processes only donations recorded after live control starts. Reymit does not publish a documented real-time donation notification API for this use, so network and polling intervals affect response time.

Input is sent to the foreground window through the standard Windows input API. If a game runs as administrator, run Titano Trigger at the same privilege level. Some games and anti-cheat systems may reject simulated input. Donofa is disabled because a public, official donation API for this use has not been verified.

## Privacy and security

- The Reymit token is not stored in source code, settings, or build output; it must be entered again each time the app runs.
- The token is held in memory only while the app is running.
- Non-secret rules are stored in `%LOCALAPPDATA%\\TitanoTrigger\\settings.json`.
- Build output, local dependencies, temporary archives, and local configuration files are excluded through `.gitignore`.
- The Windows build is not digitally signed, so Windows SmartScreen may show a warning.

## Build from source

Build on Windows with the .NET 10 SDK:

```powershell
dotnet restore desktop/TitanoTrigger.csproj
dotnet publish desktop/TitanoTrigger.csproj -c Release -o dist/Titano-Trigger-Release
```

The desktop app is in `desktop`, and the UI assets are in `src/ui`. The Vazirmatn font license is included at `src/ui/LICENSE`.

Reymit API documentation: <https://reymit.ir/APIs>
