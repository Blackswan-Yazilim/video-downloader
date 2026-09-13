using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.WinForms;
using VideoDownloader.Models;
using VideoDownloader.Services;

namespace VideoDownloader
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class JsBridge
    {
        private readonly YtDlpService _yt;
        private readonly DependencyManager _dep;
        private readonly SettingsManager _cfg;
        private readonly LocalizationService _loc;
        private readonly List<HistoryItem> _history = new();
        private readonly System.Threading.SemaphoreSlim _downloadLock = new(1, 1);
        private WebView2? _webView;
        private System.Windows.Forms.Form? _form;

        public JsBridge()
        {
            _yt = new YtDlpService();
            _dep = new DependencyManager(HttpClientFactory.Client);
            _cfg = new SettingsManager();
            _loc = new LocalizationService();

            _yt.ProgressChanged += (p, s) =>
            {
                if (_form == null || _form.IsDisposed || _webView == null) return;
                var status = (s ?? "").Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", " ").Replace("\r", "");
                var pct = (int)Math.Round(p);
                _form.BeginInvoke((Action)(() =>
                {
                    if (_webView.CoreWebView2 == null) return;
                    _webView.CoreWebView2.ExecuteScriptAsync(
                        $"if(window._onProgress)window._onProgress({pct},'{status}')");
                }));
            };
            _yt.DownloadCompleted += (ok, msg) =>
            {
                if (ok) UpdateHistoryStatus("completed");
                else UpdateHistoryStatus("failed");

                if (_form == null || _form.IsDisposed || _webView == null) return;
                var safeMsg = (msg ?? "").Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", " ").Replace("\r", "");
                _form.BeginInvoke((Action)(() =>
                {
                    if (_webView.CoreWebView2 == null) return;
                    _webView.CoreWebView2.ExecuteScriptAsync(
                        ok
                            ? "if(window._onDownloadDone)window._onDownloadDone(true)"
                            : $"if(window._onDownloadDone)window._onDownloadDone(false,'{safeMsg}')");
                }));
            };

            LoadHistory();
        }

        public void SetWebView(WebView2 wv) { _webView = wv; _form = wv.FindForm(); }

        /// <summary>Handles custom bridge:// commands via navigation interception.</summary>
        public void HandleCustomCommand(string cmd, string? query)
        {
            if (cmd == "pick-folder" && _form != null && !_form.IsDisposed)
            {
                _form.BeginInvoke((Action)(() =>
                {
                    using var dlg = new System.Windows.Forms.FolderBrowserDialog();
                    dlg.Description = "Select download folder";
                    dlg.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "Video Downloader");
                    if (dlg.ShowDialog(_form) == System.Windows.Forms.DialogResult.OK && !string.IsNullOrEmpty(dlg.SelectedPath))
                    {
                        var path = dlg.SelectedPath.Replace("\\", "\\\\").Replace("'", "\\'");
                        _webView?.CoreWebView2?.ExecuteScriptAsync(
                            $"document.getElementById('defaultPathInput').value='{path}'");
                    }
                }));
            }
            else if (cmd == "fetch-metadata" && !string.IsNullOrEmpty(query))
            {
                try
                {
                    var args = System.Text.Json.JsonSerializer.Deserialize<string[]>(query);
                    if (args != null && args.Length > 0 && _form != null && !_form.IsDisposed)
                        _form.BeginInvoke((Action)(() => _ = FetchAndEmitMetadata(args[0])));
                }
                catch { }
            }
            else if (cmd == "get-settings")
            {
                Emit("settings-data", new { language = _cfg.LoadLanguage().ToString(), theme = _cfg.LoadTheme().ToString() });
            }
            else if (cmd == "get-history")
            {
                Emit("history-data", _history);
            }
            else if (cmd == "save-setting" && !string.IsNullOrEmpty(query))
            {
                try
                {
                    var args = System.Text.Json.JsonSerializer.Deserialize<string[]>(query);
                    if (args != null && args.Length >= 2)
                        SaveSetting(args[0], args[1]);
                }
                catch { }
            }
            else if (cmd == "clear-history")
            {
                _history.Clear();
                SaveHistory();
                Emit("history-updated", _history);
            }
            else if (cmd == "start-download" && !string.IsNullOrEmpty(query))
            {
                try
                {
                    var args = System.Text.Json.JsonSerializer.Deserialize<object[]>(query);
                    if (args != null && args.Length >= 4)
                    {
                        var url = args[0]?.ToString()!;
                        var path = args[1]?.ToString()!;
                        var quality = args[2]?.ToString() ?? "best";
                        var subs = args[3] is System.Text.Json.JsonElement je && je.ValueKind == System.Text.Json.JsonValueKind.True;
                        if (_form != null && !_form.IsDisposed)
                        {
                            _form.BeginInvoke((Action)(() =>
                            {
                                // Show download started in UI
                                _webView?.CoreWebView2?.ExecuteScriptAsync(
                                    "var al=document.getElementById('activeList');if(al&&!al.children.length)al.innerHTML=''");
                                _ = StartDownloadAsync(url, path, quality, subs);
                            }));
                        }
                    }
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine("start-download parse error: " + ex.Message); }
            }
        }

        /// <summary>Handles JSON messages from JavaScript window.chrome.webview.postMessage().</summary>
        public void HandleMessage(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var cmd = doc.RootElement.GetProperty("cmd").GetString();
                var args = doc.RootElement.TryGetProperty("args", out var a) ? a : default;

                switch (cmd)
                {
                    case "start-download":
                        string url = args[0].GetString()!;
                        string path = args[1].GetString()!;
                        string quality = args.GetArrayLength() > 2 ? args[2].GetString()! : "best";
                        bool subs = args.GetArrayLength() > 3 && args[3].GetBoolean();
                        if (_form != null && !_form.IsDisposed)
                            _form.BeginInvoke((Action)(() => _ = StartDownloadAsync(url, path, quality, subs)));
                        break;
                    case "pause":
                        PauseDownload();
                        break;
                    case "cancel":
                        CancelDownload();
                        break;
                    case "save-setting":
                        string key = args[0].GetString()!;
                        string val = args[1].GetString()!;
                        SaveSetting(key, val);
                        break;
                    case "clear-history":
                        _history.Clear();
                        SaveHistory();
                        Emit("history-updated", _history);
                        break;
                    case "get-history":
                        Emit("history-data", _history);
                        break;
                    case "get-settings":
                        Emit("settings-data", new { language = _cfg.LoadLanguage().ToString(), theme = _cfg.LoadTheme().ToString() });
                        break;
                    case "fetch-metadata":
                        if (args.GetArrayLength() > 0)
                        {
                            var metaUrl = args[0].GetString()!;
                            if (_form != null && !_form.IsDisposed)
                                _form.BeginInvoke((Action)(() => _ = FetchAndEmitMetadata(metaUrl)));
                        }
                        break;
                }
            }
            catch { }
        }

        private void Emit(string type, object? data = null)
        {
            if (_form == null || _form.IsDisposed || _webView == null) return;
            var json = JsonSerializer.Serialize(new { type, data });
            _form.BeginInvoke((Action)(() =>
            {
                if (_webView.CoreWebView2 == null) return;
                _webView.CoreWebView2.PostWebMessageAsJson(json);
            }));
        }

        // ── Called from JS: bridge.methodName(args) ───────────────────

        public async Task<string> FetchMetadata(string url)
        {
            try
            {
                var meta = await _yt.GetVideoMetadataAsync(url, _dep.UseStandaloneYtDlp, _dep.StandaloneYtDlpPath, default);
                if (meta == null) return "{}";
                return JsonSerializer.Serialize(new { title = meta.Title, channel = meta.Channel, duration = meta.Duration, thumbnail = meta.ThumbnailUrl });
            }
            catch { return "{}"; }
        }

        private async Task StartDownloadAsync(string url, string path, string quality, bool subtitles)
        {
            if (!await _downloadLock.WaitAsync(0))
            {
                if (_form != null && !_form.IsDisposed)
                {
                    _form.BeginInvoke((Action)(() =>
                    {
                        _webView?.CoreWebView2?.ExecuteScriptAsync("if(window._onDownloadBusy)window._onDownloadBusy();");
                    }));
                }
                return;
            }
            try
            {
                // Resolve to absolute path — use MyVideos as base for relative paths
                string resolvedPath;
                if (Path.IsPathRooted(path))
                    resolvedPath = path;
                else
                    resolvedPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "Video Downloader");
                try { Directory.CreateDirectory(resolvedPath); } catch { }

                if (!await _dep.CheckYtDlpInstalledAsync())
                    await _dep.InstallYtDlpAsync();

                bool ffOk = await _dep.CheckFFmpegInstalledAsync();
                string ffPath = "";
                if (!ffOk) ffOk = await _dep.InstallFFmpegAsync();
                _dep.TryGetLocalFFmpegDirectory(out var lf);
                if (lf != null) ffPath = lf;
                bool ffAvailable = ffOk || !string.IsNullOrEmpty(ffPath);

                var item = new HistoryItem { Url = url, Title = url, StartedAt = DateTime.Now, Status = "downloading" };
                _history.Insert(0, item);
                SaveHistory();
                Emit("history-updated", _history);

                // Single clean call — exactly like v1 WinForms that works perfectly
                var downloadSucceeded = await _yt.DownloadAsync(url, resolvedPath, quality, subtitles,
                    _dep.UseStandaloneYtDlp, _dep.StandaloneYtDlpPath, ffPath, ffAvailable);

                // After download completes, try to find the output file
                if (downloadSucceeded)
                    NotifyDownloadResult(resolvedPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Download error: " + ex.Message);
                if (_form != null && !_form.IsDisposed)
                    _form.BeginInvoke((Action)(() =>
                        _webView?.CoreWebView2?.ExecuteScriptAsync(
                            "var al=document.getElementById('activeList');if(al&&al.children.length>0){var it=al.children[0];var st=it.querySelector('.font-label-sm');if(st){st.textContent='Error: " + ex.Message.Replace("'", "\\'").Replace("\n", " ") + "';st.classList.add('text-error')}}")));
                UpdateHistoryStatus("failed");
            }
            finally
            {
                _downloadLock.Release();
            }
        }

        /// <summary>Checks if the download actually produced a file and notifies the UI.</summary>
        private void NotifyDownloadResult(string outputDir)
        {
            if (_form == null || _form.IsDisposed || _webView == null) return;
            try
            {
                var dir = new DirectoryInfo(outputDir);
                if (dir.Exists)
                {
                    var newest = dir.GetFiles().OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
                    if (newest != null && (DateTime.Now - newest.LastWriteTime).TotalMinutes < 5)
                    {
                        var fileName = newest.Name.Replace("\\", "\\\\").Replace("'", "\\'");
                        var dirPath = outputDir.Replace("\\", "\\\\").Replace("'", "\\'");
                        _form.BeginInvoke((Action)(() =>
                            _webView.CoreWebView2?.ExecuteScriptAsync(
                                $"alert('Downloaded: {fileName}\\nLocation: {dirPath}')")));
                        return;
                    }
                }
                // No file found
                _form.BeginInvoke((Action)(() =>
                    _webView.CoreWebView2?.ExecuteScriptAsync(
                        $"alert('Download may have completed but no file found in:\\n{outputDir.Replace("\\", "\\\\").Replace("'", "\\'")}')")));
            }
            catch { }
        }

        /// <summary>Quick check if a recent download file exists in the output directory.</summary>
        private bool CheckDownloadResult(string outputDir)
        {
            try
            {
                var dir = new DirectoryInfo(outputDir);
                if (dir.Exists)
                {
                    var newest = dir.GetFiles().OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
                    return newest != null && (DateTime.Now - newest.LastWriteTime).TotalMinutes < 5;
                }
            }
            catch { }
            return false;
        }

        /// <summary>Auto-detect the best browser for cookie extraction.</summary>
        private static string? DetectBestCookieBrowser()
        {
            // Priority order: Chrome > Edge > Firefox > Brave > Opera
            string[] browsers = { "chrome", "edge", "firefox", "brave", "opera" };
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            foreach (var browser in browsers)
            {
                string path = browser switch
                {
                    "chrome" => Path.Combine(localAppData, @"Google\Chrome\User Data"),
                    "edge" => Path.Combine(localAppData, @"Microsoft\Edge\User Data"),
                    "firefox" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Mozilla\Firefox\Profiles"),
                    "brave" => Path.Combine(localAppData, @"BraveSoftware\Brave-Browser\User Data"),
                    "opera" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Opera Software\Opera Stable"),
                    _ => null
                };
                if (path != null && Directory.Exists(path))
                    return browser;
            }
            return null; // No supported browser found
        }

        public void PauseDownload() => _yt.PauseResume();
        public void CancelDownload() => _yt.Cancel();

        public string PickFolder()
        {
            try
            {
                using var dlg = new System.Windows.Forms.FolderBrowserDialog();
                dlg.Description = "Select download folder";
                dlg.UseDescriptionForTitle = true;
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    return dlg.SelectedPath;
                return "";
            }
            catch (Exception ex) { return "ERROR:" + ex.Message; }
        }

        public string GetHistory() => JsonSerializer.Serialize(_history);
        public string GetSettings() => JsonSerializer.Serialize(new { language = _cfg.LoadLanguage().ToString(), theme = _cfg.LoadTheme().ToString() });
        public string GetLanguage() => _cfg.LoadLanguage().ToString();

        private async Task FetchAndEmitMetadata(string url)
        {
            try
            {
                if (!await _dep.CheckYtDlpInstalledAsync())
                    await _dep.InstallYtDlpAsync();
                var meta = await _yt.GetVideoMetadataAsync(url, _dep.UseStandaloneYtDlp, _dep.StandaloneYtDlpPath, default);
                if (meta != null && _form != null && !_form.IsDisposed && _webView != null)
                {
                    var title = (meta.Title ?? "").Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", " ").Replace("\r", "");
                    var channel = (meta.Channel ?? "").Replace("\\", "\\\\").Replace("'", "\\'").Replace("\n", " ").Replace("\r", "");
                    var thumb = (meta.ThumbnailUrl ?? "").Replace("\\", "\\\\").Replace("'", "\\'");
                    var dur = meta.Duration;
                    _form.BeginInvoke((Action)(() =>
                    {
                        if (_webView.CoreWebView2 == null) return;
                        _webView.CoreWebView2.ExecuteScriptAsync(
                            $"var pt=document.getElementById('previewTitle');if(pt)pt.textContent='{title}';" +
                            $"var pi=document.getElementById('previewInfo');if(pi)pi.textContent='{channel}';" +
                            (dur > 0 ? $"var db=document.getElementById('durationBadge');if(db){{db.textContent='{FormatDuration(dur)}';db.classList.remove('hidden')}}" : "") +
                            (thumb.Length > 0 ? $"var ti=document.getElementById('thumbnailImg');if(ti){{ti.src='{thumb}';ti.classList.remove('hidden')}}" : ""));
                    }));
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Metadata error: " + ex.Message); }
        }
        private static string FormatDuration(long seconds)
        {
            var m = seconds / 60;
            var s = seconds % 60;
            if (m >= 60)
                return string.Format("{0}:{1:D2}:{2:D2}", m / 60, m % 60, s);
            return string.Format("{0}:{1:D2}", m, s);
        }

        public void SaveSetting(string key, string value)
        {
            try
            {
                if (key == "language")
                {
                    _cfg.SaveSettings(Enum.Parse<Services.AppLanguage>(value), _cfg.LoadTheme());
                    _form?.BeginInvoke((Action)(() =>
                    {
                        _webView?.CoreWebView2?.ExecuteScriptAsync(
                            $"localStorage.setItem('vdlang','{value}');if(window.vdApplyLanguage)window.vdApplyLanguage();");
                    }));
                }
                else if (key == "theme")
                {
                    _cfg.SaveSettings(_cfg.LoadLanguage(), Enum.Parse<Services.AppTheme>(value));
                    _form?.BeginInvoke((Action)(() =>
                    {
                        _webView?.CoreWebView2?.ExecuteScriptAsync(
                            $"localStorage.setItem('vdtheme','{value}');document.documentElement.setAttribute('data-theme','{value}');");
                    }));
                }
            }
            catch { }
        }

        private void UpdateHistoryStatus(string status)
        {
            var item = _history.FirstOrDefault(h => h.Status == "downloading");
            if (item != null) { item.Status = status; item.CompletedAt = DateTime.Now; SaveHistory(); Emit("history-updated", _history); }
        }

        private void LoadHistory()
        {
            try { var path = GetHistoryPath(); if (File.Exists(path)) _history.AddRange(JsonSerializer.Deserialize<List<HistoryItem>>(File.ReadAllText(path)) ?? new()); } catch { }
        }
        private void SaveHistory()
        {
            try { Directory.CreateDirectory(Path.GetDirectoryName(GetHistoryPath())!); File.WriteAllText(GetHistoryPath(), JsonSerializer.Serialize(_history)); } catch { }
        }
        private static string GetHistoryPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VideoDownloader", "history.json");
    }

    public class HistoryItem
    {
        public string Url { get; set; } = "";
        public string Title { get; set; } = "";
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Status { get; set; } = "pending";
    }
}

