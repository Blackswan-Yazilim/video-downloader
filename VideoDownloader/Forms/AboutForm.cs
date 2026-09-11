using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using VideoDownloader.Services;

namespace VideoDownloader.Forms
{
    /// <summary>
    /// About dialog showing app version, features, and technologies.
    /// Extracted from MainForm.AboutMenuItem_Click.
    /// </summary>
    public partial class AboutForm : Form
    {
        private readonly LocalizationService _loc;
        private readonly AppLanguage _language;
        private readonly AppTheme _theme;
        private readonly ThemeService _themeService;

        public AboutForm(LocalizationService loc, AppLanguage language, AppTheme theme, ThemeService themeService)
        {
            _loc = loc;
            _language = language;
            _theme = theme;
            _themeService = themeService;

            InitializeComponent();
            ApplyTheme();
            ApplyLocalization();
        }

        private void InitializeComponent()
        {
            this.Text = "About";
            this.Size = new Size(450, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var textLabel = new Label
            {
                Location = new Point(20, 20),
                Size = new Size(400, 60),
                Font = new Font("Segoe UI", 11)
            };

            var developerLabel = new Label
            {
                Location = new Point(20, 90),
                Size = new Size(80, 20),
                Font = new Font("Segoe UI", 10)
            };

            var kayapaterLink = new LinkLabel
            {
                Text = "kayapater",
                Location = new Point(105, 90),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            kayapaterLink.LinkClicked += (s, args) =>
            {
                try { Process.Start(new ProcessStartInfo("https://github.com/kayapater") { UseShellExecute = true }); } catch { }
            };

            var featuresLabel = new Label
            {
                Location = new Point(20, 120),
                Size = new Size(400, 180),
                Font = new Font("Segoe UI", 9)
            };

            var okButton = new Button
            {
                Location = new Point(340, 320),
                Size = new Size(80, 35),
                DialogResult = DialogResult.OK,
                Font = new Font("Segoe UI", 9),
                BackColor = AppConstants.PrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            okButton.FlatAppearance.BorderSize = 0;
            okButton.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { textLabel, developerLabel, kayapaterLink, featuresLabel, okButton });
            this.AcceptButton = okButton;

            // Store references for localization
            this.Tag = new Control[] { textLabel, developerLabel, featuresLabel, okButton };
        }

        private void ApplyTheme()
        {
            var colors = _themeService.GetColors(_theme);
            this.BackColor = colors.BackgroundColor;

            if (this.Tag is Control[] controls)
            {
                foreach (var c in controls)
                {
                    if (c is Button btn)
                    {
                        btn.BackColor = AppConstants.PrimaryColor;
                        btn.ForeColor = Color.White;
                    }
                    else if (c is Label lbl)
                    {
                        lbl.ForeColor = colors.ForegroundColor;
                    }
                    else if (c is LinkLabel ll)
                    {
                        ll.BackColor = colors.BackgroundColor;
                        ll.LinkColor = AppConstants.PrimaryColor;
                    }
                }
            }
        }

        private void ApplyLocalization()
        {
            this.Text = _loc.GetText("AboutTitle", _language);

            if (this.Tag is Control[] controls)
            {
                controls[0].Text = _loc.GetText("AppDescription", _language); // textLabel
                controls[1].Text = _loc.GetText("Developer", _language);       // developerLabel
                controls[3].Text = _loc.GetText("OK", _language);              // okButton

                // featuresLabel
                var featuresText = _language == AppLanguage.Turkish
                    ? $@"Bu uygulama ile:
• YouTube, Twitter, Instagram videoları
• TikTok, Facebook, Vimeo ve daha fazlası
• Farklı kalite seçenekleri (4K, 1080p, 720p)
• MP3 olarak ses indirme
• Altyazı indirme desteği

Teknolojiler: .NET 8.0, yt-dlp {DependencyVersions.YtDlp}, FFmpeg {DependencyVersions.FFmpeg}"
                    : $@"With this app:
• YouTube, Twitter, Instagram videos
• TikTok, Facebook, Vimeo and more
• Different quality options (4K, 1080p, 720p)
• Download audio as MP3
• Subtitle download support

Technologies: .NET 8.0, yt-dlp {DependencyVersions.YtDlp}, FFmpeg {DependencyVersions.FFmpeg}";
                controls[2].Text = featuresText;
            }
        }
    }
}
