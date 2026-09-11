using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VideoDownloader.Interop;
using VideoDownloader.Models;
using VideoDownloader.Strategies;

namespace VideoDownloader.Services
{
    public class YtDlpService
    {
        private readonly List<IPlatformStrategy> _strategies;
        private Process? _currentProcess;
        private bool _isCancelled;
        private bool _isPaused;
        private string _lastErrorLine = string.Empty;
        private readonly object _processLock = new object();
        private static readonly Regex ArgumentTokenizer = new Regex("\"[^\"]*\"|\\S+", RegexOptions.Compiled);

        public event Action<string>? OutputReceived;
        public event Action<double, string>? ProgressChanged;
        public event Action<bool, string>? DownloadCompleted;

        public YtDlpService()
        {
            _strategies = new List<IPlatformStrategy>
            {
                new KickStrategy(),
                new TwitchStrategy(),
                new DefaultStrategy()
            };
        }

        public async Task<VideoMetadata?> GetVideoMetadataAsync(string url, bool useStandalone, string? standalonePath, CancellationToken token)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var strategy = _strategies.FirstOrDefault(s => s.CanHandle(url)) ?? new DefaultStrategy();
                    string extraArgs = strategy.GetExtraArguments(url);

                    bool runStandalone = useStandalone && !string.IsNullOrEmpty(standalonePath);
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = runStandalone ? standalonePath! : "python",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = System.Text.Encoding.UTF8
                    };

                    if (!runStandalone)
                    {
                        startInfo.ArgumentList.Add("-m");
                        startInfo.ArgumentList.Add("yt_dlp");
                    }

                    AddArgumentsFromString(startInfo, extraArgs);
                    startInfo.ArgumentList.Add("--no-download");
                    startInfo.ArgumentList.Add("--print-json");
                    startInfo.ArgumentList.Add(url);

                    using var process = new Process { StartInfo = startInfo };
                    process.Start();

                    var output = process.StandardOutput.ReadToEnd();
                    if (!process.WaitForExit(AppConstants.MetadataTimeoutMs))
                    {
                        // Timeout — kill the process to avoid zombie
                        try { process.Kill(entireProcessTree: true); } catch { }
                        return null;
                    }

                    if (token.IsCancellationRequested) return null;

