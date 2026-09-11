namespace VideoDownloader
{
    internal static class DependencyVersions
    {
        // Bundled binary versions shipped with the app.
        public const string YtDlp = "2026.03.17";
        public const string FFmpeg = "release"; // Uses gyan.dev latest essentials build

        // Key managed dependency declared in the project file.
        public const string NewtonsoftJson = "13.0.3";

        // ── Download URLs ───────────────────────────────────────────
        public const string YtDlpDownloadUrl = "https://github.com/yt-dlp/yt-dlp/releases/download/2026.03.17/yt-dlp.exe";
        public const string FFmpegDownloadUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip";

        // ── SHA256 checksums for downloaded binaries ─────────────────
        // Update these when bumping YtDlp or FFmpeg versions.
        // Set to empty string to skip verification (not recommended for production).
        public const string YtDlpSha256 = "";
        public const string FFmpegSha256 = "";
    }
}
