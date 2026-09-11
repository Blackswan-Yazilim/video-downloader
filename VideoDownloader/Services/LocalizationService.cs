using System.Collections.Generic;

namespace VideoDownloader.Services
{
    /// <summary>
    /// In-memory localization service with fallback to key name.
    /// Provides Turkish and English translations for all UI strings.
    /// Extracted from MainForm.cs InitializeTranslations().
    /// </summary>
    public class LocalizationService
    {
        private readonly Dictionary<string, Dictionary<AppLanguage, string>> _translations;

        public LocalizationService()
        {
            _translations = new Dictionary<string, Dictionary<AppLanguage, string>>
            {
                ["FormTitle"] = Tr("Video İndirici", "Video Downloader"),
                ["VideoURL"] = Tr("Video URL", "Video URL"),
                ["URLPlaceholder"] = Tr("YouTube, Twitter, Instagram linkini yapıştırın...", "Paste YouTube, Twitter, Instagram link..."),
                ["Quality"] = Tr("Kalite", "Quality"),
                ["DownloadSubtitles"] = Tr("Altyazı İndir", "Download Subtitles"),
                ["DownloadPath"] = Tr("İndirme Klasörü", "Download Folder"),
                ["Browse"] = Tr("Gözat", "Browse"),
                ["Download"] = Tr("⬇️ İndir", "⬇️ Download"),
                ["Ready"] = Tr("Hazır", "Ready"),
                ["Settings"] = Tr("Ayarlar", "Settings"),
                ["Language"] = Tr("Dil", "Language"),
                ["Turkish"] = Tr("Türkçe", "Turkish"),
                ["English"] = Tr("İngilizce", "English"),
                ["Theme"] = Tr("Tema", "Theme"),
                ["LightTheme"] = Tr("Açık Tema", "Light Theme"),
                ["DarkTheme"] = Tr("Koyu Tema", "Dark Theme"),
                ["OceanTheme"] = Tr("🌊 Okyanus", "🌊 Ocean"),
                ["ForestTheme"] = Tr("🌲 Orman", "🌲 Forest"),
                ["SunsetTheme"] = Tr("🌅 Gün Batımı", "🌅 Sunset"),
                ["PurpleTheme"] = Tr("💜 Mor Rüya", "💜 Purple Dreams"),
                ["RoseTheme"] = Tr("🌹 Gül", "🌹 Rose"),
                ["MidnightTheme"] = Tr("🌙 Gece Mavisi", "🌙 Midnight"),
                ["Help"] = Tr("Yardım", "Help"),
                ["About"] = Tr("Hakkında", "About"),
                ["AboutTitle"] = Tr("Video İndirici Hakkında", "About Video Downloader"),
                ["AppDescription"] = Tr("Video İndirici v1.6.2\n\nYouTube, Twitter, Instagram ve 50+ platformdan video indirme aracı", "Video Downloader v1.6.2\n\nDownload videos from YouTube, Twitter, Instagram and 50+ platforms"),
                ["Developer"] = Tr("Geliştirici:", "Developer:"),
                ["OK"] = Tr("Tamam", "OK"),
                ["VideoOption"] = Tr("🎬 Video", "🎬 Video"),
                ["AudioOption"] = Tr("🎵 Ses", "🎵 Audio"),
                ["SupportedSites"] = Tr("Desteklenen Siteler", "Supported Sites"),
                ["Cancel"] = Tr("İptal", "Cancel"),
                ["Pause"] = Tr("⏸ Duraklat", "⏸ Pause"),
                ["Resume"] = Tr("▶ Devam", "▶ Resume"),
                ["Paused"] = Tr("Duraklatıldı", "Paused"),
                ["Downloading"] = Tr("İndiriliyor...", "Downloading..."),
                ["Completed"] = Tr("Tamamlandı!", "Completed!"),
                ["Error"] = Tr("Hata", "Error"),
                ["Success"] = Tr("Başarılı", "Success"),
                ["SystemCheck"] = Tr("Sistem Kontrolü", "System Check"),
                ["CheckingDependencies"] = Tr("Bağımlılıklar kontrol ediliyor...", "Checking dependencies..."),
                ["InstallingYtDlp"] = Tr("yt-dlp kuruluyor...", "Installing yt-dlp..."),
                ["YtDlpInstalled"] = Tr("yt-dlp başarıyla kuruldu!", "yt-dlp installed successfully!"),
                ["YtDlpInstallFailed"] = Tr("yt-dlp kurulumu başarısız!", "yt-dlp installation failed!"),
                ["InstallingFFmpeg"] = Tr("FFmpeg kuruluyor...", "Installing FFmpeg..."),
                ["FFmpegInstalled"] = Tr("FFmpeg başarıyla kuruldu!", "FFmpeg installed successfully!"),
                ["FFmpegInstallFailed"] = Tr("FFmpeg kurulumu başarısız!", "FFmpeg installation failed!"),
                ["PythonNotFound"] = Tr("Python bulunamadı! Lütfen python.org adresinden Python kurun.", "Python not found! Please install Python from python.org"),
                ["AllDependenciesOk"] = Tr("✓ Tüm bağımlılıklar hazır", "✓ All dependencies ready"),
                ["Processing"] = Tr("İşleniyor...", "Processing..."),
                ["Starting"] = Tr("Başlatılıyor...", "Starting..."),
                ["Cancelled"] = Tr("İptal edildi", "Cancelled"),
                ["UnknownChannel"] = Tr("Bilinmeyen Kanal", "Unknown Channel"),
                ["LoadingPreview"] = Tr("Video bilgileri yükleniyor...", "Loading video info..."),
                ["PasteTooltip"] = Tr("📋 Yapıştır", "📋 Paste"),
                ["BrowseTooltip"] = Tr("📁 Gözat", "📁 Browse"),
                ["UrlPlaceholderShort"] = Tr("📋 Buraya yapıştır", "📋 Paste here"),
                // Download result messages
                ["VideoDownloaded"] = Tr("Video başarıyla indirildi!", "Video downloaded successfully!"),
                ["AudioDownloaded"] = Tr("Ses başarıyla indirildi!", "Audio downloaded successfully!"),
                ["DownloadComplete"] = Tr("İndirme tamamlandı!\n\nKlasörü açmak ister misiniz?", "Download complete!\n\nWould you like to open the folder?"),
                ["DownloadCancelled"] = Tr("İndirme iptal edildi", "Download cancelled"),
                ["DownloadFailed"] = Tr("İndirme başarısız!", "Download failed!"),
                // Error messages
                ["NoUrl"] = Tr("Lütfen bir video URL'si girin!", "Please enter a video URL!"),
                ["NoPath"] = Tr("Lütfen indirme yolunu belirtin!", "Please specify download path!"),
                ["FolderCreateFailed"] = Tr("İndirme klasörü oluşturulamadı: {0}", "Could not create download folder: {0}"),
                ["YtDlpInstallFailedMsg"] = Tr("yt-dlp kurulumu başarısız!", "yt-dlp installation failed!"),
                ["FFmpegRequiredMp3"] = Tr("FFmpeg bulunamadı! MP3 indirme için FFmpeg gereklidir.", "FFmpeg not found! FFmpeg is required for MP3 download."),
                ["FFmpegRequiredTwitch"] = Tr("FFmpeg bulunamadı! Twitch indirmeleri için FFmpeg gereklidir.", "FFmpeg not found! FFmpeg is required for Twitch downloads."),
                ["FFmpegRequired"] = Tr("FFmpeg bulunamadı! FFmpeg bu program için zorunludur. Lütfen FFmpeg'i yükleyin.", "FFmpeg not found! FFmpeg is required for this program. Please install FFmpeg."),
                ["ExitConfirmTitle"] = Tr("Çıkış Onayı", "Exit Confirmation"),
                ["ExitConfirmMessage"] = Tr("İndirme işlemi devam ediyor. Çıkmak istediğinizden emin misiniz?", "Download is in progress. Are you sure you want to exit?"),
                ["Warning"] = Tr("Uyarı", "Warning"),
                // Quality labels
                ["BestQuality"] = Tr("En İyi Kalite", "Best Quality"),
                // System check labels
                ["SystemStatus"] = Tr("🔧 Sistem Durumu", "🔧 System Status"),
                ["CheckingPython"] = Tr("⏳ Python kontrol ediliyor...", "⏳ Checking Python..."),
                ["CheckingYtDlp"] = Tr("⏳ yt-dlp kontrol ediliyor...", "⏳ Checking yt-dlp..."),
                ["CheckingFFmpeg"] = Tr("⏳ FFmpeg kontrol ediliyor...", "⏳ Checking FFmpeg..."),
                ["PythonNotRequired"] = Tr("✅ Python gerekmez (standalone yt-dlp)", "✅ Python not required (standalone yt-dlp)"),
                ["PythonInstalled"] = Tr("✅ Python kurulu", "✅ Python installed"),
                ["PythonNotFoundCheck"] = Tr("❌ Python bulunamadı", "❌ Python not found"),
                ["YtDlpInstalledCheck"] = Tr("✅ yt-dlp kurulu", "✅ yt-dlp installed"),
                ["YtDlpNotFoundCheck"] = Tr("❌ yt-dlp bulunamadı", "❌ yt-dlp not found"),
                ["FFmpegInstalledCheck"] = Tr("✅ FFmpeg kurulu", "✅ FFmpeg installed"),
                ["FFmpegNotFoundCheck"] = Tr("❌ FFmpeg bulunamadı (zorunlu)", "❌ FFmpeg not found (required)"),
                ["AllReady"] = Tr("✅ Tüm gerekli bağımlılıklar hazır!", "✅ All required dependencies ready!"),
                ["PythonRequired"] = Tr("Python gerekli! python.org adresinden indirin.", "Python required! Download from python.org"),
                ["InstallYtDlpBtn"] = Tr("yt-dlp Kur", "Install yt-dlp"),
                ["Installing"] = Tr("Kuruluyor...", "Installing..."),
                ["InstallingYtDlpWait"] = Tr("yt-dlp kuruluyor, lütfen bekleyin...", "Installing yt-dlp, please wait..."),
                ["YtDlpSuccess"] = Tr("✅ yt-dlp başarıyla kuruldu!", "✅ yt-dlp installed successfully!"),
                ["YtDlpFailed"] = Tr("❌ yt-dlp kurulumu başarısız!", "❌ yt-dlp installation failed!"),
                ["Retry"] = Tr("Tekrar Dene", "Retry"),
                ["YtDlpNotInstalledMsg"] = Tr("yt-dlp kurulu değil. Kurmak için butona tıklayın.", "yt-dlp not installed. Click button to install."),
                // Download failed dialog
                ["DownloadFailedMsg"] = Tr("İndirme başarısız!\n\n{0}\n\nLütfen hata detayını kontrol edip tekrar deneyin.", "Download failed!\n\n{0}\n\nPlease check the error details and try again."),
                ["PauseError"] = Tr("Duraklatma hatası: {0}", "Pause error: {0}"),
                ["Resuming"] = Tr("Devam ediliyor...", "Resuming..."),
                ["BrowseDialogTitle"] = Tr("İndirilen videoların kaydedileceği klasörü seçin", "Select the folder to save downloaded videos"),
            };
        }

        /// <summary>
        /// Gets the translated string for the given key and language.
        /// Returns the key itself if no translation is found (safe fallback).
        /// </summary>
        public string GetText(string key, AppLanguage language)
        {
            if (_translations.TryGetValue(key, out var langDict) &&
                langDict.TryGetValue(language, out var text))
            {
                return text;
            }
            return key; // Safe fallback
        }

        /// <summary>
        /// Gets a formatted translation.
        /// </summary>
        public string GetFormattedText(string key, AppLanguage language, params object[] args)
        {
            var format = GetText(key, language);
            return args.Length > 0 ? string.Format(format, args) : format;
        }

        // ── Helper for concise dictionary initialization ─────────────

        private static Dictionary<AppLanguage, string> Tr(string turkish, string english)
        {
            return new Dictionary<AppLanguage, string>
            {
                [AppLanguage.Turkish] = turkish,
                [AppLanguage.English] = english
            };
        }
    }
}
