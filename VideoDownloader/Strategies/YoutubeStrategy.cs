using System;

namespace VideoDownloader.Strategies
{
    public class YoutubeStrategy : IPlatformStrategy
    {
        public bool CanHandle(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;

            return uri.Host.Equals("youtube.com", StringComparison.OrdinalIgnoreCase) ||
                   uri.Host.EndsWith(".youtube.com", StringComparison.OrdinalIgnoreCase) ||
                   uri.Host.Equals("youtu.be", StringComparison.OrdinalIgnoreCase);
        }

        public string GetExtraArguments(string url)
        {
            return "--force-ipv4 --retries 10";
        }

        public string PreferHlsFormat(string qualityArg)
        {
            if (qualityArg.Contains("--extract-audio", StringComparison.Ordinal))
                return "--format \"bestaudio[protocol^=m3u8]/bestaudio/best\" " +
                       qualityArg + " --postprocessor-args \"ExtractAudio:-threads 0\"";

            const string formatPrefix = "--format ";
            if (!qualityArg.StartsWith(formatPrefix, StringComparison.Ordinal))
                return qualityArg;

            var format = qualityArg[formatPrefix.Length..].Trim().Trim('"');
            var hlsFormat = format.Replace("bestvideo", "bestvideo[protocol^=m3u8]")
                .Replace("bestaudio", "bestaudio[protocol^=m3u8]")
                .Replace("best[", "best[protocol^=m3u8][");

            return $"--format \"{hlsFormat}/{format}\"";
        }

        public string GetPlatformName() => "YouTube";
    }
}