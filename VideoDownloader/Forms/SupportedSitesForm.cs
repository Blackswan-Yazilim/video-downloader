using System;
using System.Drawing;
using System.Windows.Forms;
using VideoDownloader.Services;

namespace VideoDownloader.Forms
{
    /// <summary>
    /// Dialog listing all supported video platforms.
    /// Extracted from MainForm.SupportedSitesMenuItem_Click.
    /// </summary>
    public partial class SupportedSitesForm : Form
    {
        private readonly LocalizationService _loc;
        private readonly AppLanguage _language;
        private readonly AppTheme _theme;
        private readonly ThemeService _themeService;

        public SupportedSitesForm(LocalizationService loc, AppLanguage language, AppTheme theme, ThemeService themeService)
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
            this.Text = "Supported Sites";
            this.Size = new Size(520, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var sitesLabel = new Label
            {
                Location = new Point(20, 20),
                Size = new Size(460, 400),
                Font = new Font("Segoe UI", 10)
            };

            var okButton = new Button
            {
                Location = new Point(410, 440),
                Size = new Size(80, 35),
                DialogResult = DialogResult.OK,
                Font = new Font("Segoe UI", 9),
                BackColor = AppConstants.PrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            okButton.FlatAppearance.BorderSize = 0;
            okButton.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { sitesLabel, okButton });
            this.AcceptButton = okButton;

            this.Tag = new Control[] { sitesLabel, okButton };
        }

        private void ApplyTheme()
        {
            var colors = _themeService.GetColors(_theme);
            this.BackColor = colors.BackgroundColor;

            if (this.Tag is Control[] controls)
            {
                controls[0].ForeColor = colors.ForegroundColor; // sitesLabel
                controls[1].BackColor = AppConstants.PrimaryColor;
                controls[1].ForeColor = Color.White;
            }
        }

        private void ApplyLocalization()
        {
            this.Text = _loc.GetText("SupportedSites", _language);

            var sitesText = _language == AppLanguage.Turkish
                ? @"📺 ANA PLATFORMLAR:
YouTube, Instagram, TikTok, Twitter/X
Facebook, Twitch, Vimeo, Dailymotion
Reddit, LinkedIn

🔞 YETİŞKİN (+18):
Pornhub, XVideos, RedTube ve diğerleri

📺 TV & HABER:
BBC iPlayer, CNN, ESPN, Arte

🌏 ULUSLARARASI:
Bilibili, Niconico, VK

🎵 MÜZİK:
SoundCloud, Bandcamp, Mixcloud

📚 EĞİTİM:
Udemy, Coursera, Khan Academy

Ve 1000+ site daha..."
                : @"📺 MAIN PLATFORMS:
YouTube, Instagram, TikTok, Twitter/X
Facebook, Twitch, Vimeo, Dailymotion
Reddit, LinkedIn

🔞 ADULT (+18):
Pornhub, XVideos, RedTube and others

📺 TV & NEWS:
BBC iPlayer, CNN, ESPN, Arte

🌏 INTERNATIONAL:
Bilibili, Niconico, VK

🎵 MUSIC:
SoundCloud, Bandcamp, Mixcloud

📚 EDUCATION:
Udemy, Coursera, Khan Academy

And 1000+ more sites...";

            if (this.Tag is Control[] controls)
            {
                controls[0].Text = sitesText;  // sitesLabel
                controls[1].Text = _loc.GetText("OK", _language); // okButton
            }
        }
    }
}
