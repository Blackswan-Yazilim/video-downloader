using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace VideoDownloader.Services
{
    /// <summary>
    /// Manages external tool dependencies: yt-dlp and FFmpeg.
    /// Handles detection, download, and installation of both tools.
    /// Extracted from MainForm.cs.
    /// </summary>
    public class DependencyManager
    {
        private readonly HttpClient _httpClient;

        // Track whether we're using standalone yt-dlp.exe or the Python module.
        private bool _useStandaloneYtDlp;
        private string? _standaloneYtDlpPath;

        public bool UseStandaloneYtDlp => _useStandaloneYtDlp;
        public string? StandaloneYtDlpPath => _standaloneYtDlpPath;

        public DependencyManager(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        // ── Path helpers ─────────────────────────────────────────────

        public static string GetManagedToolsDirectory() => AppConstants.ManagedToolsDirectory;

        public static string GetManagedYtDlpPath() =>
            Path.Combine(GetManagedToolsDirectory(), "yt-dlp.exe");

        public static string GetManagedFFmpegDirectory() =>
            Path.Combine(GetManagedToolsDirectory(), $"ffmpeg-{DependencyVersions.FFmpeg}");

        // ── FFmpeg local detection ──────────────────────────────────

        public bool TryGetLocalFFmpegDirectory(out string directoryPath)
        {
            var managedDir = GetManagedFFmpegDirectory();
            if (File.Exists(Path.Combine(managedDir, "ffmpeg.exe")))
            {
                directoryPath = managedDir;
                return true;
            }

            var appDir = AppContext.BaseDirectory;
            if (File.Exists(Path.Combine(appDir, "ffmpeg.exe")))
            {
                directoryPath = appDir;
                return true;
            }

            var startupDir = Application.StartupPath;
            if (File.Exists(Path.Combine(startupDir, "ffmpeg.exe")))
            {
                directoryPath = startupDir;
                return true;
            }

            directoryPath = string.Empty;
            return false;
        }

        // ── yt-dlp standalone detection ─────────────────────────────

        public bool TryUseStandaloneYtDlp()
        {
            string? path = null;

            var managedPath = GetManagedYtDlpPath();
            if (File.Exists(managedPath))
                path = managedPath;
            else
            {
                var appDirPath = Path.Combine(AppContext.BaseDirectory, "yt-dlp.exe");
                if (File.Exists(appDirPath))
                    path = appDirPath;
            }

            if (path == null)
            {
                var startupPath = Path.Combine(Application.StartupPath, "yt-dlp.exe");
                if (File.Exists(startupPath))
                    path = startupPath;
            }

            if (path != null)
            {
                _useStandaloneYtDlp = true;
                _standaloneYtDlpPath = path;
                return true;
            }

            _useStandaloneYtDlp = false;
            _standaloneYtDlpPath = null;
            return false;
        }

        // ── Python check ─────────────────────────────────────────────

        public async Task<bool> CheckPythonInstalledAsync()
        {
            // If standalone yt-dlp.exe is available, Python is not required.
            if (TryUseStandaloneYtDlp()) return true;

            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processInfo };
                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await Task.Run(() => process.WaitForExit());

                return process.ExitCode == 0 && !string.IsNullOrEmpty(output);
            }
            catch { return false; }
        }

        // ── yt-dlp check ────────────────────────────────────────────

        public async Task<bool> CheckYtDlpInstalledAsync()
        {
            // 1. First check for bundled standalone yt-dlp.exe
            if (TryUseStandaloneYtDlp()) return true;

            // 2. Fall back to Python module
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "-m yt_dlp --version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processInfo };
                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await Task.Run(() => process.WaitForExit());

                return process.ExitCode == 0 && !string.IsNullOrEmpty(output);
            }
            catch { return false; }
        }

        // ── FFmpeg check ────────────────────────────────────────────

        public async Task<bool> CheckFFmpegInstalledAsync()
        {
            try
            {
                // 1. Check managed/local folders
                if (TryGetLocalFFmpegDirectory(out _)) return true;

                // 2. Check PATH
                var processInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processInfo };
                process.Start();
                await Task.Run(() => process.WaitForExit());

                return process.ExitCode == 0;
            }
            catch { return false; }
        }

        // ── yt-dlp installation ─────────────────────────────────────

        public async Task<bool> InstallYtDlpAsync()
        {
            // If standalone yt-dlp.exe is already available, nothing to do.
            if (TryUseStandaloneYtDlp()) return true;

            string toolsDir = GetManagedToolsDirectory();
            string targetPath = GetManagedYtDlpPath();
            string tempPath = targetPath + ".download";

            try
            {
                Directory.CreateDirectory(toolsDir);

                string downloadUrl = DependencyVersions.YtDlpDownloadUrl;

                using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                await using (var sourceStream = await response.Content.ReadAsStreamAsync())
                await using (var targetStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await sourceStream.CopyToAsync(targetStream);
                }

                // SHA256 verification
                if (!string.IsNullOrEmpty(DependencyVersions.YtDlpSha256))
                {
                    string actualHash = ComputeSha256(tempPath);
                    if (!string.Equals(actualHash, DependencyVersions.YtDlpSha256, StringComparison.OrdinalIgnoreCase))
                    {
                        try { File.Delete(tempPath); } catch { }
                        return false; // Hash mismatch — possible tampering
                    }
                }

                File.Move(tempPath, targetPath, true);
                _useStandaloneYtDlp = true;
                _standaloneYtDlpPath = targetPath;
                return await CheckYtDlpInstalledAsync();
            }
            catch
            {
                // Fallback to Python-based pip installation below
            }
            finally
            {
                try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { }
            }

            // Python-based fallback
            if (!await CheckPythonInstalledAsync()) return false;

            try
            {
                // Install curl-cffi for Cloudflare impersonate support
                var curlInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "-m pip install --upgrade curl-cffi",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using var curlProc = new Process { StartInfo = curlInfo };
                curlProc.Start();
                await Task.Run(() => curlProc.WaitForExit());

                // Install pinned yt-dlp
                var pipInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"-m pip install --upgrade yt-dlp=={DependencyVersions.YtDlp}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using var pipProc = new Process { StartInfo = pipInfo };
                pipProc.Start();
                await Task.Run(() => pipProc.WaitForExit());

                if (pipProc.ExitCode == 0)
                {
                    await Task.Delay(AppConstants.InstallDelayMs);
                    return await CheckYtDlpInstalledAsync();
                }
            }
            catch { }

            return false;
        }

        // ── FFmpeg installation ─────────────────────────────────────

        public async Task<bool> InstallFFmpegAsync()
        {
            if (await CheckFFmpegInstalledAsync()) return true;

            string tempRoot = Path.Combine(Path.GetTempPath(), "VideoDownloader", $"ffmpeg-{Guid.NewGuid():N}");
            string zipPath = Path.Combine(tempRoot, "ffmpeg.zip");
            string extractPath = Path.Combine(tempRoot, "extract");

            try
            {
                Directory.CreateDirectory(tempRoot);

                string downloadUrl = DependencyVersions.FFmpegDownloadUrl;
                using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                await using (var sourceStream = await response.Content.ReadAsStreamAsync())
                await using (var targetStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await sourceStream.CopyToAsync(targetStream);
                }

                // SHA256 verification
                if (!string.IsNullOrEmpty(DependencyVersions.FFmpegSha256))
                {
                    string actualHash = ComputeSha256(zipPath);
                    if (!string.Equals(actualHash, DependencyVersions.FFmpegSha256, StringComparison.OrdinalIgnoreCase))
                    {
                        return false; // Hash mismatch
                    }
                }

                ZipFile.ExtractToDirectory(zipPath, extractPath, true);

                var ffmpegExePath = Directory.GetFiles(extractPath, "ffmpeg.exe", SearchOption.AllDirectories).FirstOrDefault();
                if (string.IsNullOrEmpty(ffmpegExePath)) return false;

                string sourceDir = Path.GetDirectoryName(ffmpegExePath) ?? string.Empty;
                if (string.IsNullOrEmpty(sourceDir)) return false;

                string targetDir = GetManagedFFmpegDirectory();
                Directory.CreateDirectory(targetDir);

                foreach (var tool in new[] { "ffmpeg.exe", "ffprobe.exe", "ffplay.exe" })
                {
                    var sourceTool = Path.Combine(sourceDir, tool);
                    if (!File.Exists(sourceTool)) continue;
                    File.Copy(sourceTool, Path.Combine(targetDir, tool), true);
                }

                return File.Exists(Path.Combine(targetDir, "ffmpeg.exe"));
            }
            catch
            {
                return false;
            }
            finally
            {
                try { if (Directory.Exists(tempRoot)) Directory.Delete(tempRoot, true); } catch { }
            }
        }

        // ── SHA256 helper ────────────────────────────────────────────

        private static string ComputeSha256(string filePath)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            using var stream = File.OpenRead(filePath);
            byte[] hash = sha.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
