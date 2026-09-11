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
            // Do not force HLS protocol^=m3u8 for YouTube downloads.
            // Native formats download significantly faster and avoid throttling or stalling.
            return qualityArg;
        }

        public string GetPlatformName() => "YouTube";
    }
}
