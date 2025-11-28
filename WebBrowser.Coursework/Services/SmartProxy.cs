using System.Collections.Generic;

namespace WebBrowser.Coursework.Services
{
    public class SmartProxy : IPageLoader
    {
        private RealPageLoader _realLoader;

        private Dictionary<string, string> _cache = new Dictionary<string, string>();

        public string DownloadHtml(string url)
        {
            if (_cache.ContainsKey(url))
            {
                return $"[FROM CACHE] {_cache[url]}";
            }

            if (_realLoader == null)
            {
                _realLoader = new RealPageLoader();
            }

            string html = _realLoader.DownloadHtml(url);

            _cache[url] = html;

            return $"[FROM INTERNET] {html}";
        }
    }
}