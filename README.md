# Video Downloader

<div align="center">

![Video Downloader](VideoDownloader/logo.png)

**Modern and user-friendly video downloader**

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%2B-blue)](https://www.microsoft.com/windows)
[![Release](https://img.shields.io/badge/Release-v1.6.2-512BD4)](https://github.com/kayapater/video-downloader/releases)
[![Downloads](https://img.shields.io/github/downloads/kayapater/video-downloader/total?label=Downloads&color=blue)](https://github.com/kayapater/video-downloader/releases)

</div>

---

Video Downloader is a powerful and user-friendly Windows application that allows you to download videos from YouTube, Twitter, Instagram, TikTok, Facebook, and 50+ platforms.

### ✨ What's New in v1.6.2 — The Modularity Update

#### 🏗️ Major Architectural Refactoring
- **MainForm.cs reduced by 65%** (2300 → ~800 lines) by extracting:
  - `NativeMethods` — centralized Windows P/Invoke declarations
  - `SettingsManager` — registry-backed settings persistence
  - `DependencyManager` — yt-dlp / FFmpeg detection, download, and installation
  - `ThemeService` — 8-theme engine with `ThemeColors` records
  - `LocalizationService` — in-memory translation engine with safe key fallback
- **Dedicated UI Forms:** `AboutForm`, `SupportedSitesForm`, `SystemCheckForm`
- **Eliminated P/Invoke duplication** across MainForm and YtDlpService

#### 🔒 Security
- **SHA256 checksum verification** for downloaded yt-dlp.exe and FFmpeg binaries
- Hash constants declared in `DependencyVersions.cs` — set them to enable verification
- Shared `HttpClientFactory` with proper timeout and User-Agent headers

#### 🎨 UX Improvements
- **WebP thumbnail support** via SkiaSharp — YouTube thumbnails now display correctly
- **Quality selection modeled as enum** (`QualityOption`) — no more fragile string parsing
- All magic numbers replaced with `AppConstants` for DPI-aware layout management

#### 🐛 Bug Fixes
- **Progress parsing fixed:** now uses `CultureInfo.InvariantCulture` — works on all locales
- **Race condition resolved** in `YtDlpService` process disposal with lock synchronization
- **Process timeout handling:** zombie processes now killed after metadata timeout
- Removed unused fields in `RoundedButton`

#### 📦 New Dependencies
- `SkiaSharp` + `SkiaSharp.Views.WindowsForms` — cross-format image decoding
- `Microsoft.Extensions.Logging` — structured logging infrastructure

### 📌 Previous Highlights (v1.6.1)

#### ✅ Kick URL Parsing Fix
- Fixed argument parsing issues that could cause valid Kick VOD URLs to fail.

#### 🔧 Dependency Installation Improvements
- Improved startup dependency flow to auto-install standalone `yt-dlp` and `FFmpeg`.

---

### 🎯 Features

#### 📹 Video Downloading
- 50+ platform support (YouTube, Twitter, Instagram, TikTok, Facebook, etc.)
- Multiple quality options (360p - 4K)
- Audio extraction (MP3)
- Subtitle download support
- Playlist support

#### 🎨 User Interface
- Modern and clean design
- **8 beautiful themes** (Light, Dark, Ocean, Forest, Sunset, Purple, Rose, Midnight)
- Turkish and English language support
- Simple and clear progress display
- Pause and resume downloads

#### ⚙️ Technical
- .NET 8.0 Windows Forms
- yt-dlp (Python video download module)
- FFmpeg (video/audio processing)
- Automatic dependency management

### 📦 Installation (v1.6.1+)

#### Via Microsoft Winget (Recommended)
You can install or update Video Downloader directly from the Windows Package Manager:
```powershell
winget install kayapater.VideoDownloader
```

#### Manual Installation
1. Download the latest `VideoDownloader-v1.6.1-Setup.msi` from the [Releases](https://github.com/kayapater/video-downloader/releases/latest) page.
2. Run the MSI installer.
3. Installer includes bundled `yt-dlp.exe` and `FFmpeg` binaries (`ffmpeg/ffprobe/ffplay`) for offline-ready usage.
4. On startup, the app uses bundled tools first and only falls back to Python/pip-based install when standalone `yt-dlp.exe` is not available.

---

### 🏗️ Architectural Refactoring (SOLID)
- **Modular Design:** Migrated from a monolithic `MainForm.cs` to a modular **Service/Strategy pattern**.
- **Smart Dependency Management:** Improved system for detecting and auto-installing pinned `yt-dlp` and `FFmpeg` dependencies.
- **Pinned Dependency Versions:** Bundled `yt-dlp` and `FFmpeg` versions are declared in `VideoDownloader/DependencyVersions.cs` and exported into `bundled-dependencies.json` during packaging.


### 🛠️ Development

#### Requirements

- Visual Studio 2022 or JetBrains Rider
- .NET 8.0 SDK
- Windows 10 SDK (10.0.19041.0)

#### Building the Project

```powershell
# Clone
git clone https://github.com/kayapater/video-downloader.git
cd video-downloader

# Restore dependencies
dotnet restore

# Debug build
dotnet build -c Debug

# Release build
dotnet build -c Release

# Optional: prepare publish folder with bundled yt-dlp + ffmpeg binaries
powershell -ExecutionPolicy Bypass -File .\scripts\Prepare-BundledPublish.ps1
```

### 🌍 Supported Platforms

<details>
<summary>50+ Platform List (Click to expand)</summary>

#### 📺 Main Platforms
- YouTube
- Instagram
- TikTok
- Twitter/X
- Facebook
- Twitch
- Kick
- Vimeo
- Dailymotion
- Reddit
- LinkedIn

#### 🎵 Music Platforms
- SoundCloud
- Bandcamp
- Mixcloud

#### 📚 Education Platforms
- Udemy
- Coursera
- Khan Academy

#### 🌏 International
- Bilibili
- Niconico
- VK
- Odnoklassniki

And many more...

</details>

### 🤝 Contributing

Contributions are welcome! Please:

1. Fork this repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### 📄 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

### 👤 Developer

**kayapater**

- Twitter/X: [@kayapater](https://x.com/kayapater)
- GitHub: [@kayapater](https://github.com/kayapater)

### 🙏 Acknowledgments

- [yt-dlp](https://github.com/yt-dlp/yt-dlp) - Video download engine
- [FFmpeg](https://ffmpeg.org/) - Video/audio processing
- [Newtonsoft.Json](https://www.newtonsoft.com/json) - JSON processing

---

<div align="center">

**Made with ❤️ by kayapater**

⭐ Star this repo if you find it helpful!

</div>
