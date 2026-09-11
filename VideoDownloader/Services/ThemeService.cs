using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace VideoDownloader.Services
{
    /// <summary>
    /// Centralized theme engine supporting 8 visual themes.
    /// Extracted from MainForm.cs ApplyTheme() and related code.
    /// </summary>
    public class ThemeService
    {
        private readonly Dictionary<AppTheme, ThemeColors> _themes;

        public ThemeService()
        {
            _themes = new Dictionary<AppTheme, ThemeColors>
            {
                [AppTheme.Dark] = new ThemeColors(
                    Color.FromArgb(24, 24, 27),
                    Color.FromArgb(250, 250, 250),
                    Color.FromArgb(39, 39, 42),
                    Color.FromArgb(244, 244, 245),
                    Color.FromArgb(39, 39, 42)
                ),
                [AppTheme.Light] = new ThemeColors(
                    Color.FromArgb(249, 250, 251),
                    Color.FromArgb(17, 24, 39),
                    Color.FromArgb(255, 255, 255),
                    Color.FromArgb(17, 24, 39),
                    Color.FromArgb(243, 244, 246)
                ),
                [AppTheme.Ocean] = new ThemeColors(
                    Color.FromArgb(15, 23, 42),
                    Color.FromArgb(224, 242, 254),
                    Color.FromArgb(30, 58, 95),
                    Color.FromArgb(186, 230, 253),
                    Color.FromArgb(23, 37, 63)
                ),
                [AppTheme.Forest] = new ThemeColors(
                    Color.FromArgb(5, 46, 22),
                    Color.FromArgb(220, 252, 231),
                    Color.FromArgb(20, 83, 45),
                    Color.FromArgb(187, 247, 208),
                    Color.FromArgb(6, 50, 24)
                ),
                [AppTheme.Sunset] = new ThemeColors(
                    Color.FromArgb(49, 20, 8),
                    Color.FromArgb(255, 237, 213),
                    Color.FromArgb(82, 34, 14),
                    Color.FromArgb(254, 215, 170),
                    Color.FromArgb(52, 22, 9)
                ),
                [AppTheme.Purple] = new ThemeColors(
                    Color.FromArgb(30, 15, 50),
                    Color.FromArgb(237, 233, 254),
                    Color.FromArgb(55, 30, 85),
                    Color.FromArgb(221, 214, 254),
                    Color.FromArgb(33, 17, 55)
                ),
                [AppTheme.Rose] = new ThemeColors(
                    Color.FromArgb(50, 10, 30),
                    Color.FromArgb(255, 228, 230),
                    Color.FromArgb(85, 18, 50),
                    Color.FromArgb(254, 205, 211),
                    Color.FromArgb(55, 12, 33)
                ),
                [AppTheme.Midnight] = new ThemeColors(
                    Color.FromArgb(10, 15, 40),
                    Color.FromArgb(219, 234, 254),
                    Color.FromArgb(20, 30, 70),
                    Color.FromArgb(191, 219, 254),
                    Color.FromArgb(12, 18, 45)
                )
            };
        }

        /// <summary>
        /// Returns the color palette for the given theme.
        /// </summary>
        public ThemeColors GetColors(AppTheme theme)
        {
            return _themes.TryGetValue(theme, out var colors)
                ? colors
                : _themes[AppTheme.Dark]; // Fallback
        }

        /// <summary>
        /// Recursively applies theme colors to a form and all its child controls.
        /// </summary>
        public void ApplyTheme(Form form, AppTheme theme)
        {
            var colors = GetColors(theme);
            form.BackColor = colors.BackgroundColor;
            ApplyToControl(form, colors);
        }

        private void ApplyToControl(Control control, ThemeColors colors)
        {
            foreach (Control child in control.Controls)
            {
                if (child is Label label)
                {
                    label.ForeColor = colors.ForegroundColor;
                }
                else if (child is TextBox textBox)
                {
                    textBox.BackColor = colors.InputBackColor;
                    textBox.ForeColor = colors.InputForeColor;
                }
                else if (child is ComboBox comboBox)
                {
                    comboBox.BackColor = colors.InputBackColor;
                    comboBox.ForeColor = colors.InputForeColor;
                }
                else if (child is Panel panel)
                {
                    panel.BackColor = colors.PanelColor;
                    ApplyToControl(panel, colors); // Recurse into panels
                }
                else if (child is CheckBox checkBox)
                {
                    checkBox.ForeColor = colors.ForegroundColor;
                }
                else if (child is ProgressBar)
                {
                    // ProgressBar colors are system-controlled; skip
                }
                else if (child is MenuStrip menuStrip)
                {
                    menuStrip.BackColor = colors.BackgroundColor;
                    menuStrip.ForeColor = colors.ForegroundColor;
                    foreach (ToolStripItem item in menuStrip.Items)
                    {
                        item.BackColor = colors.BackgroundColor;
                        item.ForeColor = colors.ForegroundColor;
                    }
                }

                // Recurse into containers that aren't Panels (already handled)
                if (child.HasChildren && child is not Panel)
                {
                    ApplyToControl(child, colors);
                }
            }
        }
    }

    /// <summary>
    /// Immutable color palette for a single theme.
    /// </summary>
    public record ThemeColors(
        Color BackgroundColor,
        Color ForegroundColor,
        Color InputBackColor,
        Color InputForeColor,
        Color PanelColor
    );
}
