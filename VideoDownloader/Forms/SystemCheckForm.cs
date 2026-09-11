using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using VideoDownloader.Services;

namespace VideoDownloader.Forms
{
    /// <summary>
    /// System dependency check dialog showing Python, yt-dlp, and FFmpeg status.
    /// Allows one-click installation of missing dependencies.
    /// Extracted from MainForm.SystemCheckMenuItem_Click.
    /// </summary>
    public partial class SystemCheckForm : Form
    {
        private readonly LocalizationService _loc;
        private readonly AppLanguage _language;
        private readonly AppTheme _theme;
        private readonly ThemeService _themeService;
        private readonly DependencyManager _dependencyManager;

        private Label _titleLabel = null!;
        private Label _pythonLabel = null!;
        private Label _ytdlpLabel = null!;
        private Label _ffmpegLabel = null!;
        private Label _statusLabel = null!;
        private Button _installButton = null!;
        private Button _closeButton = null!;

        public SystemCheckForm(LocalizationService loc, AppLanguage language, AppTheme theme,
                               ThemeService themeService, DependencyManager dependencyManager)
        {
            _loc = loc;
            _language = language;
            _theme = theme;
            _themeService = themeService;
            _dependencyManager = dependencyManager;

            InitializeComponent();
            ApplyTheme();
            ApplyLocalization();
        }

        private void InitializeComponent()
        {
            this.Text = "System Check";
            this.Size = new Size(450, 370);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            _titleLabel = new Label
            {
                Location = new Point(20, 20),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 14, FontStyle.Bold)
            };

            _pythonLabel = new Label
            {
                Location = new Point(20, 70),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 10)
            };

            _ytdlpLabel = new Label
            {
                Location = new Point(20, 100),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 10)
            };

            _ffmpegLabel = new Label
            {
                Location = new Point(20, 130),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 10)
            };

            _statusLabel = new Label
            {
                Location = new Point(20, 180),
                Size = new Size(400, 50),
                Font = new Font("Segoe UI", 9)
            };

            _installButton = new Button
            {
                Location = new Point(20, 260),
                Size = new Size(120, 35),
                Font = new Font("Segoe UI", 9),
                BackColor = AppConstants.PrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = false
            };
            _installButton.FlatAppearance.BorderSize = 0;

            _closeButton = new Button
            {
                Location = new Point(340, 260),
                Size = new Size(80, 35),
                DialogResult = DialogResult.OK,
                Font = new Font("Segoe UI", 9),
                BackColor = AppConstants.PrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _closeButton.FlatAppearance.BorderSize = 0;
            _closeButton.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { _titleLabel, _pythonLabel, _ytdlpLabel, _ffmpegLabel, _statusLabel, _installButton, _closeButton });
            this.AcceptButton = _closeButton;

            this.Shown += async (s, args) => await RunChecksAsync();
            _installButton.Click += async (s, args) => await InstallYtDlpClickedAsync();
        }

        private void ApplyTheme()
        {
            var colors = _themeService.GetColors(_theme);
            this.BackColor = colors.BackgroundColor;

            _titleLabel.ForeColor = colors.ForegroundColor;
            _pythonLabel.ForeColor = colors.ForegroundColor;
            _ytdlpLabel.ForeColor = colors.ForegroundColor;
            _ffmpegLabel.ForeColor = colors.ForegroundColor;
            _statusLabel.ForeColor = colors.ForegroundColor;
            _installButton.BackColor = AppConstants.PrimaryColor;
            _installButton.ForeColor = Color.White;
            _closeButton.BackColor = AppConstants.PrimaryColor;
            _closeButton.ForeColor = Color.White;
        }

        private void ApplyLocalization()
        {
            _titleLabel.Text = _loc.GetText("SystemStatus", _language);
            _pythonLabel.Text = _loc.GetText("CheckingPython", _language);
            _ytdlpLabel.Text = _loc.GetText("CheckingYtDlp", _language);
            _ffmpegLabel.Text = _loc.GetText("CheckingFFmpeg", _language);
            _installButton.Text = _loc.GetText("InstallYtDlpBtn", _language);
            _closeButton.Text = _loc.GetText("OK", _language);
        }

        private async Task RunChecksAsync()
        {
            var successColor = Color.FromArgb(34, 197, 94);
            var errorColor = Color.FromArgb(239, 68, 68);
            var warningColor = Color.FromArgb(234, 179, 8);

            // yt-dlp check
            var ytdlpOk = await _dependencyManager.CheckYtDlpInstalledAsync();
            var pythonOk = ytdlpOk || await _dependencyManager.CheckPythonInstalledAsync();

            if (ytdlpOk)
            {
                _pythonLabel.Text = _loc.GetText("PythonNotRequired", _language);
                _pythonLabel.ForeColor = successColor;
            }
            else
            {
                _pythonLabel.Text = pythonOk
                    ? _loc.GetText("PythonInstalled", _language)
                    : _loc.GetText("PythonNotFoundCheck", _language);
                _pythonLabel.ForeColor = pythonOk ? successColor : errorColor;
            }

            _ytdlpLabel.Text = ytdlpOk
                ? _loc.GetText("YtDlpInstalledCheck", _language)
                : _loc.GetText("YtDlpNotFoundCheck", _language);
            _ytdlpLabel.ForeColor = ytdlpOk ? successColor : errorColor;

            if (!pythonOk)
            {
                _statusLabel.Text = _loc.GetText("PythonRequired", _language);
                _statusLabel.ForeColor = errorColor;
                return;
            }

            if (!ytdlpOk)
            {
                _installButton.Visible = true;
                _statusLabel.Text = _loc.GetText("YtDlpNotInstalledMsg", _language);
                _statusLabel.ForeColor = warningColor;
            }

            // FFmpeg check — MANDATORY, not optional
            var ffmpegOk = await _dependencyManager.CheckFFmpegInstalledAsync();
            _ffmpegLabel.Text = ffmpegOk
                ? _loc.GetText("FFmpegInstalledCheck", _language)
                : _loc.GetText("FFmpegNotFoundCheck", _language);
            _ffmpegLabel.ForeColor = ffmpegOk ? successColor : errorColor;

            if (pythonOk && ytdlpOk && ffmpegOk)
            {
                _statusLabel.Text = _loc.GetText("AllReady", _language);
                _statusLabel.ForeColor = successColor;
            }
            else if (!ffmpegOk)
            {
                _statusLabel.Text = _loc.GetText("FFmpegRequired", _language);
                _statusLabel.ForeColor = errorColor;
            }
        }

        private async Task InstallYtDlpClickedAsync()
        {
            _installButton.Enabled = false;
            _installButton.Text = _loc.GetText("Installing", _language);
            _statusLabel.Text = _loc.GetText("InstallingYtDlpWait", _language);
            _statusLabel.ForeColor = _themeService.GetColors(_theme).ForegroundColor;

            var installed = await _dependencyManager.InstallYtDlpAsync();

            if (installed)
            {
                _ytdlpLabel.Text = _loc.GetText("YtDlpInstalledCheck", _language);
                _ytdlpLabel.ForeColor = Color.FromArgb(34, 197, 94);
                _statusLabel.Text = _loc.GetText("YtDlpSuccess", _language);
                _statusLabel.ForeColor = Color.FromArgb(34, 197, 94);
                _installButton.Visible = false;
            }
            else
            {
                _statusLabel.Text = _loc.GetText("YtDlpFailed", _language);
                _statusLabel.ForeColor = Color.FromArgb(239, 68, 68);
                _installButton.Text = _loc.GetText("Retry", _language);
                _installButton.Enabled = true;
            }
        }
    }
}
