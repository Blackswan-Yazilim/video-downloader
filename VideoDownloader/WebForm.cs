using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace VideoDownloader
{
    public partial class WebForm : Form
    {
        private WebView2 _webView;
        private JsBridge _bridge;

        public WebForm()
        {
            Text = "Video Downloader v2.0.1";
            ClientSize = new System.Drawing.Size(1280, 760);
            MinimumSize = new System.Drawing.Size(1000, 660);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            StartPosition = FormStartPosition.CenterScreen;
            try
            {
                var logoPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "logo.png");
                if (File.Exists(logoPath))
                {
                    using var bmp = new Bitmap(logoPath);
                    Icon = Icon.FromHandle(bmp.GetHicon());
                }
                else Icon = SystemIcons.Application;
            }
            catch { Icon = SystemIcons.Application; }

            _bridge = new JsBridge();
            _webView = new WebView2 { Dock = DockStyle.Fill };
            Controls.Add(_webView);

            Load += async (_, _) => await InitAsync();
        }

        private async Task InitAsync()
        {
            try
            {
                var userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "VideoDownloader",
                    "WebView2"
                );
                Directory.CreateDirectory(userDataFolder);
                var env = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(null, userDataFolder);
                await _webView.EnsureCoreWebView2Async(env);
            }
            catch
            {
                await _webView.EnsureCoreWebView2Async(null);
            }

            if (_webView.CoreWebView2 != null)
            {
                _webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
                _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            }

            _bridge.SetWebView(_webView);

            _webView.CoreWebView2.DOMContentLoaded += async (s, e) =>
            {
                var lang = _bridge.GetLanguage();
                await _webView.CoreWebView2.ExecuteScriptAsync(
                    $"if(!localStorage.getItem('vdlang')){{localStorage.setItem('vdlang','{lang}');if(window.vdApplyLanguage)window.vdApplyLanguage();}}");
            };

            // Handle navigation and external browser links
            _webView.CoreWebView2.NavigationStarting += (s, e) =>
            {
                if (e.Uri.StartsWith("http://bridge.local/"))
                {
                    e.Cancel = true;
                    try
                    {
                        var path = e.Uri.Replace("http://bridge.local/", "");
                        var slashIdx = path.IndexOf('/');
                        var cmd = slashIdx > 0 ? path.Substring(0, slashIdx) : path;
                        var data = slashIdx > 0 ? Uri.UnescapeDataString(path.Substring(slashIdx + 1)) : null;
                        _bridge.HandleCustomCommand(cmd, data);
                    }
                    catch { }
                }
                else if (e.Uri.StartsWith("http://") || e.Uri.StartsWith("https://"))
                {
                    e.Cancel = true;
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri) { UseShellExecute = true });
                    }
                    catch { }
                }
            };

            _webView.CoreWebView2.NewWindowRequested += (s, e) =>
            {
                e.Handled = true;
                if (!string.IsNullOrEmpty(e.Uri) && (e.Uri.StartsWith("http://") || e.Uri.StartsWith("https://")))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri) { UseShellExecute = true });
                    }
                    catch { }
                }
            };

            // Handle messages from JS
            _webView.CoreWebView2.WebMessageReceived += (s, e) =>
            {
                try { _bridge.HandleMessage(e.WebMessageAsJson); } catch { }
            };

            // Navigate to main page
            string wwwRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
            string htmlPath = Path.Combine(wwwRoot, "download.html");
            if (File.Exists(htmlPath))
                _webView.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);
            else
                MessageBox.Show("wwwroot\\download.html not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
