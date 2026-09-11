using System;
using System.Net.Http;

namespace VideoDownloader.Services
{
    /// <summary>
    /// Provides a shared, properly-configured HttpClient instance.
    /// Avoids socket exhaustion by reusing a single static instance.
    /// </summary>
    public static class HttpClientFactory
    {
        private static readonly Lazy<HttpClient> _lazyClient = new(CreateClient);

        /// <summary>
        /// Shared HttpClient with timeout and user-agent headers configured.
        /// </summary>
        public static HttpClient Client => _lazyClient.Value;

        private static HttpClient CreateClient()
        {
            var handler = new HttpClientHandler
            {
                // Allow redirects (default)
                AllowAutoRedirect = true
            };

            var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(AppConstants.HttpClientTimeoutSeconds)
            };

            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36"
            );

            return client;
        }
    }
}
