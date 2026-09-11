using System;
using System.Drawing;

namespace VideoDownloader
{
    /// <summary>
    /// Application-wide constants replacing magic numbers scattered across the codebase.
    /// </summary>
    internal static class AppConstants
    {
        // ── Form dimensions ──────────────────────────────────────────
        public const int FormWidth = 770;
        public const int FormMinWidth = 788;

        public const int FormCollapsedHeight = 458;
        public const int FormCollapsedMinHeight = 505;

        public const int FormWithPreviewHeight = 575;
        public const int FormWithPreviewMinHeight = 620;

        public const int FormWithProgressHeight = 485;
        public const int FormWithProgressMinHeight = 532;

        // ── Preview panel expand/collapse offsets ────────────────────
        public const int PreviewExpandOffsetY = 115;

        // ── Timing ───────────────────────────────────────────────────
        public const int MetadataTimeoutMs = 15000;
        public const int InstallDelayMs = 2000;
        public const int HttpClientTimeoutSeconds = 30;

        // ── UI spacing ───────────────────────────────────────────────

        // Collapsed layout Y positions
        public const int QualityLabelCollapsedY = 145;
        public const int QualityComboBoxCollapsedY = 175;
        public const int SubtitleCheckBoxCollapsedY = 182;
        public const int PathLabelCollapsedY = 230;
        public const int PathTextBoxCollapsedY = 260;
        public const int BrowseButtonCollapsedY = 258;
        public const int DownloadButtonCollapsedY = 320;
        public const int ProgressPanelCollapsedY = 390;

        // Expanded layout Y positions (preview visible)
        public const int QualityLabelExpandedY = 260;
        public const int QualityComboBoxExpandedY = 290;
        public const int SubtitleCheckBoxExpandedY = 297;
        public const int PathLabelExpandedY = 345;
        public const int PathTextBoxExpandedY = 375;
        public const int BrowseButtonExpandedY = 373;
        public const int DownloadButtonExpandedY = 435;
        public const int ProgressPanelExpandedY = 505;

        // ── Registry ─────────────────────────────────────────────────
        public const string RegistryKeyPath = @"SOFTWARE\VideoDownloader";
        public const string RegistryValueLanguage = "Language";
        public const string RegistryValueTheme = "Theme";

        // ── Default paths ────────────────────────────────────────────
        public static string DefaultDownloadPath => System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
            "Video Downloader"
        );

        // ── Brand colors (Indigo-500 primary) ────────────────────────
        public static readonly Color PrimaryColor = Color.FromArgb(99, 102, 241);
        public static readonly Color SuccessColor = Color.FromArgb(34, 197, 94);
        public static readonly Color DangerColor = Color.FromArgb(239, 68, 68);
        public static readonly Color WarningColor = Color.FromArgb(234, 179, 8);
        public static readonly Color GrayColor = Color.FromArgb(107, 114, 128);

        // ── Tool subfolder ───────────────────────────────────────────
        public static string ManagedToolsDirectory => System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VideoDownloader",
            "tools"
        );
    }
}
