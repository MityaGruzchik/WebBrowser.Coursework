using System.Net.Http;
using System.Threading;

namespace WebBrowser.Coursework.Services
{
    public class RealPageLoader : IPageLoader
    {
        public string DownloadHtml(string url)
        {
            // Імітуємо "важку" операцію (затримка 3 секунди)
            Thread.Sleep(3000);

            // Реальне завантаження даних (синхронне для простоти прикладу патерну)
            using (var client = new HttpClient())
            {
                // Намагаємось завантажити, якщо URL некоректний - повертаємо заглушку
                try
                {
                    // У реальному житті краще використовувати await, 
                    // але патерн Proxy часто демонструють на синхронних методах.
                    var response = client.GetAsync(url).Result;
                    return response.Content.ReadAsStringAsync().Result;
                }
                catch
                {
                    return $"<html><body><h1>Error loading {url}</h1></body></html>";
                }
            }
        }
    }
}