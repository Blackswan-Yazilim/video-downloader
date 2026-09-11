using System;
using Microsoft.Win32;

namespace VideoDownloader.Services
{
    /// <summary>
    /// Manages application settings persisted to the Windows Registry.
    /// Extracted from MainForm.cs to respect Single Responsibility Principle.
    /// </summary>
    public class SettingsManager
    {
        private const string RegistryKeyPath = @"SOFTWARE\VideoDownloader";
        private const string LanguageValue = "Language";
        private const string ThemeValue = "Theme";

        /// <summary>
        /// Loads the saved language preference, falling back to the OS culture.
        /// </summary>
        public AppLanguage LoadLanguage()
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath))
                {
                    var langValue = key?.GetValue(LanguageValue)?.ToString();
                    if (Enum.TryParse<AppLanguage>(langValue, out var language))
                    {
                        return language;
                    }
                }
            }
            catch { }

            // First-run: detect from system culture
            var systemCulture = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            return systemCulture == "tr" ? AppLanguage.Turkish : AppLanguage.English;
        }

        /// <summary>
        /// Loads the saved theme preference, defaulting to Dark.
        /// </summary>
        public AppTheme LoadTheme()
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath))
                {
                    var themeValue = key?.GetValue(ThemeValue)?.ToString();
                    if (Enum.TryParse<AppTheme>(themeValue, out var theme))
                    {
                        return theme;
                    }
                }
            }
            catch { }

            return AppTheme.Dark;
        }

        /// <summary>
        /// Persists current language and theme to the registry.
        /// </summary>
        public void SaveSettings(AppLanguage language, AppTheme theme)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath))
                {
                    key?.SetValue(LanguageValue, language.ToString());
                    key?.SetValue(ThemeValue, theme.ToString());
                }
            }
            catch { }
        }
    }

    // ── Shared enums (moved out of MainForm) ─────────────────────

    public enum AppLanguage { Turkish, English }

    public enum AppTheme { Light, Dark, Ocean, Forest, Sunset, Purple, Rose, Midnight }
}
