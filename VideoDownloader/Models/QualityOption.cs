using System.Drawing;

namespace VideoDownloader.Models
{
    /// <summary>
    /// Represents a video quality option in the UI dropdown.
    /// Replaces string-based quality handling in MainForm.
    /// </summary>
    public enum QualityOption
    {
        Best = 0,
        Q2160p = 1,
        Q1440p = 2,
        Q1080p = 3,
        Q720p = 4,
        Q480p = 5,
        Q360p = 6
    }

    /// <summary>
    /// Helper for QualityOption display and argument conversion.
    /// </summary>
    public static class QualityOptionExtensions
    {
        /// <summary>
        /// Returns the yt-dlp format argument for the given quality option.
        /// </summary>
        public static string ToFormatArgument(this QualityOption quality)
        {
            return quality switch
            {
                QualityOption.Best => "--format \"bestvideo+bestaudio/best\"",
                QualityOption.Q2160p => "--format \"bestvideo[height<=2160]+bestaudio/best[height<=2160]/best\"",
                QualityOption.Q1440p => "--format \"bestvideo[height<=1440]+bestaudio/best[height<=1440]/best\"",
                QualityOption.Q1080p => "--format \"bestvideo[height<=1080]+bestaudio/best[height<=1080]/best\"",
                QualityOption.Q720p => "--format \"bestvideo[height<=720]+bestaudio/best[height<=720]/best\"",
                QualityOption.Q480p => "--format \"bestvideo[height<=480]+bestaudio/best[height<=480]/best\"",
                QualityOption.Q360p => "--format \"bestvideo[height<=360]+bestaudio/best[height<=360]/best\"",
                _ => "--format \"bestvideo+bestaudio/best\""
            };
        }

        /// <summary>
        /// Returns the display label for the given quality option.
        /// </summary>
        public static string ToDisplayLabel(this QualityOption quality, bool isTurkish)
        {
            return quality switch
            {
                QualityOption.Best => isTurkish ? "En İyi Kalite" : "Best Quality",
                QualityOption.Q2160p => "2160p (4K)",
                QualityOption.Q1440p => "1440p (2K)",
                QualityOption.Q1080p => "1080p (Full HD)",
                QualityOption.Q720p => "720p (HD)",
                QualityOption.Q480p => "480p (SD)",
                QualityOption.Q360p => "360p",
                _ => isTurkish ? "En İyi" : "Best"
            };
        }

        /// <summary>
        /// Returns a short human-readable label.
        /// </summary>
        public static string ToShortLabel(this QualityOption quality, bool isTurkish)
        {
            return quality switch
            {
                QualityOption.Best => isTurkish ? "En İyi" : "Best",
                QualityOption.Q2160p => "2160p (4K)",
                QualityOption.Q1440p => "1440p (2K)",
                QualityOption.Q1080p => "1080p (Full HD)",
                QualityOption.Q720p => "720p (HD)",
                QualityOption.Q480p => "480p (SD)",
                QualityOption.Q360p => "360p",
                _ => isTurkish ? "En İyi" : "Best"
            };
        }
    }
}
