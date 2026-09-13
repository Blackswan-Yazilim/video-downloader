# Video Downloader

<p align="center">
  <img src="VideoDownloader/logo.png" width="130" alt="Video Downloader Logo" />
</p>

<div align="center">

**Modern, fast, and user-friendly video downloader for Windows**

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![WebView2](https://img.shields.io/badge/WebView2-1.0.4129-blue)](https://developer.microsoft.com/en-us/microsoft-edge/webview2/)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%2B-blue)](https://www.microsoft.com/windows)
[![Release](https://img.shields.io/badge/Release-v2.0.1-10B981)](https://github.com/Blackswan-Yazilim/video-downloader/releases)
[![Downloads](https://img.shields.io/github/downloads/Blackswan-Yazilim/video-downloader/total?color=0ea5e9&label=Downloads)](https://github.com/Blackswan-Yazilim/video-downloader/releases)

</div>

---

**Video Downloader** is a modern Windows desktop application that enables downloading videos and audio from YouTube, Twitter/X, Instagram, TikTok, Facebook, Twitch, Kick, and 50+ other platforms with highest available qualities (up to 4K Ultra HD) or audio extraction (MP3).

## ✨ What's New in v2.0.1

### ⚡ Stability & UI Polish (v2.0.1)
- **Resolved Download Stalls:** Fixed standard output/error stream buffering deadlocks during metadata extraction and video downloads.
- **True Desktop App Feel:** Completely disabled browser-like link URL status bar popups, F12 developer tools, and default browser context menus.
- **Refined Download Cards:** Active download cards now display clean, truncated video titles instead of raw URLs, and the cancel action button stays neatly within container boundaries.
- **Enhanced Error Diagnostics:** Added clear user-facing error reporting with actionable troubleshooting advice.

### 🚀 Modern WebView2 & Tailwind Architecture
- **Complete UI Redesign:** Rebuilt from scratch with Microsoft Edge WebView2, Tailwind CSS, and custom glassmorphism components.
- **Dynamic Themes:** Smooth Dark, Light, and High Contrast theme toggling.
- **Full Localization:** Instant bilingual support (Turkish & English) with persistent settings.
- **Active Download Manager:** Real-time progress bars, speed, ETA, and completed download history tracking.
- **Direct System Integration:** Native shell execution for links, folder opening, and media playback.

### 📦 Installation

#### Via Windows Package Manager (Winget)
```powershell
winget install kayapater.VideoDownloader
```

#### Via GitHub Releases (MSI Installer)
Download the latest `VideoDownloader-2.0.1-Setup.msi` installer from the [Releases](https://github.com/Blackswan-Yazilim/video-downloader/releases/latest) page and run the setup wizard.

---

## 🛠️ Building from Source

### Prerequisites
- Windows 10 (Build 19041+) or Windows 11
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Microsoft Edge WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/)

### Build Instructions
```powershell
# Clone repository
git clone https://github.com/Blackswan-Yazilim/video-downloader.git
cd video-downloader

# Build solution in Release configuration
dotnet build VideoDownloader.sln -c Release
```

The compiled application and its Web assets will be located in:
`VideoDownloader\bin\Release\net8.0-windows10.0.19041.0\`

---

## 📜 License
This project is licensed under the [MIT License](LICENSE).
