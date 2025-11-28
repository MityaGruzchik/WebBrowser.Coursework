using System.Collections.Generic;

namespace WebBrowser.Coursework.Services
{
    public class SmartProxy : IPageLoader
    {
        // Посилання на реальний об'єкт (лінива ініціалізація)
        private RealPageLoader _realLoader;

        // Кеш: зберігає пару "URL -> HTML-код"
        private Dictionary<string, string> _cache = new Dictionary<string, string>();

        public string DownloadHtml(string url)
        {
            // 1. Перевірка кешу: якщо сторінка вже завантажувалася
            if (_cache.ContainsKey(url))
            {
                // Повертаємо дані миттєво, додаючи помітку для наочності
                return $"[FROM CACHE] {_cache[url]}";
            }

            // 2. Якщо реального об'єкта ще немає - створюємо його
            if (_realLoader == null)
            {
                _realLoader = new RealPageLoader();
            }

            // 3. Викликаємо реальний (повільний) метод
            string html = _realLoader.DownloadHtml(url);

            // 4. Зберігаємо результат у кеш
            _cache[url] = html;

            return $"[FROM INTERNET] {html}";
        }
    }
}