                    if (!string.IsNullOrEmpty(output))
                    {
                        using var doc = JsonDocument.Parse(output);
                        var root = doc.RootElement;

                        string thumbnailUrl = "";
                        if (root.TryGetProperty("thumbnails", out var thumbnails) && thumbnails.ValueKind == JsonValueKind.Array)
                        {
                            var thumbList = thumbnails.EnumerateArray().ToList();
                            // Prefer JPG thumbnails, but accept any (including WebP — handled by SkiaSharp)
                            var jpgThumbs = thumbList.Where(t => t.TryGetProperty("url", out var u) && (u.GetString()?.Contains(".jpg") == true)).ToList();
                            if (jpgThumbs.Any()) thumbnailUrl = jpgThumbs.Last().GetProperty("url").GetString() ?? "";
                            else if (thumbList.Any()) thumbnailUrl = thumbList.Last().GetProperty("url").GetString() ?? "";
                        }

                        if (string.IsNullOrEmpty(thumbnailUrl) && root.TryGetProperty("thumbnail", out var thumb))
                            thumbnailUrl = thumb.GetString() ?? "";

                        return new VideoMetadata
                        {
                            Title = root.TryGetProperty("title", out var title) ? title.GetString() ?? "" : "",
                            Channel = root.TryGetProperty("uploader", out var uploader) ? uploader.GetString() ?? "" :
                                     (root.TryGetProperty("channel", out var channel) ? channel.GetString() ?? "" : ""),
                            Duration = root.TryGetProperty("duration", out var duration) && duration.ValueKind == JsonValueKind.Number ? duration.GetInt32() : 0,
                            ThumbnailUrl = thumbnailUrl
                        };
                    }
                }
                catch { }
                return null;
            }, token).ConfigureAwait(false);
        }

        public async Task DownloadAsync(string url, string outputPath, string qualityArg, bool downloadSubs, bool useStandalone, string? standalonePath, string? ffmpegPath, bool ffmpegAvailable)
        {
            _isCancelled = false;
            _isPaused = false;
            _lastErrorLine = string.Empty;

            var strategy = _strategies.FirstOrDefault(s => s.CanHandle(url)) ?? new DefaultStrategy();
            string extraArgs = strategy.GetExtraArguments(url);

            bool runStandalone = useStandalone && !string.IsNullOrEmpty(standalonePath);
            var startInfo = new ProcessStartInfo
            {
                FileName = runStandalone ? standalonePath! : "python",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            if (!runStandalone)
            {
                startInfo.ArgumentList.Add("-m");
                startInfo.ArgumentList.Add("yt_dlp");
            }

            AddArgumentsFromString(startInfo, extraArgs);

            if (!string.IsNullOrWhiteSpace(ffmpegPath))
            {
                startInfo.ArgumentList.Add("--ffmpeg-location");
                startInfo.ArgumentList.Add(ffmpegPath);
            }

            startInfo.ArgumentList.Add("--continue");
            startInfo.ArgumentList.Add("--no-playlist");

            AddArgumentsFromString(startInfo, qualityArg);

            if (downloadSubs)
            {
                startInfo.ArgumentList.Add("--embed-subs");
                startInfo.ArgumentList.Add("--write-auto-sub");
            }

            if (ffmpegAvailable)
            {
                startInfo.ArgumentList.Add("--embed-thumbnail");
                startInfo.ArgumentList.Add("--merge-output-format");
                startInfo.ArgumentList.Add("mp4");
            }

            startInfo.ArgumentList.Add("-o");
            startInfo.ArgumentList.Add(Path.Combine(outputPath, "%(title)s.%(ext)s"));
            startInfo.ArgumentList.Add(url);

            Process? processToRun = null;
            lock (_processLock)
            {
                _currentProcess = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
                processToRun = _currentProcess;
            }

            processToRun.OutputDataReceived += (s, e) => HandleOutput(e.Data);
            processToRun.ErrorDataReceived += (s, e) => HandleOutput(e.Data);

            await Task.Run(() =>
            {
                try
                {
                    processToRun.Start();
                    processToRun.BeginOutputReadLine();
                    processToRun.BeginErrorReadLine();
                    processToRun.WaitForExit();

                    if (!_isCancelled)
                    {
                        bool success = processToRun.ExitCode == 0;
                        string message = success
                            ? "Success"
                            : (!string.IsNullOrWhiteSpace(_lastErrorLine) ? _lastErrorLine : "Process exited with error");
                        DownloadCompleted?.Invoke(success, message);
                    }
                }
                catch (Exception ex)
                {
                    if (!_isCancelled)
                        DownloadCompleted?.Invoke(false, ex.Message);
                }
                finally
                {
                    lock (_processLock)
                    {
                        try { _currentProcess?.Dispose(); } catch { }
                        _currentProcess = null;
                    }
                }
            }).ConfigureAwait(false);
        }

        private static void AddArgumentsFromString(ProcessStartInfo startInfo, string arguments)
        {
            if (string.IsNullOrWhiteSpace(arguments)) return;

            foreach (Match match in ArgumentTokenizer.Matches(arguments))
            {
                var token = match.Value;
                if (token.Length >= 2 && token.StartsWith("\"") && token.EndsWith("\""))
                {
                    token = token.Substring(1, token.Length - 2);
                }

                if (!string.IsNullOrWhiteSpace(token))
                {
                    startInfo.ArgumentList.Add(token);
                }
            }
        }

        private void HandleOutput(string? data)
        {
            if (string.IsNullOrEmpty(data) || _isCancelled) return;

            OutputReceived?.Invoke(data);

            if (data.Contains("ERROR:", StringComparison.OrdinalIgnoreCase))
            {
                _lastErrorLine = data.Trim();
            }

            // Progress parsing — use InvariantCulture to handle both '.' and ',' decimal separators
            if (data.Contains("[download]") && data.Contains('%'))
            {
                try
                {
                    var match = Regex.Match(data, @"(\d+\.?\d*)\s*%");
                    if (match.Success &&
                        double.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double progress))
                    {
                        ProgressChanged?.Invoke(progress, "Downloading");
                    }
                }
                catch { }
            }
        }

        public void Cancel()
        {
            _isCancelled = true;
            int pid;
            lock (_processLock)
            {
                pid = _currentProcess?.Id ?? 0;
            }
            NativeMethods.KillProcessTree(pid);
        }

        public void PauseResume()
        {
            int pid;
            lock (_processLock)
            {
                if (_currentProcess == null) return;
                pid = _currentProcess.Id;
            }

            if (_isPaused)
            {
                NativeMethods.ResumeProcess(pid);
                _isPaused = false;
            }
            else
            {
                NativeMethods.PauseProcess(pid);
                _isPaused = true;
            }
        }
    }
}
