# Video Downloader v2.0.0 Package

This folder contains the v2 source snapshot, the current Windows Release output, and the MSI installer input used by the WiX project.

## Repository & Links

- **Repository**: [Blackswan-Yazilim/video-downloader](https://github.com/Blackswan-Yazilim/video-downloader)
- **Release Download URL**: `https://github.com/Blackswan-Yazilim/video-downloader/releases/download/v2.0.0/VideoDownloader-2.0.0-Setup.msi`
- **Installer SHA-256**: `FD4BED29E97D07F4FD4EF656B405DC609E323C880E60911F3E681DEA3E0AE868`
- **ProductCode**: `{32BBBB78-C4EC-4ED4-BBED-87E675920550}`
- **UpgradeCode**: `{A6A7B2F7-8B1D-4A07-9E20-C3AB1DBBC0A4}`

## Contents

- `Source/v2-webui/`: v2 C# source, project file, and WebView UI assets (.NET 8 + WinForms + WebView2).
- `Release/`: Build output including the app, WebView2 bridge dependencies, yt-dlp, FFmpeg, and UI files.
- `Installer/VideoDownloader-2.0.0-Setup.msi`: WiX Toolset MSI installer (SHA256 verified).

## Winget Distribution

The winget manifests are configured in:
- `packaging/winget/`: Local package manifest files.
- `packaging/winget-pkgs/manifests/k/kayapater/VideoDownloader/2.0.0/`: Ready-to-commit directory tree for submitting a PR to [microsoft/winget-pkgs](https://github.com/microsoft/winget-pkgs) (following PR #377396 pattern).

### Dependencies Declared

- `Microsoft.DotNet.DesktopRuntime.8` (>= 8.0.0)
- `Microsoft.EdgeWebView2Runtime`

### Validation Command

```powershell
winget validate --manifest packaging/winget
```

